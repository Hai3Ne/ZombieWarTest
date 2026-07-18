using UnityEngine;
using UnityEngine.AI;
using ZombieWar.Combat;

namespace ZombieWar.Enemies
{
    public enum ZombieState
    {
        Inactive,
        Spawn,
        Chase,
        Attack,
        Hit,
        Knockback,
        Dead
    }

    [RequireComponent(typeof(Health), typeof(Rigidbody), typeof(NavMeshAgent))]
    [RequireComponent(typeof(ZombieVisualController), typeof(ZombieAnimationController))]
    public sealed class ZombieAgent : MonoBehaviour
    {
        #region Refs
        private Health _health;
        private Rigidbody _rigidbody;
        private NavMeshAgent _agent;
        private Collider _collider;
        private ZombieVisualController _visual;
        private ZombieAnimationController _animation;
        private EnemyPool _pool;
        private Transform _target;
        private Health _targetHealth;
        #endregion

        #region State
        private EnemyConfig _config;
        private ZombieState _state;
        private float _nextAttackTime;
        private float _hitFlashUntil;
        private float _knockbackUntil;
        private float _deathProgress;
        private Vector3 _spawnScale;
        public bool IsAlive => _state is not ZombieState.Inactive and not ZombieState.Dead;
        public bool IsGiant => _config != null && _config.IsGiant;
        public int SimulationSlot { get; private set; }
        public Vector3 AimPoint => transform.position + Vector3.up * (IsGiant ? 1.8f : 0.8f);
        public ZombieState State => _state;
        public EnemyConfig Config => _config;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _health)
                || !TryGetComponent(out _rigidbody)
                || !TryGetComponent(out _agent)
                || !TryGetComponent(out _collider)
                || !TryGetComponent(out _visual)
                || !TryGetComponent(out _animation))
            {
                Debug.LogError("[Zombie War] ZombieAgent is missing a required component.", this);
                enabled = false;
                return;
            }

            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
            _spawnScale = transform.localScale;
        }

        private void OnDestroy()
        {
            if (_health == null)
            {
                return;
            }

            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }
        #endregion

        #region API
        public void Initialize(EnemyPool pool, int simulationSlot)
        {
            _pool = pool;
            SimulationSlot = simulationSlot;
        }

        public void Spawn(Vector3 position, EnemyConfig config, Transform target, Health targetHealth)
        {
            _config = config;
            _target = target;
            _targetHealth = targetHealth;
            _state = ZombieState.Spawn;
            _deathProgress = 0f;
            _nextAttackTime = 0f;

            transform.position = position;
            transform.localScale = _spawnScale * config.ScaleMultiplier;
            _collider.enabled = true;
            _rigidbody.isKinematic = true;
            _health.Configure(config.MaxHealth);

            _agent.enabled = true;
            _agent.speed = config.MoveSpeed;
            _agent.angularSpeed = 540f;
            _agent.acceleration = 18f;
            _agent.radius = config.IsGiant ? 0.8f : 0.32f;
            _agent.height = config.IsGiant ? 3.6f : 1.8f;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

            _animation.ResetForSpawn();
            SetVisual(0f, 0f);
            _state = ZombieState.Chase;
        }

        public void Simulate(float deltaTime, bool allowAvoidance)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_state == ZombieState.Dead)
            {
                const float fallSeconds = 0.6f;
                const float dissolveSeconds = 0.5f;
                _deathProgress += deltaTime / (fallSeconds + dissolveSeconds);
                _animation.SetMoving(false);
                float elapsed = _deathProgress * (fallSeconds + dissolveSeconds);
                SetVisual(0f, Mathf.Clamp01((elapsed - fallSeconds) / dissolveSeconds));

                if (_deathProgress >= 1f)
                {
                    _pool.Release(this);
                }

                return;
            }

            if (!IsAlive || _target == null || _targetHealth == null || _targetHealth.IsDead)
            {
                return;
            }

            SetVisual(Time.time < _hitFlashUntil ? 1f : 0f, 0f);

            if (_state == ZombieState.Knockback)
            {
                _animation.SetMoving(false);
                if (Time.time >= _knockbackUntil)
                {
                    RecoverFromKnockback();
                }
                return;
            }

            Vector3 offset = _target.position - transform.position;
            offset.y = 0f;
            float distanceSquared = offset.sqrMagnitude;
            float attackRangeSquared = _config.AttackRange * _config.AttackRange;

            if (distanceSquared <= attackRangeSquared)
            {
                _state = ZombieState.Attack;
                _animation.SetMoving(false);
                Attack();
                return;
            }

            _state = ZombieState.Chase;
            _animation.SetMoving(true);
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.obstacleAvoidanceType = allowAvoidance
                    ? ObstacleAvoidanceType.LowQualityObstacleAvoidance
                    : ObstacleAvoidanceType.NoObstacleAvoidance;
                _agent.SetDestination(_target.position);
                return;
            }

            if (offset.sqrMagnitude > 0.001f)
            {
                Vector3 direction = offset.normalized;
                transform.position += direction * (_config.MoveSpeed * deltaTime);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    12f * deltaTime);
            }
        }

        public void ApplyDamage(in DamageInfo damage)
        {
            _health.ApplyDamage(in damage);
        }

        public void ApplyExplosion(Vector3 center, float radius, float force)
        {
            if (_state == ZombieState.Dead)
            {
                return;
            }

            if (_agent.enabled)
            {
                _agent.enabled = false;
            }

            _state = ZombieState.Knockback;
            _animation.SetMoving(false);
            _animation.PlayHit();
            _rigidbody.isKinematic = false;
            _rigidbody.AddExplosionForce(force, center, radius, 1.5f, ForceMode.VelocityChange);
            _knockbackUntil = Time.time + 0.35f;
        }

        public void Deactivate()
        {
            _state = ZombieState.Inactive;
            if (_agent.enabled)
            {
                _agent.enabled = false;
            }
            _rigidbody.isKinematic = true;
            _animation.SetMoving(false);
            gameObject.SetActive(false);
        }
        #endregion

        #region Internal
        private void Attack()
        {
            if (Time.time < _nextAttackTime)
            {
                return;
            }

            _nextAttackTime = Time.time + _config.AttackInterval;
            _animation.PlayAttack();
            _pool.PlayAttackAudio(this);
            Vector3 impulse = (_target.position - transform.position).normalized * 1.5f;
            DamageInfo damage = new(
                _config.AttackDamage,
                _target.position,
                impulse,
                gameObject,
                DamageType.Melee);
            _targetHealth.ApplyDamage(in damage);
        }

        private void OnDamaged(DamageInfo damage)
        {
            if (_health.CurrentHealth <= 0f)
            {
                return;
            }

            _hitFlashUntil = Time.time + 0.08f;
            _animation.PlayHit();
            _pool.PlayHitAudio(this);
            if (_state != ZombieState.Knockback)
            {
                _state = ZombieState.Hit;
            }
        }

        private void OnDied()
        {
            _state = ZombieState.Dead;
            _deathProgress = 0f;
            _animation.PlayDeath();
            _pool.PlayDeathAudio(this);
            _collider.enabled = false;
            if (_agent.enabled)
            {
                _agent.enabled = false;
            }
            _rigidbody.isKinematic = true;
        }

        private void RecoverFromKnockback()
        {
            _rigidbody.isKinematic = true;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                _agent.enabled = true;
                _agent.Warp(hit.position);
            }

            _state = ZombieState.Chase;
        }

        private void SetVisual(float hitFlash, float dissolve)
        {
            _visual.SetState(_config != null ? _config.TintColor : Color.white, hitFlash, dissolve);
        }
        #endregion
    }
}
