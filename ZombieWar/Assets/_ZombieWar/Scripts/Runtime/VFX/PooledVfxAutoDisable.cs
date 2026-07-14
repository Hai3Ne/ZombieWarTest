using UnityEngine;

namespace ZombieWar.VFX
{
    public sealed class PooledVfxAutoDisable : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0.1f)] private float _duration = 4f;
        #endregion

        #region State
        private float _disableTime;
        #endregion

        #region Lifecycle
        private void OnEnable()
        {
            _disableTime = Time.time + _duration;
        }

        private void Update()
        {
            if (Time.time >= _disableTime)
            {
                gameObject.SetActive(false);
            }
        }
        #endregion

        #region API
        public void SetDuration(float duration)
        {
            _duration = Mathf.Max(0.1f, duration);
        }
        #endregion
    }
}
