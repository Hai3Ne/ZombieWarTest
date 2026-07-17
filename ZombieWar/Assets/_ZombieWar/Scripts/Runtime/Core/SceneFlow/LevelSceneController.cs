using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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
        [SerializeField] private LevelExitPortal _exitPortal;
        [SerializeField] private Transform[] _playerSpawnPoints;
        [SerializeField] private LayerMask _playerSpawnBlockingLayers = ~0;
        [SerializeField, Min(0.1f)] private float _playerSpawnSampleRadius = 2f;
        [SerializeField, Min(0.1f)] private float _playerSpawnCapsuleHeight = 2f;
        [SerializeField, Min(0.05f)] private float _playerSpawnCapsuleRadius = 0.48f;

        private void Start()
        {
            ResolveLevelConfig();
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            PlaceSoldierAtSpawnPoint();
            _weaponController.Configure(_weapons, _projectilePool, _muzzle);
            _bombController.Configure(_enemyPool);
            _scheduler.Configure(_enemyPool, _soldier.transform);
            _waveDirector.Configure(_enemyPool, _waveSequence, _soldier.transform, _soldier.Health, _worldCamera);
            _session.Configure(_soldier.Health, _waveDirector, _hud, _levelCatalog, _exitPortal, _enemyPool);
            _hud.Initialize(_soldier, _weaponController, _bombController, _waveDirector, _enemyPool, _session.Restart, _session.LoadNext);
        }

        private void ResolveLevelConfig()
        {
            if (_levelCatalog == null)
            {
                return;
            }

            LevelDefinition level = _levelCatalog.GetLevelBySceneName(SceneManager.GetActiveScene().name);
            if (level != null)
            {
                _waveSequence = level.WaveSequence;
            }
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
            Camera worldCamera,
            LevelExitPortal exitPortal)
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
            _exitPortal = exitPortal;
        }

        public void SetExitPortal(LevelExitPortal exitPortal)
        {
            _exitPortal = exitPortal;
        }

        public void SetPlayerSpawnPoints(Transform[] playerSpawnPoints)
        {
            _playerSpawnPoints = playerSpawnPoints;
        }

        private void PlaceSoldierAtSpawnPoint()
        {
            if (_playerSpawnPoints == null || _playerSpawnPoints.Length == 0)
            {
                return;
            }

            float halfHeight = _playerSpawnCapsuleHeight * 0.5f;
            float radius = Mathf.Min(_playerSpawnCapsuleRadius, halfHeight);
            for (int index = 0; index < _playerSpawnPoints.Length; index++)
            {
                Transform spawnPoint = _playerSpawnPoints[index];
                if (spawnPoint == null
                    || !NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, _playerSpawnSampleRadius, NavMesh.AllAreas))
                {
                    continue;
                }

                Vector3 playerPosition = hit.position + Vector3.up * halfHeight;
                Vector3 capsuleBottom = playerPosition + Vector3.up * (-halfHeight + radius);
                Vector3 capsuleTop = playerPosition + Vector3.up * (halfHeight - radius);
                if (Physics.CheckCapsule(
                        capsuleBottom,
                        capsuleTop,
                        radius,
                        _playerSpawnBlockingLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                _soldier.transform.SetPositionAndRotation(playerPosition, spawnPoint.rotation);
                Physics.SyncTransforms();
                return;
            }

            Debug.LogWarning("[Zombie War] No clear Player Spawn Point was found; keeping the authored Soldier position.", this);
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
                && _worldCamera != null
                && _exitPortal != null;
            if (!valid)
            {
                Debug.LogError("[Zombie War] LevelSceneController has missing serialized references.", this);
            }
            return valid;
        }
    }
}
