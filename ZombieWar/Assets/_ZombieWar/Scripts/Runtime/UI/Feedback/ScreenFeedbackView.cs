using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.UI
{
    public sealed class ScreenFeedbackView : MonoBehaviour
    {
        #region Config
        [SerializeField, Min(0.01f)] private float _damageFadeDuration = 0.32f;
        [SerializeField, Range(0f, 1f)] private float _damageMaximumAlpha = 0.32f;
        [SerializeField, Min(0.01f)] private float _healingFadeDuration = 0.55f;
        [SerializeField, Range(0f, 1f)] private float _healingMaximumAlpha = 0.2f;
        [SerializeField, Range(0.05f, 0.75f)] private float _lowHealthThreshold = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _lowHealthMaximumAlpha = 0.16f;
        [SerializeField, Min(0.1f)] private float _lowHealthPulseSpeed = 1.6f;
        #endregion

        #region Refs
        [SerializeField] private CanvasGroup _damageOverlay;
        [SerializeField] private CanvasGroup _healingOverlay;
        [SerializeField] private CanvasGroup _lowHealthOverlay;
        private Health _health;
        #endregion

        #region State
        private float _damageStrength;
        private float _healingStrength;
        private float _normalizedHealth = 1f;
        private bool _isDead;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_damageOverlay == null || _healingOverlay == null || _lowHealthOverlay == null)
            {
                Debug.LogError("[Zombie War] ScreenFeedbackView has missing overlay references.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            _damageStrength = Mathf.MoveTowards(_damageStrength, 0f, deltaTime / _damageFadeDuration);
            _healingStrength = Mathf.MoveTowards(_healingStrength, 0f, deltaTime / _healingFadeDuration);

            _damageOverlay.alpha = _isDead
                ? _damageMaximumAlpha
                : _damageStrength * _damageStrength * _damageMaximumAlpha;
            _healingOverlay.alpha = _healingStrength * _healingStrength * _healingMaximumAlpha;
            _lowHealthOverlay.alpha = CalculateLowHealthAlpha();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
        #endregion

        #region API
        public void Initialize(Health health)
        {
            Unsubscribe();
            _health = health;
            if (_health == null)
            {
                Debug.LogError("[Zombie War] ScreenFeedbackView requires player Health.", this);
                enabled = false;
                return;
            }

            _health.Damaged += OnDamaged;
            _health.Healed += OnHealed;
            _health.Changed += OnHealthChanged;
            _health.Died += OnDied;
            _normalizedHealth = _health.Normalized;
            _isDead = _health.IsDead;
            enabled = true;
            ClearVisuals();
        }

        public void SetViewReferences(
            CanvasGroup damageOverlay,
            CanvasGroup healingOverlay,
            CanvasGroup lowHealthOverlay)
        {
            _damageOverlay = damageOverlay;
            _healingOverlay = healingOverlay;
            _lowHealthOverlay = lowHealthOverlay;
            ClearVisuals();
        }
        #endregion

        #region Internal
        private void OnDamaged(DamageInfo damage)
        {
            float normalizedDamage = Mathf.Clamp01(damage.Amount / Mathf.Max(1f, _health.MaxHealth) * 4f);
            _damageStrength = Mathf.Max(_damageStrength, Mathf.Lerp(0.45f, 1f, normalizedDamage));
            _healingStrength = 0f;
        }

        private void OnHealed(float amount)
        {
            float normalizedHealing = Mathf.Clamp01(amount / Mathf.Max(1f, _health.MaxHealth) * 5f);
            _healingStrength = Mathf.Max(_healingStrength, Mathf.Lerp(0.5f, 1f, normalizedHealing));
            _damageStrength = 0f;
        }

        private void OnHealthChanged(float normalizedHealth)
        {
            _normalizedHealth = normalizedHealth;
            if (_normalizedHealth > 0f)
            {
                _isDead = false;
            }
        }

        private void OnDied()
        {
            _isDead = true;
            _damageStrength = 1f;
        }

        private float CalculateLowHealthAlpha()
        {
            if (_isDead || _normalizedHealth >= _lowHealthThreshold)
            {
                return 0f;
            }

            float severity = 1f - _normalizedHealth / _lowHealthThreshold;
            float pulse = 0.65f + Mathf.Sin(Time.unscaledTime * _lowHealthPulseSpeed * Mathf.PI * 2f) * 0.35f;
            return severity * pulse * _lowHealthMaximumAlpha;
        }

        private void ClearVisuals()
        {
            _damageStrength = 0f;
            _healingStrength = 0f;
            if (_damageOverlay != null)
            {
                _damageOverlay.alpha = 0f;
            }
            if (_healingOverlay != null)
            {
                _healingOverlay.alpha = 0f;
            }
            if (_lowHealthOverlay != null)
            {
                _lowHealthOverlay.alpha = 0f;
            }
        }

        private void Unsubscribe()
        {
            if (_health == null)
            {
                return;
            }

            _health.Damaged -= OnDamaged;
            _health.Healed -= OnHealed;
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnDied;
            _health = null;
        }
        #endregion
    }
}
