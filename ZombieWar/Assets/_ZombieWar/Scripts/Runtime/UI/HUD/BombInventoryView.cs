using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Combat;

namespace ZombieWar.UI
{
    public sealed class BombInventoryView : MonoBehaviour
    {
        #region Refs
        [SerializeField] private Slider _stepSlider;
        [SerializeField] private Slider _glowSlider;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Image _cooldownFill;
        private BombController _bomb;
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_bomb != null)
            {
                _cooldownFill.fillAmount = _bomb.CooldownNormalized;
            }
        }

        private void OnDestroy()
        {
            if (_bomb != null)
            {
                _bomb.BombCountChanged -= Refresh;
            }
        }
        #endregion

        #region API
        public void SetViewReferences(Slider stepSlider, Slider glowSlider, TMP_Text countText, Image cooldownFill)
        {
            _stepSlider = stepSlider;
            _glowSlider = glowSlider;
            _countText = countText;
            _cooldownFill = cooldownFill;
        }

        public void Initialize(BombController bomb)
        {
            _bomb = bomb;
            _bomb.BombCountChanged += Refresh;
            Refresh(_bomb.BombCount, _bomb.MaxBombs);
        }
        #endregion

        #region Internal
        private void Refresh(int count, int maximum)
        {
            float normalized = maximum > 0 ? count / (float)maximum : 0f;
            _stepSlider.normalizedValue = normalized;
            _glowSlider.normalizedValue = normalized;
            _countText.text = $"{count}/{maximum}";
        }
        #endregion
    }
}
