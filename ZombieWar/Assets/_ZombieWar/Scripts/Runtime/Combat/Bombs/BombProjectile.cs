using UnityEngine;

namespace ZombieWar.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BombProjectile : MonoBehaviour
    {
        private BombController _owner;
        private Rigidbody _rigidbody;
        private float _explodeAt;

        private void Awake()
        {
            if (!TryGetComponent(out _rigidbody))
            {
                Debug.LogError("[Zombie War] BombProjectile requires a Rigidbody.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (Time.time >= _explodeAt)
            {
                _owner.Explode(this);
            }
        }

        public void Initialize(BombController owner) => _owner = owner;

        public void Launch(Vector3 position, Vector3 velocity, float fuseSeconds)
        {
            transform.position = position;
            gameObject.SetActive(true);
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = new Vector3(3f, 6f, 2f);
            _explodeAt = Time.time + fuseSeconds;
        }
    }
}
