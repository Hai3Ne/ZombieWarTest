using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Core;

namespace ZombieWar.UI
{
    public sealed class OptionsPanelController : MonoBehaviour
    {
        #region Refs
        [SerializeField] private Button _openButton;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Toggle _sfxToggle;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _cameraShakeToggle;
        [SerializeField] private TMP_Text _sfxState;
        [SerializeField] private TMP_Text _musicState;
        [SerializeField] private TMP_Text _cameraShakeState;
        #endregion

        #region State
        private float _previousTimeScale = 1f;
        private bool _isOpen;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _sfxToggle.onValueChanged.AddListener(OnSfxChanged);
            _musicToggle.onValueChanged.AddListener(OnMusicChanged);
            _cameraShakeToggle.onValueChanged.AddListener(OnCameraShakeChanged);
            RefreshView();
            _panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(Open);
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }
            if (_sfxToggle != null)
            {
                _sfxToggle.onValueChanged.RemoveListener(OnSfxChanged);
            }
            if (_musicToggle != null)
            {
                _musicToggle.onValueChanged.RemoveListener(OnMusicChanged);
            }
            if (_cameraShakeToggle != null)
            {
                _cameraShakeToggle.onValueChanged.RemoveListener(OnCameraShakeChanged);
            }
        }
        #endregion

        #region API
        public void Open()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            RefreshView();
            _panelRoot.SetActive(true);
            _panelRoot.transform.SetAsLastSibling();
        }

        public void Close()
        {
            _panelRoot.SetActive(false);
            RestoreTimeScale();
        }

        public void SetViewReferences(
            Button openButton,
            GameObject panelRoot,
            Button closeButton,
            Toggle sfxToggle,
            Toggle musicToggle,
            Toggle cameraShakeToggle,
            TMP_Text sfxState,
            TMP_Text musicState,
            TMP_Text cameraShakeState)
        {
            _openButton = openButton;
            _panelRoot = panelRoot;
            _closeButton = closeButton;
            _sfxToggle = sfxToggle;
            _musicToggle = musicToggle;
            _cameraShakeToggle = cameraShakeToggle;
            _sfxState = sfxState;
            _musicState = musicState;
            _cameraShakeState = cameraShakeState;
        }
        #endregion

        #region Internal
        private void OnSfxChanged(bool enabled)
        {
            GameOptions.SetSfxEnabled(enabled);
            SetState(_sfxState, enabled);
        }

        private void OnMusicChanged(bool enabled)
        {
            GameOptions.SetMusicEnabled(enabled);
            SetState(_musicState, enabled);
        }

        private void OnCameraShakeChanged(bool enabled)
        {
            GameOptions.SetCameraShakeEnabled(enabled);
            SetState(_cameraShakeState, enabled);
        }

        private void RefreshView()
        {
            SetToggle(_sfxToggle, _sfxState, GameOptions.SfxEnabled);
            SetToggle(_musicToggle, _musicState, GameOptions.MusicEnabled);
            SetToggle(_cameraShakeToggle, _cameraShakeState, GameOptions.CameraShakeEnabled);
        }

        private static void SetToggle(Toggle toggle, TMP_Text state, bool enabled)
        {
            toggle.SetIsOnWithoutNotify(enabled);
            SetState(state, enabled);
        }

        private static void SetState(TMP_Text state, bool enabled)
        {
            state.text = enabled ? "ON" : "OFF";
            state.color = enabled ? new Color(0.35f, 1f, 0.68f) : new Color(1f, 0.36f, 0.28f);
        }

        private bool ValidateReferences()
        {
            bool valid = _openButton != null
                && _panelRoot != null
                && _closeButton != null
                && _sfxToggle != null
                && _musicToggle != null
                && _cameraShakeToggle != null
                && _sfxState != null
                && _musicState != null
                && _cameraShakeState != null;
            if (!valid)
            {
                Debug.LogError("[Zombie War] OptionsPanelController has missing authored references.", this);
            }
            return valid;
        }

        private void RestoreTimeScale()
        {
            if (!_isOpen)
            {
                return;
            }

            Time.timeScale = _previousTimeScale;
            _isOpen = false;
        }
        #endregion
    }
}
