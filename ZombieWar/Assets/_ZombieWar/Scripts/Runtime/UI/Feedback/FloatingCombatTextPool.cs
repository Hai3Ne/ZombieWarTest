using System.Collections.Generic;
using UnityEngine;

namespace ZombieWar.UI
{
    public sealed class FloatingCombatTextPool : MonoBehaviour
    {
        #region Config
        [SerializeField] private Color _playerDamageColor = new(1f, 0.16f, 0.12f);
        [SerializeField] private Color _enemyDamageColor = new(1f, 0.68f, 0.12f);
        [SerializeField] private Color _healingColor = new(0.2f, 1f, 0.48f);
        #endregion

        #region Refs
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private FloatingCombatText[] _entries;
        #endregion

        #region State
        private readonly Queue<FloatingCombatText> _available = new(96);
        public int AvailableCount => _available.Count;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_worldCamera == null || _entries == null || _entries.Length == 0)
            {
                Debug.LogError("[Zombie War] FloatingCombatTextPool requires authored camera and entries.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                _entries[i].Initialize(this);
                _available.Enqueue(_entries[i]);
            }
        }
        #endregion

        #region API
        public void ShowDamage(float amount, Vector3 position, bool isPlayer)
        {
            if (amount <= 0f || !TryTake(out FloatingCombatText text))
            {
                return;
            }
            text.ShowDamage(amount, position, isPlayer ? _playerDamageColor : _enemyDamageColor, _worldCamera);
        }

        public void ShowHealing(float amount, Vector3 position)
        {
            if (amount <= 0f || !TryTake(out FloatingCombatText text))
            {
                return;
            }
            text.ShowHealing(amount, position, _healingColor, _worldCamera);
        }

        public void Release(FloatingCombatText text)
        {
            text.gameObject.SetActive(false);
            _available.Enqueue(text);
        }

        public void SetReferences(Camera worldCamera, FloatingCombatText[] entries)
        {
            _worldCamera = worldCamera;
            _entries = entries;
        }
        #endregion

        #region Internal
        private bool TryTake(out FloatingCombatText text)
        {
            if (!enabled || _available.Count == 0)
            {
                text = null;
                return false;
            }
            text = _available.Dequeue();
            return true;
        }
        #endregion
    }
}
