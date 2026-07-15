using UnityEngine;

namespace ZombieWar.Combat
{
    public sealed class BombExplosionVfxPool : MonoBehaviour
    {
        #region Config
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(1)] private int _capacity = 4;
        #endregion

        #region State
        private GameObject[] _instances;
        private int _nextIndex;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogError("[Zombie War] BombExplosionVfxPool requires an authored VFX prefab.", this);
                enabled = false;
                return;
            }

            _instances = new GameObject[_capacity];
            for (int i = 0; i < _instances.Length; i++)
            {
                GameObject instance = Instantiate(_prefab, transform.parent);
                instance.name = $"Bomb Explosion VFX {i + 1}";
                instance.SetActive(false);
                _instances[i] = instance;
            }
        }
        #endregion

        #region API
        public void SetPrefab(GameObject prefab, int capacity)
        {
            _prefab = prefab;
            _capacity = Mathf.Max(1, capacity);
        }

        public void Play(Vector3 position)
        {
            if (_instances == null || _instances.Length == 0)
            {
                return;
            }

            GameObject instance = FindAvailableInstance();
            instance.transform.position = position;
            instance.SetActive(false);
            instance.SetActive(true);
        }
        #endregion

        #region Internal
        private GameObject FindAvailableInstance()
        {
            for (int i = 0; i < _instances.Length; i++)
            {
                int index = (_nextIndex + i) % _instances.Length;
                if (!_instances[index].activeSelf)
                {
                    _nextIndex = (index + 1) % _instances.Length;
                    return _instances[index];
                }
            }

            GameObject fallback = _instances[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _instances.Length;
            return fallback;
        }
        #endregion
    }
}
