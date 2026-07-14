using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Combat;
using ZombieWar.Enemies;
using ZombieWar.Levels;
using ZombieWar.Player;
using ZombieWar.UI;

namespace ZombieWar.Core
{
    public sealed class BootSceneController : MonoBehaviour
    {
        #region Config
        [SerializeField] private string _nextScene = "MainMenu";
        #endregion

        #region Lifecycle
        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            _ = LoadSceneAsync(_nextScene);
        }
        #endregion

        #region Internal
        private static async Awaitable LoadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
        #endregion
    }

    public sealed class MainMenuController : MonoBehaviour
    {
        #region API
        public void LoadLevelOne()
        {
            _ = LoadSceneAsync("Level01");
        }

        public void LoadLevelTwo()
        {
            _ = LoadSceneAsync("Level02");
        }
        #endregion

        #region Internal
        private static async Awaitable LoadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
        #endregion
    }

    public sealed class LevelSceneController : MonoBehaviour
    {
        #region Config
        [SerializeField] private WeaponConfig[] _weapons;
        [SerializeField] private EnemyConfig _regularEnemy;
        [SerializeField] private EnemyConfig _giantEnemy;
        [SerializeField] private LevelConfig _level;
        #endregion

        #region Refs
        [SerializeField] private SoldierController _soldier;
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private BombController _bombController;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ProjectilePool _projectilePool;
        [SerializeField] private EnemyPool _enemyPool;
        [SerializeField] private EnemySimulationScheduler _scheduler;
        [SerializeField] private WaveDirector _waveDirector;
        [SerializeField] private GameSessionController _session;
        [SerializeField] private RuntimeHud _hud;
        [SerializeField] private Camera _worldCamera;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _weaponController.Configure(_weapons, _enemyPool, _projectilePool, _muzzle);
            _bombController.Configure(_enemyPool, _weaponController);
            _scheduler.Configure(_enemyPool, _soldier.transform);
            _waveDirector.Configure(
                _enemyPool,
                _regularEnemy,
                _giantEnemy,
                _level,
                _soldier.transform,
                _soldier.Health,
                _worldCamera);
            _session.Configure(_soldier.Health, _waveDirector, _hud);
            _hud.Initialize(
                _soldier,
                _weaponController,
                _bombController,
                _waveDirector,
                _enemyPool,
                _session.Restart,
                _session.LoadNext);
        }
        #endregion

        #region API
        public void SetReferences(
            WeaponConfig[] weapons,
            EnemyConfig regularEnemy,
            EnemyConfig giantEnemy,
            LevelConfig level,
            SoldierController soldier,
            WeaponController weaponController,
            BombController bombController,
            Transform muzzle,
            ProjectilePool projectilePool,
            EnemyPool enemyPool,
            EnemySimulationScheduler scheduler,
            WaveDirector waveDirector,
            GameSessionController session,
            RuntimeHud hud,
            Camera worldCamera)
        {
            _weapons = weapons;
            _regularEnemy = regularEnemy;
            _giantEnemy = giantEnemy;
            _level = level;
            _soldier = soldier;
            _weaponController = weaponController;
            _bombController = bombController;
            _muzzle = muzzle;
            _projectilePool = projectilePool;
            _enemyPool = enemyPool;
            _scheduler = scheduler;
            _waveDirector = waveDirector;
            _session = session;
            _hud = hud;
            _worldCamera = worldCamera;
        }
        #endregion

        #region Internal
        private bool ValidateReferences()
        {
            bool valid = _weapons is { Length: > 1 }
                && _regularEnemy != null
                && _giantEnemy != null
                && _level != null
                && _soldier != null
                && _weaponController != null
                && _bombController != null
                && _muzzle != null
                && _projectilePool != null
                && _enemyPool != null
                && _scheduler != null
                && _waveDirector != null
                && _session != null
                && _hud != null
                && _worldCamera != null;
            if (!valid)
            {
                Debug.LogError("[Zombie War] LevelSceneController has missing serialized references.", this);
            }
            return valid;
        }
        #endregion
    }
}
