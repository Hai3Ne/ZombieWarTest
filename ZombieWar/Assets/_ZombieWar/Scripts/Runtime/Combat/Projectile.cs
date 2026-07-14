using UnityEngine;

namespace ZombieWar.Combat
{
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class Projectile : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private ProjectilePool _pool;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private Vector3 _direction;
        private float _speed;
        private float _remainingRange;
        private float _damage;
        private GameObject _instigator;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            if (!TryGetComponent(out _renderer))
            {
                Debug.LogError("[Zombie War] Projectile requires a MeshRenderer.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            float distance = Mathf.Min(_remainingRange, _speed * Time.deltaTime);
            int count = Physics.SphereCastNonAlloc(transform.position, 0.12f, _direction, _hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider hit = _hits[i].collider;
                if (hit == null || hit.gameObject == _instigator)
                {
                    continue;
                }
                if (hit.TryGetComponent(out Health health))
                {
                    DamageInfo damage = new(_damage, _hits[i].point, _direction * 2f, _instigator, DamageType.Bullet);
                    health.ApplyDamage(in damage);
                    _pool.Release(this);
                    return;
                }
                if (!hit.CompareTag("Player"))
                {
                    _pool.Release(this);
                    return;
                }
            }

            transform.position += _direction * distance;
            _remainingRange -= distance;
            if (_remainingRange <= 0f)
            {
                _pool.Release(this);
            }
        }

        public void Initialize(ProjectilePool pool) => _pool = pool;

        public void Launch(Vector3 origin, Vector3 direction, float speed, float range, float damage, GameObject instigator, Color color)
        {
            transform.position = origin;
            _direction = direction.normalized;
            _speed = speed;
            _remainingRange = range;
            _damage = damage;
            _instigator = instigator;
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_propertyBlock);
            gameObject.SetActive(true);
        }
    }
}
