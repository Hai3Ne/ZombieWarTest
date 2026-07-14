using UnityEngine;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class SoldierWeaponIkController : MonoBehaviour
    {
        #region Config
        [SerializeField, Range(0f, 1f)] private float _positionWeight = 0.92f;
        [SerializeField, Range(0f, 1f)] private float _rotationWeight;
        [SerializeField] private float _lowerBodyTurnSpeed = 720f;
        #endregion

        #region Refs
        private Animator _animator;
        private Transform _hips;
        private Transform _spine;
        #endregion

        #region State
        private Transform _rightHandTarget;
        private Transform _leftHandTarget;
        private float _targetLowerBodyYaw;
        private float _lowerBodyYaw;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _animator))
            {
                Debug.LogError("[Zombie War] Weapon IK requires an Animator on the same GameObject.", this);
                enabled = false;
                return;
            }

            _hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            _spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
        }

        private void Update()
        {
            _lowerBodyYaw = Mathf.MoveTowardsAngle(
                _lowerBodyYaw,
                _targetLowerBodyYaw,
                _lowerBodyTurnSpeed * Time.deltaTime);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            ApplyLowerBodyDirection();
            if (_rightHandTarget == null || _leftHandTarget == null)
            {
                return;
            }

            ApplyHand(AvatarIKGoal.RightHand, _rightHandTarget);
            ApplyHand(AvatarIKGoal.LeftHand, _leftHandTarget);
        }
        #endregion

        #region API
        public void SetTargets(Transform rightHandTarget, Transform leftHandTarget)
        {
            _rightHandTarget = rightHandTarget;
            _leftHandTarget = leftHandTarget;
        }

        public void SetLowerBodyDirection(Vector2 localMovement)
        {
            _targetLowerBodyYaw = localMovement.sqrMagnitude > 0.01f
                ? Mathf.Atan2(localMovement.x, localMovement.y) * Mathf.Rad2Deg
                : 0f;
        }
        #endregion

        #region Internal
        private void ApplyHand(AvatarIKGoal goal, Transform target)
        {
            _animator.SetIKPositionWeight(goal, _positionWeight);
            _animator.SetIKRotationWeight(goal, _rotationWeight);
            _animator.SetIKPosition(goal, target.position);
            _animator.SetIKRotation(goal, target.rotation);
        }

        private void ApplyLowerBodyDirection()
        {
            if (_hips == null || _spine == null || Mathf.Abs(_lowerBodyYaw) < 0.1f)
            {
                return;
            }

            Quaternion upperBodyWorldRotation = _spine.rotation;
            _hips.rotation = Quaternion.AngleAxis(_lowerBodyYaw, transform.up) * _hips.rotation;
            _spine.rotation = upperBodyWorldRotation;
        }
        #endregion
    }
}
