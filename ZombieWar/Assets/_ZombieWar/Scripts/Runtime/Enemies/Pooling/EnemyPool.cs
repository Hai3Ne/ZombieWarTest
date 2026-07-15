using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Audio;
using ZombieWar.Combat;
using ZombieWar.Core;
using ZombieWar.UI;

namespace ZombieWar.Enemies
{
    [RequireComponent(typeof(ZombieAudioService))]
    public sealed class EnemyPool : MonoBehaviour
    {
        #region Config
        [SerializeField] private int _capacity = 130;
        [SerializeField] private EnemyPrefabCatalog _catalog;
        #endregion

        #region Refs
        private ZombieAudioService _audio;
        [SerializeField] private FloatingCombatTextPool _combatTextPool;
        #endregion

        #region State
        private AsyncOperationHandle<GameObject> _prefabHandle;
        private ZombieAgent _prefab;
        private float _nextAmbientTime;
        private readonly Queue<ZombieAgent> _available = new(130);
        private readonly List<ZombieAgent> _active = new(130);
        public IReadOnlyList<ZombieAgent> Active => _active;
        public int ActiveCount => _active.Count;
        public bool IsReady { get; private set; }
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _audio)
                || _catalog == null
                || _combatTextPool == null
                || !_catalog.Zombie.RuntimeKeyIsValid())
            {
                Debug.LogError("[Zombie War] EnemyPool requires an authored enemy Addressables catalog.", this);
                enabled = false;
                return;
            }

            _prefabHandle = _catalog.Zombie.LoadAssetAsync();
            _prefabHandle.Completed += OnPrefabLoaded;
        }

        private void Update()
        {
            if (!IsReady
                || !_audio.IsReady
                || _active.Count == 0
                || Time.time < _nextAmbientTime)
            {
                return;
            }

            _nextAmbientTime = Time.time + Random.Range(1.1f, 1.9f);
            ZombieAgent zombie = _active[Random.Range(0, _active.Count)];
            _audio.PlayAmbient(zombie.transform.position, zombie.IsGiant);
        }

        private void OnDestroy()
        {
            if (!_prefabHandle.IsValid())
            {
                return;
            }

            _prefabHandle.Completed -= OnPrefabLoaded;
            Addressables.Release(_prefabHandle);
        }

        #endregion

        #region API
        public ZombieAgent Spawn(Vector3 position, EnemyConfig config, Transform target, Health targetHealth)
        {
            if (_available.Count == 0)
            {
                return null;
            }

            ZombieAgent zombie = _available.Dequeue();
            zombie.gameObject.SetActive(true);
            zombie.Spawn(position, config, target, targetHealth);
            _active.Add(zombie);
            return zombie;
        }

        public void SetCatalog(EnemyPrefabCatalog catalog, int capacity)
        {
            _catalog = catalog;
            _capacity = Mathf.Max(1, capacity);
        }

        public void SetCombatTextPool(FloatingCombatTextPool combatTextPool)
        {
            _combatTextPool = combatTextPool;
        }

        public void PlayAttackAudio(ZombieAgent zombie)
        {
            _audio.PlayAttack(zombie.transform.position, zombie.IsGiant);
        }

        public void PlayHitAudio(ZombieAgent zombie)
        {
            _audio.PlayHit(zombie.transform.position, zombie.IsGiant);
        }

        public void PlayDeathAudio(ZombieAgent zombie)
        {
            _audio.PlayDeath(zombie.transform.position, zombie.IsGiant);
        }

        public int CountActive(EnemyConfig config)
        {
            int count = 0;
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].IsAlive && _active[i].Config == config)
                {
                    count++;
                }
            }
            return count;
        }

        public void Release(ZombieAgent zombie)
        {
            int index = _active.IndexOf(zombie);
            if (index >= 0)
            {
                int last = _active.Count - 1;
                _active[index] = _active[last];
                _active.RemoveAt(last);
            }

            zombie.Deactivate();
            _available.Enqueue(zombie);
        }

        public void ReleaseAll()
        {
            while (_active.Count > 0)
            {
                Release(_active[_active.Count - 1]);
            }
        }

        public ZombieAgent FindBestTarget(
            Vector3 origin,
            ZombieAgent current,
            float range,
            float switchThreshold)
        {
            float rangeSquared = range * range;
            float bestDistance = current != null && current.IsAlive
                ? (current.AimPoint - origin).sqrMagnitude
                : float.MaxValue;
            ZombieAgent best = bestDistance <= rangeSquared ? current : null;

            for (int i = 0; i < _active.Count; i++)
            {
                ZombieAgent candidate = _active[i];
                if (!candidate.IsAlive)
                {
                    continue;
                }

                float distanceSquared = (candidate.AimPoint - origin).sqrMagnitude;
                float threshold = best == current ? switchThreshold : 1f;
                if (distanceSquared > rangeSquared
                    || !GameplayMath.ShouldSwitchTarget(bestDistance, distanceSquared, threshold))
                {
                    continue;
                }

                if (!HasLineOfSight(origin + Vector3.up, candidate))
                {
                    continue;
                }

                bestDistance = distanceSquared;
                best = candidate;
            }

            return best;
        }
        #endregion

        #region Internal
        private void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded
                || handle.Result == null
                || !handle.Result.TryGetComponent(out _prefab)
                || !_prefab.TryGetComponent(out FloatingCombatTextEmitter _))
            {
                Debug.LogError("[Zombie War] Addressable zombie prefab failed to load.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _capacity; i++)
            {
                CreateZombie(i);
            }
            IsReady = true;
        }

        private void CreateZombie(int index)
        {
            ZombieAgent zombie = Instantiate(_prefab, transform);
            GameObject instance = zombie.gameObject;
            instance.name = $"Zombie_{index:000}";
            zombie.Initialize(this, index);
            if (zombie.TryGetComponent(out FloatingCombatTextEmitter emitter))
            {
                emitter.SetPool(_combatTextPool);
            }
            instance.SetActive(false);
            _available.Enqueue(zombie);
        }

        private static bool HasLineOfSight(Vector3 origin, ZombieAgent target)
        {
            Vector3 direction = target.AimPoint - origin;
            if (!Physics.Raycast(
                    origin,
                    direction.normalized,
                    out RaycastHit hit,
                    direction.magnitude,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.collider != null && hit.collider.gameObject == target.gameObject;
        }
        #endregion
    }
}
