using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieWar.Core
{
    public sealed class MainMenuController : MonoBehaviour
    {
        public void LoadLevelOne() => _ = LoadSceneAsync("Level01");

        public void LoadLevelTwo() => _ = LoadSceneAsync("Level02");

        private static async Awaitable LoadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
