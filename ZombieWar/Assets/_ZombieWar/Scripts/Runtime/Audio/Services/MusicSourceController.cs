using UnityEngine;
using ZombieWar.Core;

namespace ZombieWar.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class MusicSourceController : MonoBehaviour
    {
        #region Refs
        [SerializeField] private AudioSource _source;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_source == null && !TryGetComponent(out _source))
            {
                Debug.LogError("[Zombie War] MusicSourceController requires an AudioSource.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            GameOptions.Changed += ApplySetting;
            ApplySetting();
        }

        private void OnDisable()
        {
            GameOptions.Changed -= ApplySetting;
        }
        #endregion

        #region API
        public void SetSource(AudioSource source)
        {
            _source = source;
        }
        #endregion

        #region Internal
        private void ApplySetting()
        {
            if (_source != null)
            {
                _source.mute = !GameOptions.MusicEnabled;
            }
        }
        #endregion
    }
}
