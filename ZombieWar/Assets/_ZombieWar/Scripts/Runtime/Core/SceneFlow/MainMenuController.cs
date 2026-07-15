using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Levels;
using ZombieWar.UI;

namespace ZombieWar.Core
{
    public sealed class MainMenuController : MonoBehaviour
    {
        #region Config
        [SerializeField] private LevelCatalogConfig _catalog;
        [SerializeField] private LevelSelectButton[] _levelButtons;
        #endregion

        #region Lifecycle
        private void Start()
        {
            if (_catalog == null || _levelButtons == null || _levelButtons.Length == 0)
            {
                Debug.LogError("[Zombie War] MainMenuController requires a LevelCatalog and authored button slots.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                _levelButtons[i].Configure(this, i, _catalog.GetEnabledLevel(i));
            }
        }
        #endregion

        #region API
        public void SetReferences(LevelCatalogConfig catalog, LevelSelectButton[] levelButtons)
        {
            _catalog = catalog;
            _levelButtons = levelButtons;
        }

        public void LoadLevel(int enabledLevelIndex)
        {
            LevelDefinition level = _catalog != null ? _catalog.GetEnabledLevel(enabledLevelIndex) : null;
            if (level != null)
            {
                _ = LoadSceneAsync(level.SceneName);
            }
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
}
