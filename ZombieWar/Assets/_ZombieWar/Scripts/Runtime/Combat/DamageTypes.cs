using System;
using UnityEngine;
using ZombieWar.Core;

namespace ZombieWar.Combat
{
    public enum DamageType
    {
        Bullet,
        Explosion,
        Melee
    }

    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, Vector3 point, Vector3 impulse, GameObject instigator, DamageType type)
        {
            Amount = amount;
            Point = point;
            Impulse = impulse;
            Instigator = instigator;
            Type = type;
        }

        public float Amount { get; }
        public Vector3 Point { get; }
        public Vector3 Impulse { get; }
        public GameObject Instigator { get; }
        public DamageType Type { get; }
    }

    public interface IDamageable
    {
        void ApplyDamage(in DamageInfo damage);
    }

    public sealed class Health : MonoBehaviour, IDamageable
    {
        #region Config
        [SerializeField] private float _maxHealth = 100f;
        #endregion

        #region State
        private float _currentHealth;
        private bool _isDead;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float Normalized => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;
        public bool IsDead => _isDead;
        #endregion

        public event Action<float> Changed;
        public event Action<DamageInfo> Damaged;
        public event Action Died;

        #region Lifecycle
        private void Awake()
        {
            ResetHealth();
        }
        #endregion

        #region API
        public void Configure(float maxHealth)
        {
            _maxHealth = Mathf.Max(1f, maxHealth);
            ResetHealth();
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            Changed?.Invoke(Normalized);
        }

        public void ApplyDamage(in DamageInfo damage)
        {
            if (_isDead || damage.Amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - damage.Amount);
            Damaged?.Invoke(damage);
            Changed?.Invoke(Normalized);

            if (_currentHealth > 0f)
            {
                return;
            }

            _isDead = true;
            Died?.Invoke();
        }
        #endregion
    }
}

namespace ZombieWar.Combat
{
    using System.Collections.Generic;
    using ZombieWar.Enemies;

    public sealed class ProjectilePool : MonoBehaviour
    {
        #region Config
        [SerializeField] private Projectile _prefab;
        [SerializeField] private int _capacity = 96;
        #endregion

        #region State
        private readonly Queue<Projectile> _available = new(96);
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogError("[Zombie War] ProjectilePool requires a projectile prefab.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _capacity; i++)
            {
                Projectile projectile = Instantiate(_prefab, transform);
                GameObject instance = projectile.gameObject;
                instance.name = $"Projectile_{i:000}";
                projectile.Initialize(this);
                instance.SetActive(false);
                _available.Enqueue(projectile);
            }
        }
        #endregion

        #region API
        public void SetPrefab(Projectile prefab, int capacity)
        {
            _prefab = prefab;
            _capacity = Mathf.Max(1, capacity);
        }

        public void Fire(Vector3 origin, Vector3 direction, float speed, float range, float damage, GameObject instigator, Color color)
        {
            if (_available.Count == 0)
            {
                return;
            }
            _available.Dequeue().Launch(origin, direction, speed, range, damage, instigator, color);
        }

        public void Release(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
            _available.Enqueue(projectile);
        }
        #endregion
    }

    [RequireComponent(typeof(MeshRenderer))]
    public sealed class Projectile : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        #region Refs
        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private ProjectilePool _pool;
        private MeshRenderer _renderer;
        #endregion

        #region State
        private MaterialPropertyBlock _propertyBlock;
        private Vector3 _direction;
        private float _speed;
        private float _remainingRange;
        private float _damage;
        private GameObject _instigator;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            if (!TryGetComponent(out _renderer))
            {
                Debug.LogError("[Zombie War] Projectile requires a MeshRenderer.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            float distance = Mathf.Min(_remainingRange, _speed * Time.deltaTime);
            int count = Physics.SphereCastNonAlloc(
                transform.position,
                0.12f,
                _direction,
                _hits,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider hit = _hits[i].collider;
                if (hit == null || hit.gameObject == _instigator)
                {
                    continue;
                }
                if (hit.TryGetComponent(out Health health))
                {
                    DamageInfo damage = new(_damage, _hits[i].point, _direction * 2f, _instigator, DamageType.Bullet);
                    health.ApplyDamage(in damage);
                    _pool.Release(this);
                    return;
                }
                if (!hit.CompareTag("Player"))
                {
                    _pool.Release(this);
                    return;
                }
            }

            transform.position += _direction * distance;
            _remainingRange -= distance;
            if (_remainingRange <= 0f)
            {
                _pool.Release(this);
            }
        }
        #endregion

        #region API
        public void Initialize(ProjectilePool pool)
        {
            _pool = pool;
        }

        public void Launch(Vector3 origin, Vector3 direction, float speed, float range, float damage, GameObject instigator, Color color)
        {
            transform.position = origin;
            _direction = direction.normalized;
            _speed = speed;
            _remainingRange = range;
            _damage = damage;
            _instigator = instigator;
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_propertyBlock);
            gameObject.SetActive(true);
        }
        #endregion
    }

    public sealed class WeaponController : MonoBehaviour
    {
        private EnemyPool _enemyPool;
        private ProjectilePool _projectilePool;
        private Transform _muzzle;
        private WeaponConfig[] _weapons;
        private ZombieAgent _target;
        private int _weaponIndex;
        private float _nextTargetRefresh;
        private float _nextFireTime;

        public string CurrentWeaponName => _weapons != null ? _weapons[_weaponIndex].DisplayName : string.Empty;
        public ZombieAgent CurrentTarget => _target;
        public event Action<string> WeaponChanged;
        public event Action<float> Fired;

        private void Update()
        {
            if (_weapons == null || _enemyPool == null)
            {
                return;
            }

            WeaponConfig weapon = _weapons[_weaponIndex];
            if (Time.time >= _nextTargetRefresh)
            {
                _nextTargetRefresh = Time.time + 0.15f;
                _target = _enemyPool.FindBestTarget(transform.position, _target, weapon.Range, 0.8f);
            }
            if (_target == null || !_target.IsAlive)
            {
                return;
            }

            Vector3 targetPoint = _target.AimPoint;
            Vector3 flat = targetPoint - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 18f * Time.deltaTime);
            }
            if (GameplayMath.IsCooldownReady(Time.time, _nextFireTime))
            {
                Fire(weapon, targetPoint);
            }
        }

        public void Configure(WeaponConfig[] weapons, EnemyPool enemyPool, ProjectilePool projectilePool, Transform muzzle)
        {
            _weapons = weapons;
            _enemyPool = enemyPool;
            _projectilePool = projectilePool;
            _muzzle = muzzle;
            WeaponChanged?.Invoke(CurrentWeaponName);
        }

        public void SwitchWeapon()
        {
            if (_weapons == null || _weapons.Length < 2)
            {
                return;
            }
            _weaponIndex = (_weaponIndex + 1) % _weapons.Length;
            _nextFireTime = Time.time + 0.1f;
            WeaponChanged?.Invoke(CurrentWeaponName);
        }

        private void Fire(WeaponConfig weapon, Vector3 targetPoint)
        {
            _nextFireTime = Time.time + weapon.FireInterval;
            Vector3 baseDirection = (targetPoint - _muzzle.position).normalized;
            for (int i = 0; i < weapon.PelletCount; i++)
            {
                float yaw = UnityEngine.Random.Range(-weapon.SpreadDegrees, weapon.SpreadDegrees);
                float pitch = UnityEngine.Random.Range(-weapon.SpreadDegrees * 0.35f, weapon.SpreadDegrees * 0.35f);
                Vector3 direction = Quaternion.Euler(pitch, yaw, 0f) * baseDirection;
                _projectilePool.Fire(_muzzle.position, direction, weapon.ProjectileSpeed, weapon.Range, weapon.Damage, gameObject, weapon.AccentColor);
            }
            Fired?.Invoke(weapon.Recoil);
        }
    }

    public sealed class BombController : MonoBehaviour
    {
        #region Config
        [SerializeField] private BombProjectile _bombPrefab;
        [SerializeField] private int _capacity = 4;
        #endregion

        #region Refs
        private readonly Queue<BombProjectile> _available = new(4);
        private EnemyPool _enemyPool;
        private WeaponController _weaponController;
        #endregion

        #region State
        private float _readyTime;
        public float CooldownNormalized => Mathf.Clamp01((_readyTime - Time.time) / 8f);
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
                GameObject instance = bomb.gameObject;
                instance.name = $"Bomb_{i}";
                bomb.Initialize(this);
                instance.SetActive(false);
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

        public void Configure(EnemyPool enemyPool, WeaponController weaponController)
        {
            _enemyPool = enemyPool;
            _weaponController = weaponController;
        }

        public void ThrowBomb()
        {
            if (Time.time < _readyTime || _available.Count == 0)
            {
                return;
            }
            _readyTime = Time.time + 8f;
            Vector3 direction = transform.forward;
            if (_weaponController.CurrentTarget != null)
            {
                direction = _weaponController.CurrentTarget.transform.position - transform.position;
                direction.y = 0f;
                direction.Normalize();
            }
            _available.Dequeue().Launch(transform.position + Vector3.up * 1.2f, direction * 10f + Vector3.up * 5f, 1.5f);
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
        #endregion
    }

    public sealed class BombProjectile : MonoBehaviour
    {
        private BombController _owner;
        private Rigidbody _rigidbody;
        private float _explodeAt;

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody))
            {
                Debug.LogError("[Zombie War] BombProjectile requires a Rigidbody.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (Time.time >= _explodeAt)
            {
                _owner.Explode(this);
            }
        }
        #endregion

        #region API
        public void Initialize(BombController owner)
        {
            _owner = owner;
        }

        public void Launch(Vector3 position, Vector3 velocity, float fuseSeconds)
        {
            transform.position = position;
            gameObject.SetActive(true);
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = new Vector3(3f, 6f, 2f);
            _explodeAt = Time.time + fuseSeconds;
        }
        #endregion
    }
}
