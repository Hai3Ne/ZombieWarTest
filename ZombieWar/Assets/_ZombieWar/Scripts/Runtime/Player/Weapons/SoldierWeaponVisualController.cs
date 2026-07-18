using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.Player
{
    [RequireComponent(typeof(WeaponController))]
    public sealed class SoldierWeaponVisualController : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0f)] private float _recoilKickDistance = 0.09f;
        [SerializeField, Min(0f)] private float _recoilPitchAngle = 11f;
        [SerializeField, Min(0f)] private float _recoilRecoverySpeed = 16f;
        [SerializeField, Min(0f)] private float _movementBobFrequency = 8.5f;
        [SerializeField, Min(0f)] private float _movementBobDistance = 0.018f;
        [SerializeField, Min(0f)] private float _movementSwayDistance = 0.026f;
        [SerializeField, Min(0f)] private float _movementRollAngle = 4.5f;
        [SerializeField, Min(0f)] private float _damageJoltDistance = 0.075f;
        [SerializeField, Min(0f)] private float _damageJoltAngle = 8f;
        [SerializeField, Min(0f)] private float _damageJoltRecoverySpeed = 10f;
        #endregion

        #region Refs
        [SerializeField] private GameObject[] _weaponModels;
        [SerializeField] private Transform[] _rightHandTargets;
        [SerializeField] private Transform[] _leftHandTargets;
        [SerializeField] private Transform[] _muzzleTargets;
        [SerializeField] private Transform _gameplayMuzzle;
        [SerializeField] private SoldierWeaponIkController _weaponIk;

        private WeaponController _weaponController;
        private Rigidbody _rigidbody;
        private Health _health;
        private Transform _rightHandAnchor;
        #endregion

        #region State
        private int _activeIndex;
        private Transform[] _motionRoots;
        private Vector3[] _baseLocalPositions;
        private Quaternion[] _baseLocalRotations;
        private float _visualRecoil;
        private float _damageJolt;
        private float _bobTime;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (!TryGetComponent(out _weaponController)
                || !TryGetComponent(out _rigidbody)
                || !TryGetComponent(out _health)
                || !HasValidReferences())
            {
                Debug.LogError("[Zombie War] Soldier weapon visuals require authored weapon sets, Rigidbody and Health.", this);
                enabled = false;
                return;
            }

            Animator animator = GetComponentInChildren<Animator>(true);
            _rightHandAnchor = animator != null ? animator.GetBoneTransform(HumanBodyBones.RightHand) : null;
            if (_rightHandAnchor == null)
            {
                Debug.LogError("[Zombie War] Soldier weapon visuals require a humanoid RightHand bone.", this);
                enabled = false;
                return;
            }

            _weaponController.WeaponChanged += OnWeaponChanged;
            _weaponController.Fired += OnFired;
            _health.Damaged += OnDamaged;
            _weaponIk.BeforeApplyIk += PrepareActiveWeaponPoseForIk;
            CacheWeaponPose();
            ApplyWeapon(0);
        }

        private void Update()
        {
            _visualRecoil = Mathf.MoveTowards(_visualRecoil, 0f, _recoilRecoverySpeed * Time.deltaTime);
            _damageJolt = Mathf.MoveTowards(_damageJolt, 0f, _damageJoltRecoverySpeed * Time.deltaTime);
        }

        private void LateUpdate()
        {
            RestoreActiveWeaponPose();
            AlignActiveWeaponToAim();
            ApplyWeaponMotion();
            SyncGameplayMuzzle();
        }

        private void OnDestroy()
        {
            if (_weaponController != null)
            {
                _weaponController.WeaponChanged -= OnWeaponChanged;
                _weaponController.Fired -= OnFired;
            }

            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
            }

            if (_weaponIk != null)
            {
                _weaponIk.BeforeApplyIk -= PrepareActiveWeaponPoseForIk;
            }
        }
        #endregion

        #region API
        public void SetViewReferences(
            GameObject[] weaponModels,
            Transform[] rightHandTargets,
            Transform[] leftHandTargets,
            Transform[] muzzleTargets,
            Transform gameplayMuzzle,
            SoldierWeaponIkController weaponIk)
        {
            _weaponModels = weaponModels;
            _rightHandTargets = rightHandTargets;
            _leftHandTargets = leftHandTargets;
            _muzzleTargets = muzzleTargets;
            _gameplayMuzzle = gameplayMuzzle;
            _weaponIk = weaponIk;
        }
        #endregion

        #region Internal
        private void OnWeaponChanged(int weaponIndex, string weaponName)
        {
            ApplyWeapon(weaponIndex);
        }

        private void OnFired(float recoil)
        {
            _visualRecoil = Mathf.Max(_visualRecoil, Mathf.Clamp01(recoil * 5f));
        }

        private void OnDamaged(DamageInfo damage)
        {
            _damageJolt = 1f;
        }

        private void ApplyWeapon(int index)
        {
            RestoreActiveWeaponPose();
            _activeIndex = Mathf.Clamp(index, 0, _weaponModels.Length - 1);
            for (int i = 0; i < _weaponModels.Length; i++)
            {
                _weaponModels[i].SetActive(i == _activeIndex);
            }

            _weaponIk.SetTargets(_leftHandTargets[_activeIndex], _rightHandTargets[_activeIndex]);
            AlignActiveWeaponToAim();
            ApplyWeaponMotion();
            SyncGameplayMuzzle();
        }

        private void PrepareActiveWeaponPoseForIk()
        {
            RestoreActiveWeaponPose();
            AlignActiveWeaponToAim();
            ApplyWeaponMotion();
            SyncGameplayMuzzle();
        }

        private void SyncGameplayMuzzle()
        {
            Transform target = _muzzleTargets[_activeIndex];
            _gameplayMuzzle.SetPositionAndRotation(target.position, target.rotation);
        }

        private void CacheWeaponPose()
        {
            _motionRoots = new Transform[_weaponModels.Length];
            _baseLocalPositions = new Vector3[_weaponModels.Length];
            _baseLocalRotations = new Quaternion[_weaponModels.Length];
            for (int i = 0; i < _weaponModels.Length; i++)
            {
                Transform motionRoot = ResolveMotionRoot(_weaponModels[i].transform);
                _motionRoots[i] = motionRoot;
                _baseLocalPositions[i] = motionRoot.localPosition;
                _baseLocalRotations[i] = motionRoot.localRotation;
            }
        }

        private void RestoreActiveWeaponPose()
        {
            if (_baseLocalPositions == null || _baseLocalRotations == null || _activeIndex >= _weaponModels.Length)
            {
                return;
            }

            Transform motionRoot = _motionRoots[_activeIndex];
            motionRoot.localPosition = _baseLocalPositions[_activeIndex];
            motionRoot.localRotation = _baseLocalRotations[_activeIndex];
        }

        private void AlignActiveWeaponToAim()
        {
            Transform weaponRoot = _weaponModels[_activeIndex].transform;
            Transform rightGrip = _rightHandTargets[_activeIndex];
            Transform muzzle = _muzzleTargets[_activeIndex];
            if (rightGrip == null || muzzle == null)
            {
                return;
            }

            Vector3 aimDirection = ResolveWeaponAimDirection(weaponRoot);
            Quaternion muzzleOffsetFromWeaponRoot = Quaternion.Inverse(weaponRoot.rotation) * muzzle.rotation;
            Quaternion desiredMuzzleRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
            weaponRoot.rotation = desiredMuzzleRotation * Quaternion.Inverse(muzzleOffsetFromWeaponRoot);
            weaponRoot.position += _rightHandAnchor.position - rightGrip.position;
        }

        private Vector3 ResolveWeaponAimDirection(Transform weaponRoot)
        {
            Vector3 direction = _weaponController.AimDirection;
            direction.y = 0f;
            if (direction.sqrMagnitude >= 0.01f)
            {
                return direction.normalized;
            }

            direction = transform.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude >= 0.01f)
            {
                return direction.normalized;
            }

            direction = weaponRoot.forward;
            direction.y = 0f;
            return direction.sqrMagnitude >= 0.01f ? direction.normalized : Vector3.forward;
        }

        private void ApplyWeaponMotion()
        {
            Transform motionRoot = _motionRoots[_activeIndex];
            Transform referenceRoot = motionRoot.parent != null ? motionRoot.parent : transform;
            Vector3 localVelocity = referenceRoot.InverseTransformDirection(_rigidbody.linearVelocity);
            localVelocity.y = 0f;
            float speed01 = Mathf.Clamp01(localVelocity.magnitude / 6f);
            if (speed01 > 0.02f)
            {
                _bobTime += Time.deltaTime * Mathf.Lerp(_movementBobFrequency * 0.65f, _movementBobFrequency, speed01);
            }

            float bob = Mathf.Sin(_bobTime * Mathf.PI * 2f);
            float step = Mathf.Cos(_bobTime * Mathf.PI * 2f);
            float lateral = Mathf.Clamp(localVelocity.x / 6f, -1f, 1f);

            Vector3 movementOffset = new(
                lateral * _movementSwayDistance,
                Mathf.Abs(step) * _movementBobDistance * speed01,
                bob * _movementBobDistance * 0.35f * speed01);
            Quaternion movementRotation = Quaternion.Euler(
                -Mathf.Abs(step) * _movementRollAngle * 0.25f * speed01,
                lateral * _movementRollAngle * 0.55f,
                -lateral * _movementRollAngle);

            Vector3 recoilOffset = Vector3.back * (_visualRecoil * _recoilKickDistance);
            Quaternion recoilRotation = Quaternion.Euler(-_visualRecoil * _recoilPitchAngle, 0f, 0f);
            Vector3 damageOffset = new(0f, _damageJolt * _damageJoltDistance * 0.4f, -_damageJolt * _damageJoltDistance);
            Quaternion damageRotation = Quaternion.Euler(_damageJolt * _damageJoltAngle, _damageJolt * _damageJoltAngle * 0.35f, 0f);

            if (movementOffset.sqrMagnitude <= 0.000001f && _visualRecoil <= 0f && _damageJolt <= 0f)
            {
                return;
            }

            motionRoot.localPosition = _baseLocalPositions[_activeIndex] + movementOffset + recoilOffset + damageOffset;
            motionRoot.localRotation = _baseLocalRotations[_activeIndex] * movementRotation * recoilRotation * damageRotation;
        }

        private static Transform ResolveMotionRoot(Transform weaponRoot)
        {
            for (int i = 0; i < weaponRoot.childCount; i++)
            {
                Transform child = weaponRoot.GetChild(i);
                if (child.name.Contains("Motion Root"))
                {
                    return child;
                }
            }

            for (int i = 0; i < weaponRoot.childCount; i++)
            {
                Transform child = weaponRoot.GetChild(i);
                if (child.name.Contains("Model"))
                {
                    return child;
                }
            }

            return weaponRoot;
        }

        private bool HasValidReferences()
        {
            const int WeaponCount = 2;
            return _weaponModels != null && _weaponModels.Length == WeaponCount
                && _rightHandTargets != null && _rightHandTargets.Length == WeaponCount
                && _leftHandTargets != null && _leftHandTargets.Length == WeaponCount
                && _muzzleTargets != null && _muzzleTargets.Length == WeaponCount
                && _gameplayMuzzle != null
                && _weaponIk != null;
        }
        #endregion
    }
}
