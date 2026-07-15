using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace ZombieWar.Core
{
    public sealed class BootSceneController : MonoBehaviour
    {
        [SerializeField] private string _nextScene = "MainMenu";
        [SerializeField] private float _minimumDisplaySeconds = 0.8f;
        [SerializeField] private Image _progressFill;
        [SerializeField] private TMP_Text _progressText;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            _ = LoadSceneAsync(SceneTransitionRequest.ConsumeTarget(_nextScene));
        }

        public void SetViewReferences(Image progressFill, TMP_Text progressText)
        {
            _progressFill = progressFill;
            _progressText = progressText;
        }

        private async Awaitable LoadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                return;
            }

            operation.allowSceneActivation = false;
            float startedAt = Time.realtimeSinceStartup;
            while (operation.progress < 0.9f || Time.realtimeSinceStartup - startedAt < _minimumDisplaySeconds)
            {
                UpdateProgress(Mathf.Clamp01(operation.progress / 0.9f));
                await Awaitable.NextFrameAsync();
            }

            UpdateProgress(1f);
            operation.allowSceneActivation = true;
        }

        private void UpdateProgress(float normalized)
        {
            if (_progressFill != null)
            {
                _progressFill.fillAmount = normalized;
            }
            if (_progressText != null)
            {
                _progressText.text = $"LOADING... {Mathf.RoundToInt(normalized * 100f):00}%";
            }
        }
    }
}
