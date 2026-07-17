using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZombieWar.Audio
{
    [CreateAssetMenu(menuName = "Zombie War/Audio/Player Audio Catalog")]
    public sealed class PlayerAudioCatalog : ScriptableObject
    {
        #region Refs
        [SerializeField] private AssetReferenceT<AudioClip>[] _hurtGrunts;
        #endregion

        #region API
        public AssetReferenceT<AudioClip>[] HurtGrunts => _hurtGrunts;

        public void Configure(AssetReferenceT<AudioClip>[] hurtGrunts)
        {
            _hurtGrunts = hurtGrunts;
        }
        #endregion
    }
}
