using UnityEngine;

namespace ZombieWar.Combat
{
    public sealed class ProjectileImpactVfxPool : MonoBehaviour
    {
        #region Config
        [SerializeField] private GameObject _bloodPrefab;
        [SerializeField] private GameObject _hardSurfacePrefab;
        [SerializeField, Min(1)] private int _bloodCapacity = 24;
        [SerializeField, Min(1)] private int _hardSurfaceCapacity = 12;
        #endregion

        #region State
        private GameObject[] _bloodPool;
        private GameObject[] _hardSurfacePool;
        private int _bloodIndex;
        private int _hardSurfaceIndex;
        #endregion

        #region Lifecycle
        private void Awake()
        {
            _bloodPool = Prewarm(_bloodPrefab, _bloodCapacity, "BloodImpact");
            _hardSurfacePool = Prewarm(_hardSurfacePrefab, _hardSurfaceCapacity, "HardSurfaceImpact");
        }
        #endregion

        #region API
        public void SetPrefabs(GameObject bloodPrefab, GameObject hardSurfacePrefab, int bloodCapacity, int hardSurfaceCapacity)
        {
            _bloodPrefab = bloodPrefab;
            _hardSurfacePrefab = hardSurfacePrefab;
            _bloodCapacity = Mathf.Max(1, bloodCapacity);
            _hardSurfaceCapacity = Mathf.Max(1, hardSurfaceCapacity);
        }

        public void PlayBlood(Vector3 position, Vector3 normal)
        {
            Play(_bloodPool, ref _bloodIndex, position, normal);
        }

        public void PlayHardSurface(Vector3 position, Vector3 normal)
        {
            Play(_hardSurfacePool, ref _hardSurfaceIndex, position, normal);
        }
        #endregion

        #region Internal
        private GameObject[] Prewarm(GameObject prefab, int capacity, string objectName)
        {
            if (prefab == null)
            {
                Debug.LogError($"[Zombie War] ProjectileImpactVfxPool requires {objectName} prefab.", this);
                return System.Array.Empty<GameObject>();
            }

            GameObject[] pool = new GameObject[capacity];
            for (int i = 0; i < capacity; i++)
            {
                pool[i] = Instantiate(prefab, transform);
                pool[i].name = $"{objectName}_{i:00}";
                pool[i].SetActive(false);
            }
            return pool;
        }

        private static void Play(GameObject[] pool, ref int index, Vector3 position, Vector3 normal)
        {
            if (pool == null || pool.Length == 0)
            {
                return;
            }

            GameObject effect = pool[index];
            index = (index + 1) % pool.Length;
            effect.SetActive(false);
            effect.transform.SetPositionAndRotation(position + normal * 0.015f, Quaternion.LookRotation(normal));
            effect.SetActive(true);
        }
        #endregion
    }
}
