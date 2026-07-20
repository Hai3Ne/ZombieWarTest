using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Audio;
using ZombieWar.Combat;
using ZombieWar.Core;

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
        private AsyncOperationHandle<AudioClip> _sniperHandle;
        private AudioClip _rifleClip;
        private AudioClip _shotgunClip;
        private AudioClip _sniperClip;
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
            ReleaseHandle(ref _sniperHandle, OnSniperLoaded);
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
            _sniperHandle = _catalog.SniperFire.LoadAssetAsync();
            _rifleHandle.Completed += OnRifleLoaded;
            _shotgunHandle.Completed += OnShotgunLoaded;
            _sniperHandle.Completed += OnSniperLoaded;
        }

        private void OnRifleLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            _rifleClip = ResolveClip(handle, "rifle");
        }

        private void OnShotgunLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            _shotgunClip = ResolveClip(handle, "shotgun");
        }

        private void OnSniperLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            _sniperClip = ResolveClip(handle, "sniper");
        }

        private void OnWeaponChanged(int weaponIndex, string weaponName)
        {
            _activeWeaponIndex = Mathf.Clamp(weaponIndex, 0, 2);
        }

        private void OnFired(float recoil)
        {
            AudioClip clip = ResolveActiveClip();
            if (!GameOptions.SfxEnabled || clip == null)
            {
                return;
            }

            _audioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
            _audioSource.PlayOneShot(clip);
        }

        private AudioClip ResolveActiveClip()
        {
            return _activeWeaponIndex switch
            {
                0 => _rifleClip,
                1 => _shotgunClip,
                2 => _sniperClip,
                _ => null
            };
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
