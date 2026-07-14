using UnityEngine;

namespace ZombieWar.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        #region Refs
        private RectTransform _rectTransform;
        #endregion

        #region State
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _rectTransform))
            {
                enabled = false;
                return;
            }
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea
                || _lastScreenSize.x != Screen.width
                || _lastScreenSize.y != Screen.height)
            {
                Apply();
            }
        }
        #endregion

        #region Internal
        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 min = safeArea.position;
            Vector2 max = safeArea.position + safeArea.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            _rectTransform.anchorMin = min;
            _rectTransform.anchorMax = max;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
        #endregion
    }
}
