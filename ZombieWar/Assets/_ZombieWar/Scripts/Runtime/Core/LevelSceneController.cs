using UnityEngine;
using ZombieWar.Combat;
using ZombieWar.Enemies;
using ZombieWar.Levels;
using ZombieWar.Player;
using ZombieWar.UI;

namespace ZombieWar.Core
{
    public sealed class LevelSceneController : MonoBehaviour
    {
        [SerializeField] private WeaponConfig[] _weapons;
        [SerializeField] private WaveSequenceConfig _waveSequence;
        [SerializeField] private LevelCatalogConfig _levelCatalog;
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

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _weaponController.Configure(_weapons, _enemyPool, _projectilePool, _muzzle);
            _bombController.Configure(_enemyPool);
            _scheduler.Configure(_enemyPool, _soldier.transform);
            _waveDirector.Configure(_enemyPool, _waveSequence, _soldier.transform, _soldier.Health, _worldCamera);
            _session.Configure(_soldier.Health, _waveDirector, _hud, _levelCatalog);
            _hud.Initialize(_soldier, _weaponController, _bombController, _waveDirector, _enemyPool, _session.Restart, _session.LoadNext);
        }

        public void SetReferences(
            WeaponConfig[] weapons,
            WaveSequenceConfig waveSequence,
            LevelCatalogConfig levelCatalog,
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
            _waveSequence = waveSequence;
            _levelCatalog = levelCatalog;
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

        private bool ValidateReferences()
        {
            bool valid = _weapons is { Length: > 1 }
                && _waveSequence != null
                && _levelCatalog != null
                && _soldier != null
                && _soldier.Health != null
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
    }
}
