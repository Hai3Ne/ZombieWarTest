using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZombieWar.UI
{
    public sealed class BombAimJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        private const int NoPointer = int.MinValue;
        private int _activePointerId = NoPointer;

        public Vector2 Value { get; private set; }
        public event Action<Vector2> AimChanged;
        public event Action<Vector2> Released;

        private void Awake()
        {
            if (_background == null || _handle == null)
            {
                Debug.LogError("[Zombie War] Bomb joystick references were not authored.", this);
                enabled = false;
            }
        }

        public void Configure(RectTransform background, RectTransform handle)
        {
            _background = background;
            _handle = handle;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_activePointerId != NoPointer)
            {
                return;
            }
            _activePointerId = eventData.pointerId;
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == _activePointerId)
            {
                UpdateValue(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            Vector2 releasedValue = Value;
            _activePointerId = NoPointer;
            ResetHandle();
            Released?.Invoke(releasedValue);
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _background,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            Vector2 radius = _background.rect.size * 0.5f;
            Value = Vector2.ClampMagnitude(new Vector2(localPoint.x / radius.x, localPoint.y / radius.y), 1f);
            _handle.anchoredPosition = new Vector2(Value.x * radius.x * 0.58f, Value.y * radius.y * 0.58f);
            AimChanged?.Invoke(Value);
        }

        private void ResetHandle()
        {
            Value = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }
    }
}
