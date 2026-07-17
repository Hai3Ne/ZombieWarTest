using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.Audio
{
    public sealed class MusicDuckController : MonoBehaviour
    {
        #region Config
        [SerializeField] private float _fireDuckMultiplier = 0.55f;
        [SerializeField] private float _explosionDuckMultiplier = 0.3f;
        [SerializeField] private float _fireHoldSeconds = 0.25f;
        [SerializeField] private float _explosionHoldSeconds = 0.7f;
        [SerializeField] private float _attackSeconds = 0.05f;
        [SerializeField] private float _releaseSeconds = 0.9f;
        #endregion

        #region Refs
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private BombController _bombController;
        #endregion

        #region State
        private float _baseVolume;
        private float _currentMultiplier = 1f;
        private float _duckMultiplier = 1f;
        private float _holdUntil;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_musicSource == null || _weaponController == null || _bombController == null)
            {
                Debug.LogError("[Zombie War] MusicDuckController requires authored references.", this);
                enabled = false;
                return;
            }

            _baseVolume = _musicSource.volume;
            _weaponController.Fired += OnFired;
            _bombController.Exploded += OnExploded;
        }

        private void OnDestroy()
        {
            if (_weaponController != null)
            {
                _weaponController.Fired -= OnFired;
            }

            if (_bombController != null)
            {
                _bombController.Exploded -= OnExploded;
            }
        }

        private void Update()
        {
            float target = Time.unscaledTime < _holdUntil ? _duckMultiplier : 1f;
            float speed = target < _currentMultiplier
                ? 1f / Mathf.Max(_attackSeconds, 0.01f)
                : 1f / Mathf.Max(_releaseSeconds, 0.01f);
            _currentMultiplier = Mathf.MoveTowards(_currentMultiplier, target, speed * Time.unscaledDeltaTime);
            _musicSource.volume = _baseVolume * _currentMultiplier;
        }
        #endregion

        #region API
        public void SetReferences(
            AudioSource musicSource,
            WeaponController weaponController,
            BombController bombController)
        {
            _musicSource = musicSource;
            _weaponController = weaponController;
            _bombController = bombController;
        }
        #endregion

        #region Internal
        private void OnFired(float recoil)
        {
            RequestDuck(_fireDuckMultiplier, _fireHoldSeconds);
        }

        private void OnExploded(Vector3 position, float radius)
        {
            RequestDuck(_explosionDuckMultiplier, _explosionHoldSeconds);
        }

        private void RequestDuck(float multiplier, float holdSeconds)
        {
            _duckMultiplier = Mathf.Min(multiplier, Time.unscaledTime < _holdUntil ? _duckMultiplier : 1f);
            _holdUntil = Mathf.Max(_holdUntil, Time.unscaledTime + holdSeconds);
        }
        #endregion
    }
}
