using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Core;
using ZombieWar.Levels;

namespace ZombieWar.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class LevelSelectButton : MonoBehaviour
    {
        #region Refs
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        private MainMenuController _owner;
        #endregion

        #region State
        private int _enabledLevelIndex;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_button == null || _label == null)
            {
                Debug.LogError("[Zombie War] LevelSelectButton requires authored Button and TMP label references.", this);
                enabled = false;
                return;
            }
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClicked);
            }
        }
        #endregion

        #region API
        public void SetViewReferences(Button button, TMP_Text label)
        {
            _button = button;
            _label = label;
        }

        public void Configure(MainMenuController owner, int enabledLevelIndex, LevelDefinition level)
        {
            _owner = owner;
            _enabledLevelIndex = enabledLevelIndex;
            bool active = level != null;
            gameObject.SetActive(active);
            if (active)
            {
                _label.text = $"{level.DisplayName}\n{level.WaveSequence.TotalDuration:0}s";
            }
        }
        #endregion

        #region Internal
        private void OnClicked()
        {
            _owner?.LoadLevel(_enabledLevelIndex);
        }
        #endregion
    }
}
