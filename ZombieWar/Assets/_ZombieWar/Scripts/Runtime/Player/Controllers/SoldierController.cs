using UnityEngine;
using UnityEngine.InputSystem;
using ZombieWar.Combat;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(Health), typeof(SoldierAnimationController))]
    public sealed class SoldierController : MonoBehaviour
    {
        #region Config
        [SerializeField] private float _moveSpeed = 6f;
        #endregion

        #region Refs
        private Rigidbody _rigidbody;
        private Health _health;
        private WeaponController _weaponController;
        private SoldierAnimationController _animationController;
        #endregion

        #region State
        private Vector2 _moveInput;
        public Health Health => _health;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody)
                || !TryGetComponent(out _health)
                || !TryGetComponent(out _weaponController)
                || !TryGetComponent(out _animationController))
            {
                Debug.LogError("[Zombie War] Soldier is missing a required component.", this);
                enabled = false;
                return;
            }

            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
            _weaponController.Fired += OnFired;
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

            Vector3 worldMovement = new(_moveInput.x, 0f, _moveInput.y);
            _animationController.SetMovement(worldMovement);
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
                _health.Died -= OnDied;
            }
            if (_weaponController != null)
            {
                _weaponController.Fired -= OnFired;
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
            _animationController.TriggerHit();
        }

        private void OnFired(float recoil)
        {
            _animationController.TriggerFire(recoil);
        }

        private void OnDied()
        {
            _animationController.SetDead();
        }
        #endregion
    }
}
