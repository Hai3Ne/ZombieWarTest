using System;
using UnityEngine;
using ZombieWar.Core;
using ZombieWar.Enemies;

namespace ZombieWar.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WeaponController : MonoBehaviour
    {
        #region Refs
        private EnemyPool _enemyPool;
        private ProjectilePool _projectilePool;
        private Transform _muzzle;
        private Rigidbody _rigidbody;
        #endregion

        #region State
        private WeaponConfig[] _weapons;
        private ZombieAgent _target;
        private int _weaponIndex;
        private float _nextTargetRefresh;
        private float _nextFireTime;
        private Vector3 _aimDirection;
        #endregion

        #region API
        public string CurrentWeaponName => _weapons != null ? _weapons[_weaponIndex].DisplayName : string.Empty;
        public int CurrentWeaponIndex => _weaponIndex;
        public int WeaponCount => _weapons?.Length ?? 0;
        public ZombieAgent CurrentTarget => _target;
        public event Action<int, string> WeaponChanged;
        public event Action<float> Fired;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody))
            {
                Debug.LogError("[Zombie War] WeaponController requires a Rigidbody.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (_weapons == null || _enemyPool == null)
            {
                return;
            }

            WeaponConfig weapon = _weapons[_weaponIndex];
            if (Time.time >= _nextTargetRefresh)
            {
                _nextTargetRefresh = Time.time + 0.15f;
                _target = _enemyPool.FindBestTarget(transform.position, _target, weapon.Range, 0.8f);
            }
            if (_target == null || !_target.IsAlive)
            {
                return;
            }

            Vector3 targetPoint = _target.AimPoint;
            Vector3 flat = targetPoint - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f)
            {
                _aimDirection = flat.normalized;
            }
            if (GameplayMath.IsCooldownReady(Time.time, _nextFireTime))
            {
                Fire(_weapons[_weaponIndex], targetPoint);
            }
        }

        private void FixedUpdate()
        {
            if (_aimDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(_aimDirection);
            float blend = 1f - Mathf.Exp(-18f * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, blend));
        }
        #endregion

        #region API
        public void Configure(WeaponConfig[] weapons, EnemyPool enemyPool, ProjectilePool projectilePool, Transform muzzle)
        {
            _weapons = weapons;
            _enemyPool = enemyPool;
            _projectilePool = projectilePool;
            _muzzle = muzzle;
            WeaponChanged?.Invoke(_weaponIndex, CurrentWeaponName);
        }

        public void SwitchWeapon()
        {
            if (_weapons == null || _weapons.Length < 2)
            {
                return;
            }
            SelectWeapon((_weaponIndex + 1) % _weapons.Length);
        }

        public WeaponConfig GetWeapon(int index)
        {
            return _weapons != null && index >= 0 && index < _weapons.Length ? _weapons[index] : null;
        }

        public void SelectWeapon(int index)
        {
            if (_weapons == null || index < 0 || index >= _weapons.Length || index == _weaponIndex)
            {
                return;
            }

            _weaponIndex = index;
            WeaponChanged?.Invoke(_weaponIndex, CurrentWeaponName);
        }
        #endregion

        #region Internal
        private void Fire(WeaponConfig weapon, Vector3 targetPoint)
        {
            _nextFireTime = Time.time + weapon.FireInterval;
            Vector3 baseDirection = (targetPoint - _muzzle.position).normalized;
            for (int i = 0; i < weapon.PelletCount; i++)
            {
                float yaw = UnityEngine.Random.Range(-weapon.SpreadDegrees, weapon.SpreadDegrees);
                float pitch = UnityEngine.Random.Range(-weapon.SpreadDegrees * 0.35f, weapon.SpreadDegrees * 0.35f);
                Vector3 direction = Quaternion.Euler(pitch, yaw, 0f) * baseDirection;
                _projectilePool.Fire(_muzzle.position, direction, weapon.ProjectileSpeed, weapon.Range, weapon.Damage, gameObject, weapon.AccentColor);
            }
            Fired?.Invoke(weapon.Recoil);
        }
        #endregion
    }
}
