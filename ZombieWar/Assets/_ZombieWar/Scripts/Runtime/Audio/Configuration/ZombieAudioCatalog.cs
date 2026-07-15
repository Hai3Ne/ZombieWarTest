using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZombieWar.Audio
{
    [CreateAssetMenu(menuName = "Zombie War/Audio/Zombie Audio Catalog")]
    public sealed class ZombieAudioCatalog : ScriptableObject
    {
        #region Refs
        [SerializeField] private AssetLabelReference _voiceLabel;
        #endregion

        #region API
        public AssetLabelReference VoiceLabel => _voiceLabel;

        public void Configure(AssetLabelReference voiceLabel)
        {
            _voiceLabel = voiceLabel;
        }
        #endregion
    }
}
