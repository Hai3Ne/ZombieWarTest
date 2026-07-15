using TMPro;
using UnityEngine;

namespace ZombieWar.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class FpsDisplay : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0.1f)] private float _refreshInterval = 0.25f;
        [SerializeField, Range(0.01f, 1f)] private float _smoothing = 0.12f;
        #endregion

        #region Refs
        [SerializeField] private TMP_Text _label;
        #endregion

        #region State
        private float _smoothedDeltaTime;
        private float _nextRefreshTime;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_label == null && !TryGetComponent(out _label))
            {
                Debug.LogError("[Zombie War] FpsDisplay requires a TMP_Text reference.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _smoothedDeltaTime = 1f / Mathf.Max(1, Application.targetFrameRate);
            _nextRefreshTime = 0f;
        }

        private void Update()
        {
            _smoothedDeltaTime = Mathf.Lerp(
                _smoothedDeltaTime,
                Time.unscaledDeltaTime,
                1f - Mathf.Pow(1f - _smoothing, Time.unscaledDeltaTime * 60f));
            if (Time.unscaledTime < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = Time.unscaledTime + _refreshInterval;
            float fps = 1f / Mathf.Max(0.0001f, _smoothedDeltaTime);
            _label.SetText("FPS  {0:0}", fps);
        }
        #endregion

        #region API
        public void SetLabel(TMP_Text label)
        {
            _label = label;
        }
        #endregion
    }
}
