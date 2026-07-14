using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZombieWar.Audio
{
    [CreateAssetMenu(menuName = "Zombie War/Audio/Weapon Audio Catalog")]
    public sealed class WeaponAudioCatalog : ScriptableObject
    {
        #region Refs
        [SerializeField] private AssetReferenceT<AudioClip> _rifleFire;
        [SerializeField] private AssetReferenceT<AudioClip> _shotgunFire;
        #endregion

        #region API
        public AssetReferenceT<AudioClip> RifleFire => _rifleFire;
        public AssetReferenceT<AudioClip> ShotgunFire => _shotgunFire;

        public void Configure(
            AssetReferenceT<AudioClip> rifleFire,
            AssetReferenceT<AudioClip> shotgunFire)
        {
            _rifleFire = rifleFire;
            _shotgunFire = shotgunFire;
        }
        #endregion
    }
}
