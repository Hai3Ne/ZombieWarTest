using System;
using UnityEngine;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class SoldierWeaponIkController : MonoBehaviour
    {
        #region Config
        [SerializeField, Range(0f, 1f)] private float _leftHandPositionWeight = 0.72f;
        [SerializeField, Range(0f, 1f)] private float _leftHandRotationWeight;
        [SerializeField, Range(0f, 1f)] private float _rightHandPositionWeight;
        [SerializeField, Range(0f, 1f)] private float _rightHandRotationWeight;
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
            BeforeApplyIk?.Invoke();
            ApplyHand(AvatarIKGoal.LeftHand, _leftHandTarget, _leftHandPositionWeight, _leftHandRotationWeight);
            ApplyHand(AvatarIKGoal.RightHand, _rightHandTarget, _rightHandPositionWeight, _rightHandRotationWeight);
        }
        #endregion

        #region API
        public event Action BeforeApplyIk;

        public void SetTargets(Transform leftHandTarget, Transform rightHandTarget)
        {
            _leftHandTarget = leftHandTarget;
            _rightHandTarget = rightHandTarget;
        }
        #endregion

        #region Internal
        private void ApplyHand(AvatarIKGoal goal, Transform target, float positionWeight, float rotationWeight)
        {
            if (target == null)
            {
                _animator.SetIKPositionWeight(goal, 0f);
                _animator.SetIKRotationWeight(goal, 0f);
                return;
            }

            _animator.SetIKPositionWeight(goal, positionWeight);
            _animator.SetIKRotationWeight(goal, rotationWeight);
            _animator.SetIKPosition(goal, target.position);
            _animator.SetIKRotation(goal, target.rotation);
        }

        #endregion
    }
}
