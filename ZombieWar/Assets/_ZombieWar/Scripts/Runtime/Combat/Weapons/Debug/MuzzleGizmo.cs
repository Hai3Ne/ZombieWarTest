using UnityEngine;

namespace ZombieWar.Combat
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Zombie War/Debug/Muzzle Gizmo")]
    public sealed class MuzzleGizmo : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0.005f)] private float _sphereRadius = 0.045f;
        [SerializeField, Min(0.05f)] private float _directionLength = 0.35f;
        [SerializeField] private Color _sphereColor = new(1f, 0.55f, 0.05f, 0.95f);
        [SerializeField] private Color _directionColor = new(0.1f, 0.85f, 1f, 1f);
        #endregion

        #region Lifecycle
        private void OnDrawGizmos()
        {
            DrawMuzzleGizmo();
        }
        #endregion

        #region Internal
        private void DrawMuzzleGizmo()
        {
            Vector3 position = transform.position;

            Gizmos.color = _sphereColor;
            Gizmos.DrawSphere(position, _sphereRadius * 0.45f);
            Gizmos.DrawWireSphere(position, _sphereRadius);

            Gizmos.color = _directionColor;
            Gizmos.DrawLine(position, position + transform.forward * _directionLength);
        }
        #endregion
    }
}
