using System;
using UnityEngine;

namespace ZombieWar.Levels
{
    [Serializable]
    public sealed class LevelDefinition
    {
        #region Config
        [SerializeField] private string _displayName = "NEW AREA";
        [SerializeField] private string _sceneName;
        [SerializeField] private WaveSequenceConfig _waveSequence;
        [SerializeField] private bool _enabled = true;
        #endregion

        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public WaveSequenceConfig WaveSequence => _waveSequence;
        public bool Enabled => _enabled;
        public bool IsValid => _enabled && !string.IsNullOrWhiteSpace(_sceneName) && _waveSequence != null;

        public void Configure(string displayName, string sceneName, WaveSequenceConfig waveSequence, bool enabled)
        {
            _displayName = displayName;
            _sceneName = sceneName;
            _waveSequence = waveSequence;
            _enabled = enabled;
        }
    }
}
