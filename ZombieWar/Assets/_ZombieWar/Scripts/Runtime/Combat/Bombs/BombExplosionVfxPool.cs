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
        private ParticleSystem[][] _particleSystems;
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
            _particleSystems = new ParticleSystem[_capacity][];
            for (int i = 0; i < _instances.Length; i++)
            {
                GameObject instance = Instantiate(_prefab, transform.parent);
                instance.name = $"Bomb Explosion VFX {i + 1}";
                instance.SetActive(false);
                _instances[i] = instance;
                _particleSystems[i] = instance.GetComponentsInChildren<ParticleSystem>(true);
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

            int index = FindAvailableIndex();
            GameObject instance = _instances[index];
            instance.SetActive(false);
            instance.transform.SetPositionAndRotation(position, Quaternion.identity);
            instance.SetActive(true);

            ParticleSystem[] systems = _particleSystems[index];
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem system = systems[i];
                if (!system.gameObject.activeInHierarchy)
                {
                    continue;
                }

                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(true);
            }
        }
        #endregion

        #region Internal
        private int FindAvailableIndex()
        {
            for (int i = 0; i < _instances.Length; i++)
            {
                int index = (_nextIndex + i) % _instances.Length;
                if (!_instances[index].activeSelf)
                {
                    _nextIndex = (index + 1) % _instances.Length;
                    return index;
                }
            }

            int fallbackIndex = _nextIndex;
            _nextIndex = (_nextIndex + 1) % _instances.Length;
            return fallbackIndex;
        }
        #endregion
    }
}
