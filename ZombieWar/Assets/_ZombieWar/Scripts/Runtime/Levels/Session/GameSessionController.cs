using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Combat;
using ZombieWar.Core;
using ZombieWar.Enemies;
using ZombieWar.UI;

namespace ZombieWar.Levels
{
    public sealed class GameSessionController : MonoBehaviour
    {
        #region Refs
        private Health _playerHealth;
        private WaveDirector _wave;
        private RuntimeHud _hud;
        private LevelCatalogConfig _levelCatalog;
        private LevelExitPortal _exitPortal;
        private EnemyPool _enemyPool;
        #endregion

        #region State
        private bool _finished;
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_finished || _wave == null || _playerHealth == null)
            {
                return;
            }

            if (_playerHealth.IsDead)
            {
                Finish(false);
            }
            else if (_wave.IsCompleted)
            {
                Finish(true);
            }
        }
        #endregion

        #region API
        public void Configure(
            Health playerHealth,
            WaveDirector wave,
            RuntimeHud hud,
            LevelCatalogConfig levelCatalog,
            LevelExitPortal exitPortal,
            EnemyPool enemyPool)
        {
            _playerHealth = playerHealth;
            _wave = wave;
            _hud = hud;
            _levelCatalog = levelCatalog;
            _exitPortal = exitPortal;
            _enemyPool = enemyPool;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            _ = LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

        public void LoadNext()
        {
            Time.timeScale = 1f;
            string target = _levelCatalog != null
                ? _levelCatalog.GetNextSceneName(SceneManager.GetActiveScene().name)
                : string.Empty;
            if (!string.IsNullOrEmpty(target))
            {
                _ = SceneTransitionRequest.LoadThroughLoadingAsync(target);
            }
        }
        #endregion

        #region Internal
        private void Finish(bool won)
        {
            _finished = true;
            if (!won)
            {
                _hud.ShowResult(false);
                return;
            }

            _enemyPool.ReleaseAll();
            string target = _levelCatalog.GetNextSceneName(SceneManager.GetActiveScene().name);
            if (_exitPortal == null || string.IsNullOrWhiteSpace(target))
            {
                _hud.ShowResult(true);
                return;
            }

            _exitPortal.Open(target);
            _hud.ShowPortalOpened();
        }

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
}
