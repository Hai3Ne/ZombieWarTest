using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.UI
{
    [RequireComponent(typeof(Health))]
    public sealed class FloatingCombatTextEmitter : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0f)] private float _height = 2f;
        [SerializeField] private bool _isPlayer;
        [SerializeField] private bool _useDamagePoint = true;
        #endregion

        #region Refs
        [SerializeField] private FloatingCombatTextPool _pool;
        private Health _health;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _health))
            {
                Debug.LogError("[Zombie War] FloatingCombatTextEmitter requires Health.", this);
                enabled = false;
                return;
            }
            _health.Damaged += OnDamaged;
            _health.Healed += OnHealed;
        }

        private void OnDestroy()
        {
            if (_health == null)
            {
                return;
            }
            _health.Damaged -= OnDamaged;
            _health.Healed -= OnHealed;
        }
        #endregion

        #region API
        public void SetPool(FloatingCombatTextPool pool)
        {
            _pool = pool;
        }

        public void Configure(bool isPlayer, float height, bool useDamagePoint)
        {
            _isPlayer = isPlayer;
            _height = Mathf.Max(0f, height);
            _useDamagePoint = useDamagePoint;
        }
        #endregion

        #region Internal
        private void OnDamaged(DamageInfo damage)
        {
            if (_pool == null)
            {
                return;
            }
            float scale = Mathf.Max(1f, transform.localScale.y);
            Vector3 position = transform.position + Vector3.up * (_height * scale);
            if (_useDamagePoint)
            {
                position.x = damage.Point.x;
                position.z = damage.Point.z;
            }
            _pool.ShowDamage(damage.Amount, position, _isPlayer);
        }

        private void OnHealed(float amount)
        {
            if (_pool != null)
            {
                float scale = Mathf.Max(1f, transform.localScale.y);
                _pool.ShowHealing(amount, transform.position + Vector3.up * (_height * scale));
            }
        }
        #endregion
    }
}
