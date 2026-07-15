using TMPro;
using UnityEngine;

namespace ZombieWar.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class FloatingCombatText : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0.1f)] private float _lifetime = 1.1f;
        [SerializeField, Min(0f)] private float _riseSpeed = 0.9f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0.85f, 1f, 1.15f);
        #endregion

        #region Refs
        [SerializeField] private TMP_Text _label;
        private FloatingCombatTextPool _pool;
        private Camera _worldCamera;
        #endregion

        #region State
        private Color _baseColor;
        private Vector3 _origin;
        private Vector3 _drift;
        private Vector3 _authoredScale;
        private float _elapsed;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_label == null && !TryGetComponent(out _label))
            {
                Debug.LogError("[Zombie War] FloatingCombatText requires a TMP_Text.", this);
                enabled = false;
            }
            if (_authoredScale == Vector3.zero)
            {
                _authoredScale = transform.localScale;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(_elapsed / _lifetime);
            transform.position = _origin + Vector3.up * (_riseSpeed * _elapsed) + _drift * _elapsed;
            transform.localScale = _authoredScale * _scaleCurve.Evaluate(normalized);
            if (_worldCamera != null)
            {
                transform.rotation = _worldCamera.transform.rotation;
            }

            Color color = _baseColor;
            color.a = 1f - normalized * normalized;
            _label.color = color;
            if (_elapsed >= _lifetime)
            {
                _pool.Release(this);
            }
        }
        #endregion

        #region API
        public void Initialize(FloatingCombatTextPool pool)
        {
            _pool = pool;
            _authoredScale = transform.localScale;
            gameObject.SetActive(false);
        }

        public void ShowDamage(float amount, Vector3 position, Color color, Camera worldCamera)
        {
            Prepare(position, color, worldCamera);
            _label.SetText("-{0:0}", amount);
            _label.ForceMeshUpdate();
        }

        public void ShowHealing(float amount, Vector3 position, Color color, Camera worldCamera)
        {
            Prepare(position, color, worldCamera);
            _label.SetText("+{0:0}", amount);
            _label.ForceMeshUpdate();
        }

        public void SetLabel(TMP_Text label)
        {
            _label = label;
        }
        #endregion

        #region Internal
        private void Prepare(Vector3 position, Color color, Camera worldCamera)
        {
            _origin = position + new Vector3(Random.Range(-0.18f, 0.18f), 0f, Random.Range(-0.08f, 0.08f));
            _drift = new Vector3(Random.Range(-0.22f, 0.22f), 0f, 0f);
            _baseColor = color;
            _worldCamera = worldCamera;
            _elapsed = 0f;
            transform.SetPositionAndRotation(_origin, worldCamera != null ? worldCamera.transform.rotation : Quaternion.identity);
            transform.localScale = _authoredScale * _scaleCurve.Evaluate(0f);
            gameObject.SetActive(true);
        }
        #endregion
    }
}
