using UnityEngine;
using ZombieWar.Core;
using ZombieWar.Player;

namespace ZombieWar.Levels
{
    public sealed class LevelExitPortal : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0f)] private float _ringRotationSpeed = 55f;
        [SerializeField, Min(0f)] private float _lightPulseSpeed = 2.2f;
        [SerializeField, Min(0f)] private float _minimumLightIntensity = 2.5f;
        [SerializeField, Min(0f)] private float _maximumLightIntensity = 6f;
        #endregion

        #region Refs
        [SerializeField] private GameObject _visualRoot;
        [SerializeField] private Collider _trigger;
        [SerializeField] private Transform[] _rings;
        [SerializeField] private Light _portalLight;
        [SerializeField] private ParticleSystem _particles;
        #endregion

        #region State
        private string _targetScene;
        private bool _isTransitioning;
        public bool IsOpen { get; private set; }
        public string TargetScene => _targetScene;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_visualRoot == null || _trigger == null || _rings == null || _rings.Length == 0)
            {
                Debug.LogError("[Zombie War] LevelExitPortal has missing authored references.", this);
                enabled = false;
                return;
            }

            _trigger.isTrigger = true;
            Close();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            float rotation = _ringRotationSpeed * Time.deltaTime;
            for (int i = 0; i < _rings.Length; i++)
            {
                float direction = i % 2 == 0 ? 1f : -1f;
                _rings[i].Rotate(0f, 0f, rotation * direction, Space.Self);
            }

            if (_portalLight != null)
            {
                float pulse = Mathf.Sin(Time.time * _lightPulseSpeed) * 0.5f + 0.5f;
                _portalLight.intensity = Mathf.Lerp(_minimumLightIntensity, _maximumLightIntensity, pulse);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsOpen || _isTransitioning || other.GetComponentInParent<SoldierController>() == null)
            {
                return;
            }

            _isTransitioning = true;
            _trigger.enabled = false;
            _ = SceneTransitionRequest.LoadThroughLoadingAsync(_targetScene);
        }
        #endregion

        #region API
        public void Open(string targetScene)
        {
            if (string.IsNullOrWhiteSpace(targetScene))
            {
                Debug.LogError("[Zombie War] Exit portal requires a target scene.", this);
                return;
            }

            _targetScene = targetScene;
            _isTransitioning = false;
            IsOpen = true;
            _visualRoot.SetActive(true);
            _trigger.enabled = true;
            if (_particles != null && !_particles.isPlaying)
            {
                _particles.Play(true);
            }
        }

        public void Close()
        {
            IsOpen = false;
            _isTransitioning = false;
            _targetScene = string.Empty;
            if (_trigger != null)
            {
                _trigger.enabled = false;
            }
            if (_visualRoot != null)
            {
                _visualRoot.SetActive(false);
            }
        }

        public void SetReferences(
            GameObject visualRoot,
            Collider trigger,
            Transform[] rings,
            Light portalLight,
            ParticleSystem particles)
        {
            _visualRoot = visualRoot;
            _trigger = trigger;
            _rings = rings;
            _portalLight = portalLight;
            _particles = particles;
        }
        #endregion
    }
}
