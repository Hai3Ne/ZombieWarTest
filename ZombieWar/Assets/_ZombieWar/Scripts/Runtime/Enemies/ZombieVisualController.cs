using UnityEngine;

namespace ZombieWar.Enemies
{
    public sealed class ZombieVisualController : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int HitFlashId = Shader.PropertyToID("_HitFlash");
        private static readonly int DissolveId = Shader.PropertyToID("_DissolveAmount");

        #region Refs
        [SerializeField] private Renderer[] _renderers;
        #endregion

        #region State
        private MaterialPropertyBlock _propertyBlock;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            if (_renderers == null || _renderers.Length == 0)
            {
                Debug.LogError("[Zombie War] Zombie visual renderers were not authored.", this);
                enabled = false;
            }
        }
        #endregion

        #region API
        public void SetRenderers(Renderer[] renderers)
        {
            _renderers = renderers;
        }

        public void SetState(bool isGiant, float hitFlash, float dissolve)
        {
            Color tint = isGiant
                ? new Color(0.72f, 0.32f, 0.27f, 1f)
                : Color.white;

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColorId, tint);
                _propertyBlock.SetFloat(HitFlashId, hitFlash);
                _propertyBlock.SetFloat(DissolveId, Mathf.Clamp01(dissolve));
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
        #endregion
    }
}
