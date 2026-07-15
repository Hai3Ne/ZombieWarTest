using System;
using UnityEngine;
using ZombieWar.Enemies;

namespace ZombieWar.Levels
{
    [Serializable]
    public sealed class WaveEnemyEntry
    {
        #region Config
        [SerializeField] private EnemyConfig _enemy;
        [SerializeField, Min(0.01f)] private float _weight = 1f;
        [SerializeField, Min(0)] private int _maxConcurrent;
        #endregion

        public EnemyConfig Enemy => _enemy;
        public float Weight => Mathf.Max(0f, _weight);
        public int MaxConcurrent => Mathf.Max(0, _maxConcurrent);

        public void Configure(EnemyConfig enemy, float weight, int maxConcurrent = 0)
        {
            _enemy = enemy;
            _weight = Mathf.Max(0.01f, weight);
            _maxConcurrent = Mathf.Max(0, maxConcurrent);
        }
    }
}
