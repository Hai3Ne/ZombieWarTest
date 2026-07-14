using System.Collections.Generic;
using UnityEngine;
using ZombieWar.Enemies;

namespace ZombieWar.Combat
{
    public sealed class BombController : MonoBehaviour
    {
        #region Config
        [SerializeField] private BombProjectile _bombPrefab;
        [SerializeField] private int _capacity = 4;
        [SerializeField, Min(0.1f)] private float _cooldownDuration = 10f;
        [SerializeField, Min(1f)] private float _maximumThrowRange = 5f;
        [SerializeField, Min(0.1f)] private float _minimumThrowRange = 1f;
        [SerializeField, Min(0.1f)] private float _flightDuration = 0.75f;
        [SerializeField] private LineRenderer _trajectory;
        [SerializeField] private LineRenderer _rangeRing;
        #endregion

        #region Refs
        private readonly Queue<BombProjectile> _available = new(4);
        private EnemyPool _enemyPool;
        private WeaponController _weaponController;
        #endregion

        #region State
        private float _readyTime;

        public bool IsReady => Time.time >= _readyTime && _available.Count > 0;
        public float CooldownNormalized => Mathf.Clamp01((_readyTime - Time.time) / _cooldownDuration);
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_bombPrefab == null)
            {
                Debug.LogError("[Zombie War] BombController requires a bomb prefab.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _capacity; i++)
            {
                BombProjectile bomb = Instantiate(_bombPrefab, transform.parent);
                bomb.gameObject.name = $"Bomb_{i}";
                bomb.Initialize(this);
                bomb.gameObject.SetActive(false);
                _available.Enqueue(bomb);
            }
        }
        #endregion

        #region API
        public void SetPrefab(BombProjectile prefab, int capacity)
        {
            _bombPrefab = prefab;
            _capacity = Mathf.Max(1, capacity);
        }

        public void SetPreviewReferences(LineRenderer trajectory, LineRenderer rangeRing)
        {
            _trajectory = trajectory;
            _rangeRing = rangeRing;
            HidePreview();
        }

        public void Configure(EnemyPool enemyPool, WeaponController weaponController)
        {
            _enemyPool = enemyPool;
            _weaponController = weaponController;
        }

        public void PreviewAim(Vector2 input)
        {
            if (!IsReady || input.sqrMagnitude < 0.01f)
            {
                HidePreview();
                return;
            }

            Vector3 start = GetThrowOrigin();
            Vector3 target = GetTarget(input);
            Vector3 velocity = CalculateLaunchVelocity(start, target);
            _trajectory.enabled = true;
            _trajectory.positionCount = 24;
            for (int i = 0; i < _trajectory.positionCount; i++)
            {
                float normalized = i / (float)(_trajectory.positionCount - 1);
                float time = normalized * _flightDuration;
                _trajectory.SetPosition(i, start + velocity * time + Physics.gravity * (0.5f * time * time));
            }

            _rangeRing.enabled = true;
            _rangeRing.positionCount = 49;
            for (int i = 0; i < _rangeRing.positionCount; i++)
            {
                float angle = i / 48f * Mathf.PI * 2f;
                _rangeRing.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), 0.08f, Mathf.Sin(angle)) * _maximumThrowRange);
            }
        }

        public void CancelAim()
        {
            HidePreview();
        }

        public void ThrowBomb(Vector2 input)
        {
            HidePreview();
            if (!IsReady || input.sqrMagnitude < 0.04f)
            {
                return;
            }

            _readyTime = Time.time + _cooldownDuration;
            Vector3 start = GetThrowOrigin();
            Vector3 target = GetTarget(input);
            _available.Dequeue().Launch(start, CalculateLaunchVelocity(start, target), 1.5f);
        }

        public void Explode(BombProjectile bomb)
        {
            Vector3 center = bomb.transform.position;
            for (int i = _enemyPool.Active.Count - 1; i >= 0; i--)
            {
                ZombieAgent zombie = _enemyPool.Active[i];
                Vector3 offset = zombie.transform.position - center;
                float distance = offset.magnitude;
                if (!zombie.IsAlive || distance > 5.5f)
                {
                    continue;
                }
                float multiplier = Core.GameplayMath.CalculateDamageFalloff(distance, 5.5f, 0.35f);
                DamageInfo damage = new(95f * multiplier, zombie.transform.position, offset.normalized * 14f, gameObject, DamageType.Explosion);
                zombie.ApplyDamage(in damage);
                zombie.ApplyExplosion(center, 5.5f, 14f);
            }
            bomb.gameObject.SetActive(false);
            _available.Enqueue(bomb);
        }


        private Vector3 GetThrowOrigin()
        {
            return transform.position + Vector3.up * 1.2f;
        }

        private Vector3 GetTarget(Vector2 input)
        {
            float strength = Mathf.Clamp01(input.magnitude);
            float distance = Mathf.Lerp(_minimumThrowRange, _maximumThrowRange, strength);
            Vector2 normalized = input.normalized;
            return transform.position + new Vector3(normalized.x, 0f, normalized.y) * distance + Vector3.up * 0.12f;
        }

        private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 target)
        {
            return (target - start) / _flightDuration - Physics.gravity * (0.5f * _flightDuration);
        }

        private void HidePreview()
        {
            if (_trajectory != null)
            {
                _trajectory.enabled = false;
            }
            if (_rangeRing != null)
            {
                _rangeRing.enabled = false;
            }
        }
        #endregion
    }
}
