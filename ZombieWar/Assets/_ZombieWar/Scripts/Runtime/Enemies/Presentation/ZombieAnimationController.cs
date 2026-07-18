using UnityEngine;

namespace ZombieWar.Enemies
{
    public sealed class ZombieAnimationController : MonoBehaviour
    {
        private static readonly int MoveSpeedId = Animator.StringToHash("MoveSpeed");
        private static readonly int AttackId = Animator.StringToHash("Attack");
        private static readonly int HitId = Animator.StringToHash("Hit");
        private static readonly int DeadId = Animator.StringToHash("Dead");

        #region Refs
        [SerializeField] private Animator _animator;
        #endregion

        #region State
        private bool _isMoving;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_animator == null)
            {
                Debug.LogError("[Zombie War] Zombie Animator was not authored.", this);
                enabled = false;
                return;
            }

            _animator.applyRootMotion = false;
            _animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
        #endregion

        #region API
        public void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void ResetForSpawn()
        {
            _isMoving = false;
            _animator.speed = Random.Range(0.92f, 1.08f);
            _animator.Rebind();
            _animator.Update(0f);
            _animator.SetBool(DeadId, false);
            _animator.SetFloat(MoveSpeedId, 0f);
        }

        public void SetMoving(bool isMoving)
        {
            if (_isMoving == isMoving)
            {
                return;
            }

            _isMoving = isMoving;
            _animator.SetFloat(MoveSpeedId, isMoving ? 1f : 0f);
        }

        public void PlayAttack()
        {
            _animator.SetTrigger(AttackId);
        }

        public void PlayHit()
        {
            _animator.SetTrigger(HitId);
        }

        public void PlayDeath()
        {
            _isMoving = false;
            _animator.ResetTrigger(AttackId);
            _animator.ResetTrigger(HitId);
            _animator.SetFloat(MoveSpeedId, 0f);
            _animator.SetBool(DeadId, true);
            _animator.CrossFadeInFixedTime("Dead", 0.04f, 0, 0f);
        }
        #endregion
    }
}
