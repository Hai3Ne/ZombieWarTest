using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Combat;
using ZombieWar.Core;

namespace ZombieWar.Audio
{
    public sealed class SoldierDamageAudioPlayer : MonoBehaviour
    {
        #region Config
        [SerializeField] private float _minIntervalSeconds = 0.6f;
        [SerializeField] private float _minPitch = 0.95f;
        [SerializeField] private float _maxPitch = 1.06f;
        #endregion

        #region Refs
        [SerializeField] private PlayerAudioCatalog _catalog;
        [SerializeField] private Health _health;
        [SerializeField] private AudioSource _audioSource;
        #endregion

        #region State
        private readonly List<AsyncOperationHandle<AudioClip>> _handles = new();
        private readonly List<AudioClip> _clips = new();
        private float _nextPlayTime;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_catalog == null || _health == null || _audioSource == null)
            {
                Debug.LogError("[Zombie War] SoldierDamageAudioPlayer requires authored references.", this);
                enabled = false;
                return;
            }

            _health.Damaged += OnDamaged;
            LoadClips();
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
            }

            for (int i = 0; i < _handles.Count; i++)
            {
                if (_handles[i].IsValid())
                {
                    Addressables.Release(_handles[i]);
                }
            }
        }
        #endregion

        #region API
        public void SetReferences(PlayerAudioCatalog catalog, Health health, AudioSource audioSource)
        {
            _catalog = catalog;
            _health = health;
            _audioSource = audioSource;
        }
        #endregion

        #region Internal
        private void LoadClips()
        {
            AssetReferenceT<AudioClip>[] references = _catalog.HurtGrunts;
            for (int i = 0; i < references.Length; i++)
            {
                AsyncOperationHandle<AudioClip> handle = references[i].LoadAssetAsync();
                handle.Completed += OnClipLoaded;
                _handles.Add(handle);
            }
        }

        private void OnClipLoaded(AsyncOperationHandle<AudioClip> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _clips.Add(handle.Result);
                return;
            }

            Debug.LogError("[Zombie War] Addressable soldier hurt audio failed to load.", this);
        }

        private void OnDamaged(DamageInfo damage)
        {
            if (!GameOptions.SfxEnabled || _clips.Count == 0 || Time.time < _nextPlayTime)
            {
                return;
            }

            _nextPlayTime = Time.time + _minIntervalSeconds;
            _audioSource.pitch = Random.Range(_minPitch, _maxPitch);
            _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Count)]);
        }
        #endregion
    }
}
