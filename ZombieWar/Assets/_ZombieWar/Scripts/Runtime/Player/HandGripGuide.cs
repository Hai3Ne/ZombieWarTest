using UnityEngine;

namespace ZombieWar.Player
{
    public sealed class HandGripGuide : MonoBehaviour
    {
        #region Config
        [SerializeField] private bool _isLeftHand;
        [SerializeField, Min(0.03f)] private float _guideSize = 0.14f;
        #endregion

        #region API
        public bool IsLeftHand => _isLeftHand;
        public float GuideSize => _guideSize;
        public Vector3 FingerDirection => transform.forward;
        public Vector3 PalmNormal => transform.up;
        public Vector3 ThumbDirection => _isLeftHand ? -transform.right : transform.right;

        public void Configure(bool isLeftHand, float guideSize = 0.14f)
        {
            _isLeftHand = isLeftHand;
            _guideSize = Mathf.Max(0.03f, guideSize);
        }
        #endregion

        #region Editor
        private void OnDrawGizmosSelected()
        {
            float size = Mathf.Max(0.03f, _guideSize);
            Vector3 position = transform.position;

            Gizmos.color = new Color(0.1f, 0.9f, 1f, 1f);
            Gizmos.DrawLine(position, position + FingerDirection * size);

            Gizmos.color = new Color(1f, 0.82f, 0.12f, 1f);
            Gizmos.DrawLine(position, position + PalmNormal * size * 0.72f);

            Gizmos.color = new Color(1f, 0.2f, 0.75f, 1f);
            Gizmos.DrawLine(position, position + ThumbDirection * size * 0.72f);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(position, size * 0.08f);
        }
        #endregion
    }
}
