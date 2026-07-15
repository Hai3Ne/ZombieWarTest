using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieWar.Core
{
    public static class SceneTransitionRequest
    {
        private const string LoadingSceneName = "Loading";
        private static string _targetScene;
        private static bool _isLoading;

        public static void SetTarget(string sceneName)
        {
            _targetScene = sceneName;
        }

        public static string ConsumeTarget(string fallbackScene)
        {
            string target = string.IsNullOrWhiteSpace(_targetScene) ? fallbackScene : _targetScene;
            _targetScene = string.Empty;
            _isLoading = false;
            return target;
        }

        public static async Awaitable LoadThroughLoadingAsync(string targetScene)
        {
            if (_isLoading || string.IsNullOrWhiteSpace(targetScene))
            {
                return;
            }

            _isLoading = true;
            SetTarget(targetScene);
            AsyncOperation operation = SceneManager.LoadSceneAsync(LoadingSceneName);
            while (operation != null && !operation.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
