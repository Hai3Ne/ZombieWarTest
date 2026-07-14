using UnityEngine;
using UnityEngine.InputSystem;
using ZombieWar.Combat;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public sealed class SoldierController : MonoBehaviour
    {
        #region Config
        [SerializeField] private float _moveSpeed = 6f;
        #endregion

        #region Refs
        private Rigidbody _rigidbody;
        private Health _health;
        private Renderer _renderer;
        #endregion

        #region State
        private Vector2 _moveInput;
        private float _damageFlashUntil;
        private MaterialPropertyBlock _propertyBlock;
        public Health Health => _health;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            if (!TryGetComponent(out _rigidbody)
                || !TryGetComponent(out _health)
                || !TryGetComponent(out _renderer))
            {
                Debug.LogError("[Zombie War] Soldier is missing a required component.", this);
                enabled = false;
                return;
            }

            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _health.Damaged += OnDamaged;
        }

        private void Update()
        {
            if (_moveInput.sqrMagnitude < 0.01f && Keyboard.current != null)
            {
                Vector2 keyboard = Vector2.zero;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) keyboard.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) keyboard.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboard.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboard.x += 1f;
                _moveInput = Vector2.ClampMagnitude(keyboard, 1f);
            }

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(
                "_BaseColor",
                Time.time < _damageFlashUntil
                    ? new Color(1f, 0.2f, 0.12f)
                    : new Color(0.12f, 0.45f, 0.85f));
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private void FixedUpdate()
        {
            if (_health.IsDead)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 velocity = new(_moveInput.x * _moveSpeed, _rigidbody.linearVelocity.y, _moveInput.y * _moveSpeed);
            _rigidbody.linearVelocity = velocity;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
            }
        }
        #endregion

        #region API
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }
        #endregion

        #region Internal
        private void OnDamaged(DamageInfo damage)
        {
            _damageFlashUntil = Time.time + 0.12f;
        }
        #endregion
    }
}
