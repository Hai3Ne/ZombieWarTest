using System.Collections.Generic;
using System;
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
        [SerializeField, Min(1)] private int _maximumBombs = 3;
        [SerializeField, Min(0)] private int _startingBombs = 3;
        [SerializeField, Min(0.1f)] private float _blastRadius = 5.5f;
        [SerializeField] private LineRenderer _trajectory;
        [SerializeField] private LineRenderer _rangeRing;
        [SerializeField] private LineRenderer _blastRadiusRing;
        [SerializeField] private GameObject _targetIndicator;
        [SerializeField] private BombExplosionVfxPool _explosionVfxPool;
        #endregion

        #region Refs
        private readonly Queue<BombProjectile> _available = new(4);
        private EnemyPool _enemyPool;
        #endregion

        #region State
        private float _readyTime;
        private int _bombCount;

        public bool IsReady => Time.time >= _readyTime && _available.Count > 0 && _bombCount > 0;
        public float CooldownNormalized => Mathf.Clamp01((_readyTime - Time.time) / _cooldownDuration);
        public int BombCount => _bombCount;
        public int MaxBombs => _maximumBombs;
        public event Action<int, int> BombCountChanged;
        public event Action<Vector3, float> Exploded;
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
            _bombCount = Mathf.Clamp(_startingBombs, 0, _maximumBombs);
        }
        #endregion

        #region API
        public void SetPrefab(BombProjectile prefab, int capacity)
        {
            _bombPrefab = prefab;
            _capacity = Mathf.Max(1, capacity);
        }

        public void SetInventory(int maximumBombs, int startingBombs)
        {
            _maximumBombs = Mathf.Max(1, maximumBombs);
            _startingBombs = Mathf.Clamp(startingBombs, 0, _maximumBombs);
            _bombCount = _startingBombs;
        }

        public void SetPreviewReferences(LineRenderer trajectory, LineRenderer rangeRing, LineRenderer blastRadiusRing)
        {
            _trajectory = trajectory;
            _rangeRing = rangeRing;
            _blastRadiusRing = blastRadiusRing;
            HidePreview();
        }

        public void SetExplosionVfxPool(BombExplosionVfxPool explosionVfxPool)
        {
            _explosionVfxPool = explosionVfxPool;
        }

        public void SetTargetIndicator(GameObject targetIndicator)
        {
            _targetIndicator = targetIndicator;
            if (_targetIndicator != null)
            {
                _targetIndicator.SetActive(false);
            }
        }

        public void Configure(EnemyPool enemyPool)
        {
            _enemyPool = enemyPool;
        }

        public bool TryAddBomb(int amount = 1)
        {
            if (amount <= 0 || _bombCount >= _maximumBombs)
            {
                return false;
            }

            _bombCount = Mathf.Min(_maximumBombs, _bombCount + amount);
            BombCountChanged?.Invoke(_bombCount, _maximumBombs);
            return true;
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

            ShowTargetIndicator(target);
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
            _bombCount--;
            BombCountChanged?.Invoke(_bombCount, _maximumBombs);
            Vector3 start = GetThrowOrigin();
            Vector3 target = GetTarget(input);
            _available.Dequeue().Launch(start, CalculateLaunchVelocity(start, target), 1.5f);
        }

        public void Explode(BombProjectile bomb)
        {
            Vector3 center = bomb.transform.position;
            _explosionVfxPool.Play(center);
            Exploded?.Invoke(center, _blastRadius);
            for (int i = _enemyPool.Active.Count - 1; i >= 0; i--)
            {
                ZombieAgent zombie = _enemyPool.Active[i];
                Vector3 offset = zombie.transform.position - center;
                float distance = offset.magnitude;
                if (!zombie.IsAlive || distance > _blastRadius)
                {
                    continue;
                }
                float multiplier = Core.GameplayMath.CalculateDamageFalloff(distance, _blastRadius, 0.35f);
                DamageInfo damage = new(95f * multiplier, zombie.transform.position, offset.normalized * 14f, gameObject, DamageType.Explosion);
                zombie.ApplyDamage(in damage);
                zombie.ApplyExplosion(center, _blastRadius, 14f);
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
            if (_blastRadiusRing != null)
            {
                _blastRadiusRing.enabled = false;
            }
            if (_targetIndicator != null)
            {
                _targetIndicator.SetActive(false);
            }
        }

        private void ShowTargetIndicator(Vector3 target)
        {
            if (_targetIndicator == null)
            {
                return;
            }

            target.y = 0.12f;
            _targetIndicator.transform.SetPositionAndRotation(target, Quaternion.Euler(-90f, 0f, 0f));
            _targetIndicator.transform.localScale = Vector3.one * (_blastRadius * 0.5f);
            _targetIndicator.SetActive(true);
        }
        #endregion
    }
}
