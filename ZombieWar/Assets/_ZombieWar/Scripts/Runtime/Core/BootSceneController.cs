using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieWar.Core
{
    public sealed class BootSceneController : MonoBehaviour
    {
        [SerializeField] private string _nextScene = "MainMenu";

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            _ = LoadSceneAsync(_nextScene);
        }

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
