using Unity.Cinemachine;
using UnityEngine;
using ZombieWar.Combat;
using ZombieWar.Core;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(CinemachineImpulseSource), typeof(Health), typeof(WeaponController))]
    public sealed class CameraShakeController : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0f)] private float _minimumDamageForce = 0.22f;
        [SerializeField, Min(0f)] private float _maximumDamageForce = 0.85f;
        [SerializeField, Min(0f)] private float _weaponRecoilMultiplier = 1.8f;
        [SerializeField, Min(0f)] private float _maximumWeaponForce = 0.42f;
        [SerializeField, Min(0f)] private float _bombForce = 0.95f;
        #endregion

        #region Refs
        [SerializeField] private CinemachineImpulseSource _impulseSource;
        [SerializeField] private Health _health;
        [SerializeField] private WeaponController _weaponController;
        [SerializeField] private BombController _bombController;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if ((_impulseSource == null && !TryGetComponent(out _impulseSource))
                || (_health == null && !TryGetComponent(out _health))
                || (_weaponController == null && !TryGetComponent(out _weaponController)))
            {
                Debug.LogError("[Zombie War] CameraShakeController has missing dependencies.", this);
                enabled = false;
                return;
            }

            if (_bombController == null)
            {
                TryGetComponent(out _bombController);
            }
        }

        private void OnEnable()
        {
            if (_health == null || _weaponController == null)
            {
                return;
            }

            _health.Damaged += OnDamaged;
            _weaponController.Fired += OnWeaponFired;
            if (_bombController != null)
            {
                _bombController.Exploded += OnBombExploded;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
            }
            if (_weaponController != null)
            {
                _weaponController.Fired -= OnWeaponFired;
            }
            if (_bombController != null)
            {
                _bombController.Exploded -= OnBombExploded;
            }
        }
        #endregion

        #region API
        public void SetReferences(
            CinemachineImpulseSource impulseSource,
            Health health,
            WeaponController weaponController,
            BombController bombController)
        {
            _impulseSource = impulseSource;
            _health = health;
            _weaponController = weaponController;
            _bombController = bombController;
        }

        public void Shake(float force)
        {
            if (GameOptions.CameraShakeEnabled && _impulseSource != null && force > 0f)
            {
                _impulseSource.GenerateImpulseWithForce(force);
            }
        }
        #endregion

        #region Internal
        private void OnDamaged(DamageInfo damage)
        {
            float normalizedDamage = Mathf.Clamp01(damage.Amount / Mathf.Max(1f, _health.MaxHealth) * 4f);
            Shake(Mathf.Lerp(_minimumDamageForce, _maximumDamageForce, normalizedDamage));
        }

        private void OnWeaponFired(float recoil)
        {
            Shake(Mathf.Min(recoil * _weaponRecoilMultiplier, _maximumWeaponForce));
        }

        private void OnBombExploded(Vector3 position, float radius)
        {
            float distance = Vector3.Distance(transform.position, position);
            float proximity = 1f - Mathf.Clamp01(distance / Mathf.Max(0.01f, radius * 2f));
            Shake(_bombForce * Mathf.Lerp(0.35f, 1f, proximity));
        }
        #endregion
    }
}
