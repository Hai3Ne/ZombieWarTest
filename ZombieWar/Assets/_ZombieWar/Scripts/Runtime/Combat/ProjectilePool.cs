using System.Collections.Generic;
using UnityEngine;

namespace ZombieWar.Combat
{
    public sealed class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile _prefab;
        [SerializeField] private int _capacity = 96;

        private readonly Queue<Projectile> _available = new(96);

        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogError("[Zombie War] ProjectilePool requires a projectile prefab.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _capacity; i++)
            {
                Projectile projectile = Instantiate(_prefab, transform);
                projectile.gameObject.name = $"Projectile_{i:000}";
                projectile.Initialize(this);
                projectile.gameObject.SetActive(false);
                _available.Enqueue(projectile);
            }
        }

        public void SetPrefab(Projectile prefab, int capacity)
        {
            _prefab = prefab;
            _capacity = Mathf.Max(1, capacity);
        }

        public void Fire(Vector3 origin, Vector3 direction, float speed, float range, float damage, GameObject instigator, Color color)
        {
            if (_available.Count > 0)
            {
                _available.Dequeue().Launch(origin, direction, speed, range, damage, instigator, color);
            }
        }

        public void Release(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
            _available.Enqueue(projectile);
        }
    }
}
