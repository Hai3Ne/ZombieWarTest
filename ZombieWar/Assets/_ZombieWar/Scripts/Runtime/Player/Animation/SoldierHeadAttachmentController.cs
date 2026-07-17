using UnityEngine;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class SoldierHeadAttachmentController : MonoBehaviour
    {
        #region Refs
        [SerializeField] private Transform[] _attachments;

        private Animator _animator;
        private Transform _head;
        #endregion

        #region State
        private Vector3[] _headLocalPositions;
        private Quaternion[] _headLocalRotations;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _animator)
                || !_animator.isHuman
                || _attachments == null
                || _attachments.Length == 0)
            {
                Debug.LogError("[Zombie War] Soldier head attachments require an authored Humanoid Animator and attachment list.", this);
                enabled = false;
                return;
            }

            _head = _animator.GetBoneTransform(HumanBodyBones.Head);
            if (_head == null)
            {
                Debug.LogError("[Zombie War] Soldier Humanoid avatar has no Head bone.", this);
                enabled = false;
                return;
            }

            _headLocalPositions = new Vector3[_attachments.Length];
            _headLocalRotations = new Quaternion[_attachments.Length];
            for (int i = 0; i < _attachments.Length; i++)
            {
                Transform attachment = _attachments[i];
                if (attachment == null)
                {
                    continue;
                }

                _headLocalPositions[i] = _head.InverseTransformPoint(attachment.position);
                _headLocalRotations[i] = Quaternion.Inverse(_head.rotation) * attachment.rotation;
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _attachments.Length; i++)
            {
                Transform attachment = _attachments[i];
                if (attachment == null)
                {
                    continue;
                }

                attachment.SetPositionAndRotation(
                    _head.TransformPoint(_headLocalPositions[i]),
                    _head.rotation * _headLocalRotations[i]);
            }
        }
        #endregion

        #region API
        public void SetAttachments(Transform[] attachments)
        {
            _attachments = attachments;
        }
        #endregion
    }
}
