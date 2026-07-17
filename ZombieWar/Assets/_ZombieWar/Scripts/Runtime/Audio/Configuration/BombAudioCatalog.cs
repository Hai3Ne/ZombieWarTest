using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZombieWar.Audio
{
    [CreateAssetMenu(menuName = "Zombie War/Audio/Bomb Audio Catalog")]
    public sealed class BombAudioCatalog : ScriptableObject
    {
        #region Refs
        [SerializeField] private AssetReferenceT<AudioClip> _explosion;
        #endregion

        #region API
        public AssetReferenceT<AudioClip> Explosion => _explosion;

        public void Configure(AssetReferenceT<AudioClip> explosion)
        {
            _explosion = explosion;
        }
        #endregion
    }
}
