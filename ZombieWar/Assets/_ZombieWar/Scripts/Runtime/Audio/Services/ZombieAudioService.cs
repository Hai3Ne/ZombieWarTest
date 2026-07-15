using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Core;

namespace ZombieWar.Audio
{
    public sealed class ZombieAudioService : MonoBehaviour
    {
        #region Refs
        [SerializeField] private ZombieAudioCatalog _catalog;
        [SerializeField] private AudioSource[] _sources;
        #endregion

        #region State
        private readonly List<AudioClip> _ambientClips = new(4);
        private readonly List<AudioClip> _attackClips = new(2);
        private readonly List<AudioClip> _hitClips = new(1);
        private readonly List<AudioClip> _deathClips = new(2);
        private AsyncOperationHandle<IList<AudioClip>> _clipsHandle;
        private int _sourceCursor;
        private float _lastAmbientTime = float.MinValue;
        private float _lastAttackTime = float.MinValue;
        private float _lastHitTime = float.MinValue;
        private float _lastDeathTime = float.MinValue;
        public bool IsReady { get; private set; }
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_catalog == null
                || _catalog.VoiceLabel == null
                || !_catalog.VoiceLabel.RuntimeKeyIsValid()
                || _sources == null
                || _sources.Length == 0)
            {
                Debug.LogError("[Zombie War] Zombie audio requires an authored catalog and AudioSource pool.", this);
                enabled = false;
                return;
            }

            _clipsHandle = Addressables.LoadAssetsAsync<AudioClip>(
                _catalog.VoiceLabel.RuntimeKey,
                OnClipLoaded);
            _clipsHandle.Completed += OnClipsLoaded;
        }

        private void OnDestroy()
        {
            if (!_clipsHandle.IsValid())
            {
                return;
            }

            _clipsHandle.Completed -= OnClipsLoaded;
            Addressables.Release(_clipsHandle);
        }
        #endregion

        #region API
        public void SetReferences(ZombieAudioCatalog catalog, AudioSource[] sources)
        {
            _catalog = catalog;
            _sources = sources;
        }

        public void PlayAmbient(Vector3 position, bool isGiant)
        {
            Play(_ambientClips, position, isGiant, 0.75f, 0.55f, ref _lastAmbientTime);
        }

        public void PlayAttack(Vector3 position, bool isGiant)
        {
            Play(_attackClips, position, isGiant, 0.35f, 0.75f, ref _lastAttackTime);
        }

        public void PlayHit(Vector3 position, bool isGiant)
        {
            Play(_hitClips, position, isGiant, 0.28f, 0.6f, ref _lastHitTime);
        }

        public void PlayDeath(Vector3 position, bool isGiant)
        {
            Play(_deathClips, position, isGiant, 0.18f, 0.82f, ref _lastDeathTime);
        }
        #endregion

        #region Internal
        private void OnClipLoaded(AudioClip clip)
        {
            if (clip.name.IndexOf("Death", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _deathClips.Add(clip);
                return;
            }
            if (clip.name.IndexOf("Aggressive", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _attackClips.Add(clip);
                return;
            }
            if (clip.name.IndexOf("Grunt", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _hitClips.Add(clip);
                return;
            }

            _ambientClips.Add(clip);
        }

        private void OnClipsLoaded(AsyncOperationHandle<IList<AudioClip>> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[Zombie War] Addressable zombie voices failed to load.", this);
                return;
            }

            IsReady = _ambientClips.Count > 0
                && _attackClips.Count > 0
                && _hitClips.Count > 0
                && _deathClips.Count > 0;
            if (!IsReady)
            {
                Debug.LogError("[Zombie War] Zombie voice catalog is missing a required sound category.", this);
            }
        }

        private void Play(
            List<AudioClip> clips,
            Vector3 position,
            bool isGiant,
            float minimumInterval,
            float volumeScale,
            ref float lastPlayTime)
        {
            if (!GameOptions.SfxEnabled
                || !IsReady
                || clips.Count == 0
                || Time.time - lastPlayTime < minimumInterval)
            {
                return;
            }

            AudioSource source = GetAvailableSource();
            if (source == null)
            {
                return;
            }

            lastPlayTime = Time.time;
            source.transform.position = position;
            source.pitch = isGiant
                ? UnityEngine.Random.Range(0.72f, 0.82f)
                : UnityEngine.Random.Range(0.92f, 1.06f);
            source.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Count)], volumeScale);
        }

        private AudioSource GetAvailableSource()
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                int index = (_sourceCursor + i) % _sources.Length;
                if (_sources[index].isPlaying)
                {
                    continue;
                }

                _sourceCursor = (index + 1) % _sources.Length;
                return _sources[index];
            }

            return null;
        }
        #endregion
    }
}
