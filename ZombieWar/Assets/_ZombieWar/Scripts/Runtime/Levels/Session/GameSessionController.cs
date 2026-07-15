using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Combat;
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
            else if (_wave.Remaining <= 0f)
            {
                Finish(true);
            }
        }
        #endregion

        #region API
        public void Configure(Health playerHealth, WaveDirector wave, RuntimeHud hud, LevelCatalogConfig levelCatalog)
        {
            _playerHealth = playerHealth;
            _wave = wave;
            _hud = hud;
            _levelCatalog = levelCatalog;
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
                _ = LoadSceneAsync(target);
            }
        }
        #endregion

        #region Internal
        private void Finish(bool won)
        {
            _finished = true;
            _hud.ShowResult(won);
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
