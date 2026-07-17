using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZombieWar.Combat;
using ZombieWar.Enemies;
using ZombieWar.Levels;
using ZombieWar.Player;

namespace ZombieWar.UI
{
    public sealed class RuntimeHud : MonoBehaviour
    {
        #region Refs
        [SerializeField] private VirtualJoystick _joystick;
        [SerializeField] private VirtualJoystick _aimJoystick;
        [SerializeField] private BombAimJoystick _bombJoystick;
        [SerializeField] private WeaponRadialMenu _weaponMenu;
        [SerializeField] private BombInventoryView _bombInventoryView;
        [SerializeField] private ScreenFeedbackView _screenFeedback;
        [SerializeField] private Image _healthFill;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _weaponText;
        [SerializeField] private TMP_Text _crowdText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _nextButton;

        private SoldierController _soldier;
        private WeaponController _weapon;
        private BombController _bomb;
        private WaveDirector _wave;
        private EnemyPool _enemyPool;
        #endregion

        #region State
        private Action _restartAction;
        private Action _nextAction;
        private bool _portalOpen;
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_soldier == null)
            {
                return;
            }

            _soldier.SetMoveInput(_joystick.Value);
            _weapon.SetAimInput(_aimJoystick.Value, _aimJoystick.IsActive);
            _healthFill.fillAmount = _soldier.Health.Normalized;
            _timerText.text = FormatTime(_wave.Remaining);
            _weaponText.text = _weapon.CurrentWeaponName;
            _crowdText.text = _portalOpen
                ? "PORTAL OPEN   MOVE TO EXTRACTION"
                : $"{_wave.EncounterLabel}   THREAT {_enemyPool.ActiveCount:000}";
        }

        private void OnDestroy()
        {
            if (_bombJoystick == null)
            {
                return;
            }

            _bombJoystick.AimChanged -= OnBombAimChanged;
            _bombJoystick.Released -= OnBombReleased;
            _retryButton.onClick.RemoveListener(OnRestart);
            _nextButton.onClick.RemoveListener(OnNext);
        }
        #endregion

        #region API
        public void Initialize(
            SoldierController soldier,
            WeaponController weapon,
            BombController bomb,
            WaveDirector wave,
            EnemyPool enemyPool,
            Action restartAction,
            Action nextAction)
        {
            _soldier = soldier;
            _weapon = weapon;
            _bomb = bomb;
            _wave = wave;
            _enemyPool = enemyPool;
            _restartAction = restartAction;
            _nextAction = nextAction;

            _weaponMenu.Initialize(_weapon);
            _bombInventoryView.Initialize(_bomb);
            if (_screenFeedback != null)
            {
                _screenFeedback.Initialize(_soldier.Health);
            }
            else
            {
                Debug.LogError("[Zombie War] RuntimeHud requires an authored ScreenFeedbackView.", this);
            }
            _bombJoystick.AimChanged += OnBombAimChanged;
            _bombJoystick.Released += OnBombReleased;
            _retryButton.onClick.AddListener(OnRestart);
            _nextButton.onClick.AddListener(OnNext);
            _resultPanel.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            _portalOpen = false;
        }

        public void SetViewReferences(
            VirtualJoystick joystick,
            VirtualJoystick aimJoystick,
            BombAimJoystick bombJoystick,
            WeaponRadialMenu weaponMenu,
            BombInventoryView bombInventoryView,
            Image healthFill,
            TMP_Text timerText,
            TMP_Text weaponText,
            TMP_Text crowdText,
            GameObject resultPanel,
            TMP_Text resultText,
            Button retryButton,
            Button nextButton)
        {
            _joystick = joystick;
            _aimJoystick = aimJoystick;
            _bombJoystick = bombJoystick;
            _weaponMenu = weaponMenu;
            _bombInventoryView = bombInventoryView;
            _healthFill = healthFill;
            _timerText = timerText;
            _weaponText = weaponText;
            _crowdText = crowdText;
            _resultPanel = resultPanel;
            _resultText = resultText;
            _retryButton = retryButton;
            _nextButton = nextButton;
        }

        public void SetScreenFeedback(ScreenFeedbackView screenFeedback)
        {
            _screenFeedback = screenFeedback;
        }

        public void SetAimJoystick(VirtualJoystick aimJoystick)
        {
            _aimJoystick = aimJoystick;
        }

        public void ShowResult(bool won)
        {
            _portalOpen = false;
            _resultText.text = won ? "AREA SECURED" : "SOLDIER DOWN";
            _nextButton.gameObject.SetActive(won);
            _resultPanel.SetActive(true);
        }

        public void ShowPortalOpened()
        {
            _portalOpen = true;
            _timerText.text = "00:00";
            _resultPanel.SetActive(false);
        }
        #endregion

        #region Internal
        private void OnBombAimChanged(Vector2 input)
        {
            _bomb.PreviewAim(input);
        }

        private void OnBombReleased(Vector2 input)
        {
            _bomb.ThrowBomb(input);
        }

        private void OnRestart()
        {
            _restartAction?.Invoke();
        }

        private void OnNext()
        {
            _nextAction?.Invoke();
        }

        private static string FormatTime(float seconds)
        {
            int rounded = Mathf.CeilToInt(seconds);
            return $"{rounded / 60:00}:{rounded % 60:00}";
        }
        #endregion
    }
}
