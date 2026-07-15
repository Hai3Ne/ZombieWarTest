using System;
using UnityEngine;

namespace ZombieWar.Combat
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;
        private bool _isDead;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float Normalized => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;
        public bool IsDead => _isDead;

        public event Action<float> Changed;
        public event Action<DamageInfo> Damaged;
        public event Action<float> Healed;
        public event Action Died;

        private void Awake()
        {
            ResetHealth();
        }

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

        public float Heal(float amount)
        {
            if (_isDead || amount <= 0f || _currentHealth >= _maxHealth)
            {
                return 0f;
            }

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            float restoredAmount = _currentHealth - previousHealth;
            Healed?.Invoke(restoredAmount);
            Changed?.Invoke(Normalized);
            return restoredAmount;
        }
    }
}
