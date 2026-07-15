using UnityEngine;

namespace ZombieWar.Enemies
{
    [CreateAssetMenu(menuName = "Zombie War/Enemy Config", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private string _displayName = "Walker";
        [SerializeField] private EnemyArchetype _archetype = EnemyArchetype.Walker;
        [SerializeField] private float _maxHealth = 55f;
        [SerializeField] private float _moveSpeed = 2.4f;
        [SerializeField] private float _attackDamage = 8f;
        [SerializeField] private float _attackRange = 1.35f;
        [SerializeField] private float _attackInterval = 0.8f;
        [SerializeField] private float _scaleMultiplier = 1f;
        [SerializeField] private Color _tintColor = Color.white;
        #endregion

        public string DisplayName => _displayName;
        public EnemyArchetype Archetype => _archetype;
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackInterval => _attackInterval;
        public float ScaleMultiplier => _scaleMultiplier;
        public Color TintColor => _tintColor;
        public bool IsGiant => _archetype == EnemyArchetype.Giant;

        public void Configure(
            string displayName,
            EnemyArchetype archetype,
            float maxHealth,
            float moveSpeed,
            float attackDamage,
            float attackRange,
            float attackInterval,
            float scaleMultiplier,
            Color tintColor)
        {
            _displayName = displayName;
            _archetype = archetype;
            _maxHealth = maxHealth;
            _moveSpeed = moveSpeed;
            _attackDamage = attackDamage;
            _attackRange = attackRange;
            _attackInterval = attackInterval;
            _scaleMultiplier = Mathf.Max(0.25f, scaleMultiplier);
            _tintColor = tintColor;
        }
    }
}
