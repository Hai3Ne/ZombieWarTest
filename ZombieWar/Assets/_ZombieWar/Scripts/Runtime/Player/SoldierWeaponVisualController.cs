using System;
using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(WeaponController))]
    public sealed class SoldierWeaponVisualController : MonoBehaviour
    {
        #region Refs
        [SerializeField] private GameObject[] _weaponModels;
        [SerializeField] private Transform[] _rightHandTargets;
        [SerializeField] private Transform[] _leftHandTargets;
        [SerializeField] private Transform[] _muzzleTargets;
        [SerializeField] private Transform _gameplayMuzzle;
        [SerializeField] private SoldierWeaponIkController _weaponIk;

        private WeaponController _weaponController;
        #endregion

        #region State
        private int _activeIndex;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _weaponController) || !HasValidReferences())
            {
                Debug.LogError("[Zombie War] Soldier weapon visuals require two authored weapon sets.", this);
                enabled = false;
                return;
            }

            _weaponController.WeaponChanged += OnWeaponChanged;
            ApplyWeapon(0);
        }

        private void LateUpdate()
        {
            SyncGameplayMuzzle();
        }

        private void OnDestroy()
        {
            if (_weaponController != null)
            {
                _weaponController.WeaponChanged -= OnWeaponChanged;
            }
        }
        #endregion

        #region API
        public void SetViewReferences(
            GameObject[] weaponModels,
            Transform[] rightHandTargets,
            Transform[] leftHandTargets,
            Transform[] muzzleTargets,
            Transform gameplayMuzzle,
            SoldierWeaponIkController weaponIk)
        {
            _weaponModels = weaponModels;
            _rightHandTargets = rightHandTargets;
            _leftHandTargets = leftHandTargets;
            _muzzleTargets = muzzleTargets;
            _gameplayMuzzle = gameplayMuzzle;
            _weaponIk = weaponIk;
        }
        #endregion

        #region Internal
        private void OnWeaponChanged(string weaponName)
        {
            ApplyWeapon(weaponName.IndexOf("SHOTGUN", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0);
        }

        private void ApplyWeapon(int index)
        {
            _activeIndex = Mathf.Clamp(index, 0, _weaponModels.Length - 1);
            for (int i = 0; i < _weaponModels.Length; i++)
            {
                _weaponModels[i].SetActive(i == _activeIndex);
            }

            _weaponIk.SetTargets(_leftHandTargets[_activeIndex], _rightHandTargets[_activeIndex]);
            SyncGameplayMuzzle();
        }

        private void SyncGameplayMuzzle()
        {
            Transform target = _muzzleTargets[_activeIndex];
            _gameplayMuzzle.SetPositionAndRotation(target.position, target.rotation);
        }

        private bool HasValidReferences()
        {
            const int WeaponCount = 2;
            return _weaponModels != null && _weaponModels.Length == WeaponCount
                && _rightHandTargets != null && _rightHandTargets.Length == WeaponCount
                && _leftHandTargets != null && _leftHandTargets.Length == WeaponCount
                && _muzzleTargets != null && _muzzleTargets.Length == WeaponCount
                && _gameplayMuzzle != null
                && _weaponIk != null;
        }
        #endregion
    }
}
