using UnityEngine;

namespace ZombieWar.Player
{
    public sealed class SoldierAnimationController : MonoBehaviour
    {
        #region Config
        [SerializeField] private float _locomotionDampTime = 0.12f;
        [SerializeField] private float _recoilAngle = 7f;
        [SerializeField] private Color _damageColor = new(1f, 0.22f, 0.12f, 1f);
        #endregion

        #region Refs
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private SoldierWeaponIkController _weaponIk;
        #endregion

        #region State
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int FireHash = Animator.StringToHash("Fire");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        private MaterialPropertyBlock _propertyBlock;
        private Quaternion _baseLocalRotation;
        private float _recoil;
        private float _damageFlashUntil;
        private bool _isFlashing;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_animator == null || _visualRoot == null || _renderers == null || _renderers.Length == 0 || _weaponIk == null)
            {
                Debug.LogError("[Zombie War] SoldierAnimationController requires authored visual references.", this);
                enabled = false;
                return;
            }

            _animator.applyRootMotion = false;
            _baseLocalRotation = _visualRoot.localRotation;
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            _recoil = Mathf.MoveTowards(_recoil, 0f, 38f * Time.deltaTime);

            if (_isFlashing && Time.time >= _damageFlashUntil)
            {
                _isFlashing = false;
                ApplyRendererColors(false);
            }
        }

        private void LateUpdate()
        {
            _visualRoot.localRotation = _baseLocalRotation * Quaternion.Euler(-_recoil, 0f, 0f);
        }
        #endregion

        #region API
        public void SetViewReferences(
            Animator animator,
            Transform visualRoot,
            Renderer[] renderers,
            SoldierWeaponIkController weaponIk)
        {
            _animator = animator;
            _visualRoot = visualRoot;
            _renderers = renderers;
            _weaponIk = weaponIk;
        }

        public void SetMovement(Vector3 worldMovement)
        {
            if (_animator != null)
            {
                Vector3 localMovement = _visualRoot.InverseTransformDirection(worldMovement);
                float speed = Mathf.Clamp01(localMovement.magnitude);
                _animator.SetFloat(SpeedHash, speed, _locomotionDampTime, Time.deltaTime);
                _animator.SetFloat(MoveXHash, localMovement.x, _locomotionDampTime, Time.deltaTime);
                _animator.SetFloat(MoveYHash, localMovement.z, _locomotionDampTime, Time.deltaTime);
            }
        }

        public void TriggerFire(float recoil)
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetTrigger(FireHash);
            _recoil = Mathf.Max(_recoil, _recoilAngle * Mathf.Clamp(recoil * 5f, 0.65f, 1.4f));
        }

        public void TriggerHit()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetTrigger(HitHash);
            _damageFlashUntil = Time.time + 0.13f;
            if (!_isFlashing)
            {
                _isFlashing = true;
                ApplyRendererColors(true);
            }
        }

        public void SetDead()
        {
            if (_animator != null)
            {
                _animator.SetBool(DeadHash, true);
            }
        }
        #endregion

        #region Internal
        private void ApplyRendererColors(bool damaged)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer target = _renderers[i];
                if (target == null)
                {
                    continue;
                }

                if (damaged)
                {
                    target.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor("_BaseColor", _damageColor);
                    target.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    target.SetPropertyBlock(null);
                }
            }
        }
        #endregion
    }
}
