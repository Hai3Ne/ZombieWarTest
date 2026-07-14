using System;
using UnityEngine;
using ZombieWar.Core;
using ZombieWar.Enemies;

namespace ZombieWar.Combat
{
    public sealed class WeaponController : MonoBehaviour
    {
        private EnemyPool _enemyPool;
        private ProjectilePool _projectilePool;
        private Transform _muzzle;
        private WeaponConfig[] _weapons;
        private ZombieAgent _target;
        private int _weaponIndex;
        private float _nextTargetRefresh;
        private float _nextFireTime;

        public string CurrentWeaponName => _weapons != null ? _weapons[_weaponIndex].DisplayName : string.Empty;
        public ZombieAgent CurrentTarget => _target;
        public event Action<string> WeaponChanged;
        public event Action<float> Fired;

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
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 18f * Time.deltaTime);
            }
            if (GameplayMath.IsCooldownReady(Time.time, _nextFireTime))
            {
                Fire(_weapons[_weaponIndex], targetPoint);
            }
        }

        public void Configure(WeaponConfig[] weapons, EnemyPool enemyPool, ProjectilePool projectilePool, Transform muzzle)
        {
            _weapons = weapons;
            _enemyPool = enemyPool;
            _projectilePool = projectilePool;
            _muzzle = muzzle;
            WeaponChanged?.Invoke(CurrentWeaponName);
        }

        public void SwitchWeapon()
        {
            if (_weapons == null || _weapons.Length < 2)
            {
                return;
            }
            _weaponIndex = (_weaponIndex + 1) % _weapons.Length;
            _nextFireTime = Time.time + 0.1f;
            WeaponChanged?.Invoke(CurrentWeaponName);
        }

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
    }
}
