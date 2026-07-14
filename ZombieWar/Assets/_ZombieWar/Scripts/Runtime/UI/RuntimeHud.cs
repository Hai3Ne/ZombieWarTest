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
        [SerializeField] private BombAimJoystick _bombJoystick;
        [SerializeField] private WeaponRadialMenu _weaponMenu;
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _bombCooldownFill;
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
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_soldier == null)
            {
                return;
            }

            _soldier.SetMoveInput(_joystick.Value);
            _healthFill.fillAmount = _soldier.Health.Normalized;
            _bombCooldownFill.fillAmount = _bomb.CooldownNormalized;
            _timerText.text = FormatTime(_wave.Remaining);
            _weaponText.text = _weapon.CurrentWeaponName;
            _crowdText.text = $"THREAT  {_enemyPool.ActiveCount:000}";
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
            _bombJoystick.AimChanged += OnBombAimChanged;
            _bombJoystick.Released += OnBombReleased;
            _retryButton.onClick.AddListener(OnRestart);
            _nextButton.onClick.AddListener(OnNext);
            _resultPanel.SetActive(false);
        }

        public void SetViewReferences(
            VirtualJoystick joystick,
            BombAimJoystick bombJoystick,
            WeaponRadialMenu weaponMenu,
            Image healthFill,
            Image bombCooldownFill,
            TMP_Text timerText,
            TMP_Text weaponText,
            TMP_Text crowdText,
            GameObject resultPanel,
            TMP_Text resultText,
            Button retryButton,
            Button nextButton)
        {
            _joystick = joystick;
            _bombJoystick = bombJoystick;
            _weaponMenu = weaponMenu;
            _healthFill = healthFill;
            _bombCooldownFill = bombCooldownFill;
            _timerText = timerText;
            _weaponText = weaponText;
            _crowdText = crowdText;
            _resultPanel = resultPanel;
            _resultText = resultText;
            _retryButton = retryButton;
            _nextButton = nextButton;
        }

        public void ShowResult(bool won)
        {
            _resultText.text = won ? "AREA SECURED" : "SOLDIER DOWN";
            _resultPanel.SetActive(true);
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
