using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Audio;
using ZombieWar.Combat;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(WeaponController), typeof(AudioSource))]
    public sealed class SoldierWeaponAudioController : MonoBehaviour
    {
        #region Refs
        [SerializeField] private WeaponAudioCatalog _catalog;

        private WeaponController _weaponController;
        private AudioSource _audioSource;
        #endregion

        #region State
        private AsyncOperationHandle<AudioClip> _rifleHandle;
        private AsyncOperationHandle<AudioClip> _shotgunHandle;
        private AudioClip _rifleClip;
        private AudioClip _shotgunClip;
        private int _activeWeaponIndex;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_catalog == null
                || !TryGetComponent(out _weaponController)
                || !TryGetComponent(out _audioSource))
            {
                Debug.LogError("[Zombie War] Weapon audio requires an authored Addressables catalog.", this);
                enabled = false;
                return;
            }

            _weaponController.WeaponChanged += OnWeaponChanged;
            _weaponController.Fired += OnFired;
            LoadClips();
        }

        private void OnDestroy()
        {
            if (_weaponController != null)
            {
                _weaponController.WeaponChanged -= OnWeaponChanged;
                _weaponController.Fired -= OnFired;
            }

            ReleaseHandle(ref _rifleHandle, OnRifleLoaded);
            ReleaseHandle(ref _shotgunHandle, OnShotgunLoaded);
        }
        #endregion

        #region API
        public void SetCatalog(WeaponAudioCatalog catalog)
        {
            _catalog = catalog;
        }
        #endregion

        #region Internal
        private void LoadClips()
        {
            _rifleHandle = _catalog.RifleFire.LoadAssetAsync();
            _shotgunHandle = _catalog.ShotgunFire.LoadAssetAsync();
            _rifleHandle.Completed += OnRifleLoaded;
            _shotgunHandle.Completed += OnShotgunLoaded;
        }

        private void OnRifleLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            _rifleClip = ResolveClip(handle, "rifle");
        }

        private void OnShotgunLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            _shotgunClip = ResolveClip(handle, "shotgun");
        }

        private void OnWeaponChanged(int weaponIndex, string weaponName)
        {
            _activeWeaponIndex = Mathf.Clamp(weaponIndex, 0, 1);
        }

        private void OnFired(float recoil)
        {
            AudioClip clip = _activeWeaponIndex == 0 ? _rifleClip : _shotgunClip;
            if (clip == null)
            {
                return;
            }

            _audioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
            _audioSource.PlayOneShot(clip);
        }

        private AudioClip ResolveClip(AsyncOperationHandle<AudioClip> handle, string weaponName)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($"[Zombie War] Addressable {weaponName} fire audio failed to load.", this);
            return null;
        }

        private static void ReleaseHandle(
            ref AsyncOperationHandle<AudioClip> handle,
            Action<AsyncOperationHandle<AudioClip>> completed)
        {
            if (!handle.IsValid())
            {
                return;
            }

            handle.Completed -= completed;
            Addressables.Release(handle);
        }
        #endregion
    }
}
