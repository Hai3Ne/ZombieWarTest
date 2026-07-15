using UnityEngine;

namespace ZombieWar.Levels
{
    [CreateAssetMenu(menuName = "Zombie War/Waves/Camera Profile", fileName = "CameraProfile")]
    public sealed class CameraProfileConfig : ScriptableObject
    {
        #region Config
        [SerializeField] private string _displayName = "Landscape Top Down";
        [SerializeField] private Vector3 _followOffset = new(0f, 19f, -11f);
        [SerializeField] private Vector3 _rotationEuler = new(60f, 0f, 0f);
        [SerializeField, Range(20f, 90f)] private float _fieldOfView = 50f;
        [SerializeField] private Vector3 _positionDamping = new(0.22f, 0.22f, 0.22f);
        [SerializeField, Min(1f)] private float _previewSize = 12f;
        #endregion

        public string DisplayName => _displayName;
        public Vector3 FollowOffset => _followOffset;
        public Vector3 RotationEuler => _rotationEuler;
        public float FieldOfView => _fieldOfView;
        public Vector3 PositionDamping => _positionDamping;
        public float PreviewSize => _previewSize;

        public void Configure(
            string displayName,
            Vector3 followOffset,
            Vector3 rotationEuler,
            float fieldOfView,
            Vector3 positionDamping,
            float previewSize)
        {
            _displayName = displayName;
            _followOffset = followOffset;
            _rotationEuler = rotationEuler;
            _fieldOfView = Mathf.Clamp(fieldOfView, 20f, 90f);
            _positionDamping = positionDamping;
            _previewSize = Mathf.Max(1f, previewSize);
        }
    }
}
