using UnityEngine;

namespace ZombieWar.Enemies
{
    [CreateAssetMenu(menuName = "Zombie War/Enemy Config", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private float _maxHealth = 55f;
        [SerializeField] private float _moveSpeed = 2.4f;
        [SerializeField] private float _attackDamage = 8f;
        [SerializeField] private float _attackRange = 1.35f;
        [SerializeField] private float _attackInterval = 0.8f;
        [SerializeField] private bool _isGiant;
        #endregion

        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackInterval => _attackInterval;
        public bool IsGiant => _isGiant;

        public void Configure(
            float maxHealth,
            float moveSpeed,
            float attackDamage,
            float attackRange,
            float attackInterval,
            bool isGiant)
        {
            _maxHealth = maxHealth;
            _moveSpeed = moveSpeed;
            _attackDamage = attackDamage;
            _attackRange = attackRange;
            _attackInterval = attackInterval;
            _isGiant = isGiant;
        }
    }
}
