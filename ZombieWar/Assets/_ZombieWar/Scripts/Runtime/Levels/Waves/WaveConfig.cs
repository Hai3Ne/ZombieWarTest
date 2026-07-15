using UnityEngine;
using ZombieWar.Enemies;

namespace ZombieWar.Levels
{
    [CreateAssetMenu(menuName = "Zombie War/Waves/Wave Config", fileName = "WaveConfig")]
    public sealed class WaveConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private string _displayName = "WAVE 1";
        [SerializeField, Min(1f)] private float _durationSeconds = 60f;
        [SerializeField, Min(0)] private int _startCount = 10;
        [SerializeField, Min(0)] private int _endCount = 24;
        [SerializeField, Range(1, 8)] private int _spawnPerFrame = 2;
        [SerializeField] private WaveEnemyEntry[] _enemies;
        #endregion

        public string DisplayName => _displayName;
        public float DurationSeconds => Mathf.Max(1f, _durationSeconds);
        public int StartCount => Mathf.Max(0, _startCount);
        public int EndCount => Mathf.Max(StartCount, _endCount);
        public int SpawnPerFrame => Mathf.Clamp(_spawnPerFrame, 1, 8);
        public WaveEnemyEntry[] Enemies => _enemies;

        public void Configure(
            string displayName,
            float durationSeconds,
            int startCount,
            int endCount,
            int spawnPerFrame,
            WaveEnemyEntry[] enemies)
        {
            _displayName = displayName;
            _durationSeconds = Mathf.Max(1f, durationSeconds);
            _startCount = Mathf.Max(0, startCount);
            _endCount = Mathf.Max(_startCount, endCount);
            _spawnPerFrame = Mathf.Clamp(spawnPerFrame, 1, 8);
            _enemies = enemies;
        }

        public int EvaluateTargetCount(float normalizedTime)
        {
            return Mathf.RoundToInt(Mathf.Lerp(StartCount, EndCount, Mathf.Clamp01(normalizedTime)));
        }

        public EnemyConfig SelectEnemy(float normalizedRoll)
        {
            return SelectEnemy(normalizedRoll, null);
        }

        public EnemyConfig SelectEnemy(float normalizedRoll, EnemyPool pool)
        {
            if (_enemies == null || _enemies.Length == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (IsAvailable(_enemies[i], pool))
                {
                    totalWeight += _enemies[i].Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float threshold = Mathf.Clamp01(normalizedRoll) * totalWeight;
            EnemyConfig fallback = null;
            for (int i = 0; i < _enemies.Length; i++)
            {
                WaveEnemyEntry entry = _enemies[i];
                if (!IsAvailable(entry, pool))
                {
                    continue;
                }

                fallback = entry.Enemy;
                threshold -= entry.Weight;
                if (threshold <= 0f)
                {
                    return entry.Enemy;
                }
            }

            return fallback;
        }

        private static bool IsAvailable(WaveEnemyEntry entry, EnemyPool pool)
        {
            return entry != null
                && entry.Enemy != null
                && (pool == null
                    || entry.MaxConcurrent <= 0
                    || pool.CountActive(entry.Enemy) < entry.MaxConcurrent);
        }
    }
}
