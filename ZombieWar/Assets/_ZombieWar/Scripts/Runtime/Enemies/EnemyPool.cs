using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ZombieWar.Combat;
using ZombieWar.Core;

namespace ZombieWar.Enemies
{
    public sealed class EnemyPool : MonoBehaviour
    {
        #region Config
        [SerializeField] private int _capacity = 130;
        [SerializeField] private ZombieAgent _prefab;
        #endregion

        #region State
        private readonly Queue<ZombieAgent> _available = new(130);
        private readonly List<ZombieAgent> _active = new(130);
        public IReadOnlyList<ZombieAgent> Active => _active;
        public int ActiveCount => _active.Count;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogError("[Zombie War] EnemyPool requires a zombie prefab.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _capacity; i++)
            {
                CreateZombie(i);
            }
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

        public void SetPrefab(ZombieAgent prefab, int capacity)
        {
            _prefab = prefab;
            _capacity = Mathf.Max(1, capacity);
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
        private void CreateZombie(int index)
        {
            ZombieAgent zombie = Instantiate(_prefab, transform);
            GameObject instance = zombie.gameObject;
            instance.name = $"Zombie_{index:000}";
            zombie.Initialize(this, index);
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
