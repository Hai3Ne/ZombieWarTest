using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZombieWar.Enemies
{
    [CreateAssetMenu(menuName = "Zombie War/Enemies/Enemy Prefab Catalog")]
    public sealed class EnemyPrefabCatalog : ScriptableObject
    {
        #region Refs
        [SerializeField] private AssetReferenceT<GameObject> _zombie;
        #endregion

        #region API
        public AssetReferenceT<GameObject> Zombie => _zombie;

        public void Configure(AssetReferenceT<GameObject> zombie)
        {
            _zombie = zombie;
        }
        #endregion
    }
}
