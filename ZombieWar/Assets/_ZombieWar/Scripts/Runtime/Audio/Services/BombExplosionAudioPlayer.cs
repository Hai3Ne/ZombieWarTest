using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Combat;
using ZombieWar.Core;

namespace ZombieWar.Audio
{
    public sealed class BombExplosionAudioPlayer : MonoBehaviour
    {
        #region Refs
        [SerializeField] private BombAudioCatalog _catalog;
        [SerializeField] private BombController _bombController;
        [SerializeField] private AudioSource _audioSource;
        #endregion

        #region State
        private AsyncOperationHandle<AudioClip> _explosionHandle;
        private AudioClip _explosionClip;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_catalog == null || _bombController == null || _audioSource == null)
            {
                Debug.LogError("[Zombie War] Bomb explosion audio requires authored references.", this);
                enabled = false;
                return;
            }

            _bombController.Exploded += OnExploded;
            _explosionHandle = _catalog.Explosion.LoadAssetAsync();
            _explosionHandle.Completed += OnExplosionLoaded;
        }

        private void OnDestroy()
        {
            if (_bombController != null)
            {
                _bombController.Exploded -= OnExploded;
            }

            if (_explosionHandle.IsValid())
            {
                _explosionHandle.Completed -= OnExplosionLoaded;
                Addressables.Release(_explosionHandle);
            }
        }
        #endregion

        #region API
        public void SetReferences(BombAudioCatalog catalog, BombController bombController, AudioSource audioSource)
        {
            _catalog = catalog;
            _bombController = bombController;
            _audioSource = audioSource;
        }
        #endregion

        #region Internal
        private void OnExplosionLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _explosionClip = handle.Result;
                return;
            }

            Debug.LogError("[Zombie War] Addressable bomb explosion audio failed to load.", this);
        }

        private void OnExploded(Vector3 position, float radius)
        {
            if (!GameOptions.SfxEnabled || _explosionClip == null)
            {
                return;
            }

            _audioSource.transform.position = position;
            _audioSource.pitch = UnityEngine.Random.Range(0.94f, 1.03f);
            _audioSource.PlayOneShot(_explosionClip);
        }
        #endregion
    }
}
