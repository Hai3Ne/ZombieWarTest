using UnityEngine;
using UnityEngine.EventSystems;

namespace ZombieWar.UI
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Refs
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        #endregion

        #region State
        private Vector2 _value;
        public Vector2 Value => _value;
        #endregion

        #region API
        public void Configure(RectTransform background, RectTransform handle)
        {
            _background = background;
            _handle = handle;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_background == null || _handle == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _background,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            Vector2 radius = _background.rect.size * 0.5f;
            _value = new Vector2(localPoint.x / radius.x, localPoint.y / radius.y);
            _value = Vector2.ClampMagnitude(_value, 1f);
            _handle.anchoredPosition = new Vector2(_value.x * radius.x * 0.55f, _value.y * radius.y * 0.55f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _value = Vector2.zero;
            if (_handle != null)
            {
                _handle.anchoredPosition = Vector2.zero;
            }
        }
        #endregion
    }
}
