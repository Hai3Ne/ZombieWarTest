using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZombieWar.Combat
{
    [CreateAssetMenu(menuName = "Zombie War/Weapon Config", fileName = "WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private string _displayName = "Assault Rifle";
        [SerializeField] private float _damage = 18f;
        [SerializeField] private float _fireInterval = 0.12f;
        [SerializeField] private float _range = 18f;
        [SerializeField] private float _projectileSpeed = 32f;
        [SerializeField] private int _pelletCount = 1;
        [SerializeField] private float _spreadDegrees = 2f;
        [SerializeField] private float _recoil = 0.08f;
        [SerializeField] private Color _accentColor = new(1f, 0.75f, 0.2f);
        [SerializeField] private AssetReferenceSprite _icon;
        [SerializeField] private AssetReferenceT<GameObject> _viewPrefab;
        #endregion

        public string DisplayName => _displayName;
        public float Damage => _damage;
        public float FireInterval => _fireInterval;
        public float Range => _range;
        public float ProjectileSpeed => _projectileSpeed;
        public int PelletCount => _pelletCount;
        public float SpreadDegrees => _spreadDegrees;
        public float Recoil => _recoil;
        public Color AccentColor => _accentColor;
        public AssetReferenceSprite Icon => _icon;
        public AssetReferenceT<GameObject> ViewPrefab => _viewPrefab;

        public void Configure(
            string displayName,
            float damage,
            float fireInterval,
            float range,
            float projectileSpeed,
            int pelletCount,
            float spreadDegrees,
            float recoil,
            Color accentColor)
        {
            _displayName = displayName;
            _damage = damage;
            _fireInterval = fireInterval;
            _range = range;
            _projectileSpeed = projectileSpeed;
            _pelletCount = pelletCount;
            _spreadDegrees = spreadDegrees;
            _recoil = recoil;
            _accentColor = accentColor;
        }

        public void ConfigurePresentation(AssetReferenceSprite icon, AssetReferenceT<GameObject> viewPrefab)
        {
            _icon = icon;
            _viewPrefab = viewPrefab;
        }
    }
}
