using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.VFX
{
    [RequireComponent(typeof(WeaponController))]
    public sealed class WeaponMuzzleVfxController : MonoBehaviour
    {
        #region Config
        [SerializeField] private GameObject[] _effects;
        #endregion

        #region Refs
        private WeaponController _weapon;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            _weapon = GetComponent<WeaponController>();
            _weapon.Fired += OnFired;
        }

        private void OnDestroy()
        {
            if (_weapon != null)
            {
                _weapon.Fired -= OnFired;
            }
        }
        #endregion

        #region API
        public void SetViewReferences(GameObject[] effects)
        {
            _effects = effects;
        }
        #endregion

        #region Internal
        private void OnFired(float recoil)
        {
            int index = _weapon.CurrentWeaponIndex;
            if (_effects == null || index < 0 || index >= _effects.Length || _effects[index] == null)
            {
                return;
            }

            _effects[index].SetActive(false);
            _effects[index].SetActive(true);
        }
        #endregion
    }
}
