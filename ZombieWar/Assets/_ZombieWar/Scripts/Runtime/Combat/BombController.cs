using System.Collections.Generic;
using UnityEngine;
using ZombieWar.Enemies;

namespace ZombieWar.Combat
{
    public sealed class BombController : MonoBehaviour
    {
        [SerializeField] private BombProjectile _bombPrefab;
        [SerializeField] private int _capacity = 4;

        private readonly Queue<BombProjectile> _available = new(4);
        private EnemyPool _enemyPool;
        private WeaponController _weaponController;
        private float _readyTime;

        public float CooldownNormalized => Mathf.Clamp01((_readyTime - Time.time) / 8f);

        private void Awake()
        {
            if (_bombPrefab == null)
            {
                Debug.LogError("[Zombie War] BombController requires a bomb prefab.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _capacity; i++)
            {
                BombProjectile bomb = Instantiate(_bombPrefab, transform.parent);
                bomb.gameObject.name = $"Bomb_{i}";
                bomb.Initialize(this);
                bomb.gameObject.SetActive(false);
                _available.Enqueue(bomb);
            }
        }

        public void SetPrefab(BombProjectile prefab, int capacity)
        {
            _bombPrefab = prefab;
            _capacity = Mathf.Max(1, capacity);
        }

        public void Configure(EnemyPool enemyPool, WeaponController weaponController)
        {
            _enemyPool = enemyPool;
            _weaponController = weaponController;
        }

        public void ThrowBomb()
        {
            if (Time.time < _readyTime || _available.Count == 0)
            {
                return;
            }
            _readyTime = Time.time + 8f;
            Vector3 direction = transform.forward;
            if (_weaponController.CurrentTarget != null)
            {
                direction = _weaponController.CurrentTarget.transform.position - transform.position;
                direction.y = 0f;
                direction.Normalize();
            }
            _available.Dequeue().Launch(transform.position + Vector3.up * 1.2f, direction * 10f + Vector3.up * 5f, 1.5f);
        }

        public void Explode(BombProjectile bomb)
        {
            Vector3 center = bomb.transform.position;
            for (int i = _enemyPool.Active.Count - 1; i >= 0; i--)
            {
                ZombieAgent zombie = _enemyPool.Active[i];
                Vector3 offset = zombie.transform.position - center;
                float distance = offset.magnitude;
                if (!zombie.IsAlive || distance > 5.5f)
                {
                    continue;
                }
                float multiplier = Core.GameplayMath.CalculateDamageFalloff(distance, 5.5f, 0.35f);
                DamageInfo damage = new(95f * multiplier, zombie.transform.position, offset.normalized * 14f, gameObject, DamageType.Explosion);
                zombie.ApplyDamage(in damage);
                zombie.ApplyExplosion(center, 5.5f, 14f);
            }
            bomb.gameObject.SetActive(false);
            _available.Enqueue(bomb);
        }
    }
}
