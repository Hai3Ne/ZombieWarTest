using UnityEngine;

namespace ZombieWar.Levels
{
    [CreateAssetMenu(menuName = "Zombie War/Level Config", fileName = "LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private string _displayName = "Containment Yard";
        [SerializeField] private float _durationSeconds = 180f;
        [SerializeField] private int _startCount = 25;
        [SerializeField] private int _peakCount = 100;
        [SerializeField] private int _hardCap = 120;
        [SerializeField] private bool _spawnGiant;
        [SerializeField] private float _giantSpawnTime = 120f;
        #endregion

        public string DisplayName => _displayName;
        public float DurationSeconds => _durationSeconds;
        public int StartCount => _startCount;
        public int PeakCount => _peakCount;
        public int HardCap => _hardCap;
        public bool SpawnGiant => _spawnGiant;
        public float GiantSpawnTime => _giantSpawnTime;

        public void Configure(
            string displayName,
            float durationSeconds,
            int startCount,
            int peakCount,
            int hardCap,
            bool spawnGiant,
            float giantSpawnTime)
        {
            _displayName = displayName;
            _durationSeconds = durationSeconds;
            _startCount = startCount;
            _peakCount = peakCount;
            _hardCap = hardCap;
            _spawnGiant = spawnGiant;
            _giantSpawnTime = giantSpawnTime;
        }
    }
}
