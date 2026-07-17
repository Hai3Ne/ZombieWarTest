using UnityEngine;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class SoldierWeaponIkController : MonoBehaviour
    {
        #region Config
        [SerializeField, Range(0f, 1f)] private float _positionWeight = 1f;
        [SerializeField, Range(0f, 1f)] private float _rotationWeight;
        #endregion

        #region Refs
        private Animator _animator;
        #endregion

        #region State
        private Transform _leftHandTarget;
        private Transform _rightHandTarget;
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

        }

        private void OnAnimatorIK(int layerIndex)
        {
            ApplyHand(AvatarIKGoal.LeftHand, _leftHandTarget);
            ApplyHand(AvatarIKGoal.RightHand, _rightHandTarget);
        }
        #endregion

        #region API
        public void SetTargets(Transform leftHandTarget, Transform rightHandTarget)
        {
            _leftHandTarget = leftHandTarget;
            _rightHandTarget = rightHandTarget;
        }
        #endregion

        #region Internal
        private void ApplyHand(AvatarIKGoal goal, Transform target)
        {
            if (target == null)
            {
                _animator.SetIKPositionWeight(goal, 0f);
                _animator.SetIKRotationWeight(goal, 0f);
                return;
            }

            _animator.SetIKPositionWeight(goal, _positionWeight);
            _animator.SetIKRotationWeight(goal, _rotationWeight);
            _animator.SetIKPosition(goal, target.position);
            _animator.SetIKRotation(goal, target.rotation);
        }

        #endregion
    }
}
