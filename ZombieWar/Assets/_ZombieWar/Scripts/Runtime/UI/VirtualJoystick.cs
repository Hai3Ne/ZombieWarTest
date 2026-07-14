using UnityEngine;
using UnityEngine.EventSystems;

namespace ZombieWar.UI
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Refs
        [SerializeField] private RectTransform _inputZone;
        [SerializeField] private RectTransform _visualRoot;
        [SerializeField] private RectTransform _handle;
        #endregion

        #region State
        private const int NoPointer = int.MinValue;

        private Vector2 _value;
        private int _activePointerId = NoPointer;
        public Vector2 Value => _value;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_inputZone == null || _visualRoot == null || _handle == null)
            {
                Debug.LogError("[Zombie War] Floating joystick references were not authored.", this);
                enabled = false;
                return;
            }

            ResetJoystick(false);
        }
        #endregion

        #region API
        public void Configure(RectTransform inputZone, RectTransform visualRoot, RectTransform handle)
        {
            _inputZone = inputZone;
            _visualRoot = visualRoot;
            _handle = handle;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_activePointerId != NoPointer || !TryGetZonePoint(eventData, out Vector2 zonePoint))
            {
                return;
            }

            _activePointerId = eventData.pointerId;
            _visualRoot.anchoredPosition = ClampVisualCenter(zonePoint);
            _visualRoot.gameObject.SetActive(true);
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            UpdateValue(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            _activePointerId = NoPointer;
            ResetJoystick(false);
        }
        #endregion

        #region Internal
        private void UpdateValue(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _visualRoot,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            Vector2 radius = _visualRoot.rect.size * 0.5f;
            _value = new Vector2(localPoint.x / radius.x, localPoint.y / radius.y);
            _value = Vector2.ClampMagnitude(_value, 1f);
            _handle.anchoredPosition = new Vector2(_value.x * radius.x * 0.55f, _value.y * radius.y * 0.55f);
        }

        private bool TryGetZonePoint(PointerEventData eventData, out Vector2 zonePoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _inputZone,
                eventData.position,
                eventData.pressEventCamera,
                out zonePoint);
        }

        private Vector2 ClampVisualCenter(Vector2 requestedCenter)
        {
            Rect zone = _inputZone.rect;
            Vector2 scaledSize = Vector2.Scale(_visualRoot.rect.size, _visualRoot.localScale);
            Vector2 halfSize = scaledSize * 0.5f;
            return new Vector2(
                Mathf.Clamp(requestedCenter.x, zone.xMin + halfSize.x, zone.xMax - halfSize.x),
                Mathf.Clamp(requestedCenter.y, zone.yMin + halfSize.y, zone.yMax - halfSize.y));
        }

        private void ResetJoystick(bool visible)
        {
            _value = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
            _visualRoot.gameObject.SetActive(visible);
        }
        #endregion
    }
}
