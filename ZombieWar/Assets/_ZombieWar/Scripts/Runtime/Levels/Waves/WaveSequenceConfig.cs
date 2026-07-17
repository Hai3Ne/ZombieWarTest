using UnityEngine;
using ZombieWar.Enemies;

namespace ZombieWar.Levels
{
    [CreateAssetMenu(menuName = "Zombie War/Waves/Wave Sequence", fileName = "WaveSequence")]
    public sealed class WaveSequenceConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private string _displayName = "Level Waves";
        [SerializeField, Min(1)] private int _hardCap = 120;
        [SerializeField] private CameraProfileConfig _cameraProfile;
        [SerializeField] private WaveConfig[] _waves;
        [SerializeField] private EnemyConfig _bossEnemy;
        #endregion

        public string DisplayName => _displayName;
        public int HardCap => Mathf.Max(1, _hardCap);
        public CameraProfileConfig CameraProfile => _cameraProfile;
        public WaveConfig[] Waves => _waves;
        public EnemyConfig BossEnemy => _bossEnemy;
        public int WaveCount => _waves?.Length ?? 0;

        public float TotalDuration
        {
            get
            {
                float duration = 0f;
                if (_waves == null)
                {
                    return duration;
                }
                for (int i = 0; i < _waves.Length; i++)
                {
                    if (_waves[i] != null)
                    {
                        duration += _waves[i].DurationSeconds;
                    }
                }
                return duration;
            }
        }

        public void Configure(
            string displayName,
            int hardCap,
            CameraProfileConfig cameraProfile,
            WaveConfig[] waves,
            EnemyConfig bossEnemy = null)
        {
            _displayName = displayName;
            _hardCap = Mathf.Max(1, hardCap);
            _cameraProfile = cameraProfile;
            _waves = waves;
            _bossEnemy = bossEnemy;
        }

        public WaveConfig GetWave(int index)
        {
            return _waves != null && index >= 0 && index < _waves.Length ? _waves[index] : null;
        }

        public WaveConfig GetWaveAtTime(float elapsed, out int waveIndex, out float normalizedWaveTime)
        {
            waveIndex = -1;
            normalizedWaveTime = 0f;
            if (_waves == null || _waves.Length == 0)
            {
                return null;
            }

            float cursor = Mathf.Max(0f, elapsed);
            for (int i = 0; i < _waves.Length; i++)
            {
                WaveConfig wave = _waves[i];
                if (wave == null)
                {
                    continue;
                }

                if (cursor < wave.DurationSeconds || i == _waves.Length - 1)
                {
                    waveIndex = i;
                    normalizedWaveTime = Mathf.Clamp01(cursor / wave.DurationSeconds);
                    return wave;
                }
                cursor -= wave.DurationSeconds;
            }

            return null;
        }
    }
}
