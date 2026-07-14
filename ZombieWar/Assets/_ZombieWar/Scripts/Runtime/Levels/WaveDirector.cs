using UnityEngine;
using UnityEngine.AI;
using ZombieWar.Combat;
using ZombieWar.Core;
using ZombieWar.Enemies;

namespace ZombieWar.Levels
{
    public sealed class WaveDirector : MonoBehaviour
    {
        #region Refs
        private EnemyPool _pool;
        private EnemyConfig _regularConfig;
        private EnemyConfig _giantConfig;
        private LevelConfig _levelConfig;
        private Transform _target;
        private Health _targetHealth;
        private Camera _camera;
        #endregion

        #region State
        private float _elapsed;
        private bool _giantSpawned;
        public float Elapsed => _elapsed;
        public float Remaining => Mathf.Max(0f, _levelConfig.DurationSeconds - _elapsed);
        public float NormalizedTime => Mathf.Clamp01(_elapsed / _levelConfig.DurationSeconds);
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_levelConfig == null
                || _pool == null
                || !_pool.IsReady
                || _targetHealth == null
                || _targetHealth.IsDead)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            int targetCount = Mathf.RoundToInt(Mathf.Lerp(
                _levelConfig.StartCount,
                _levelConfig.PeakCount,
                GameplayMath.EvaluateWaveIntensity(NormalizedTime)));
            targetCount = Mathf.Min(targetCount, _levelConfig.HardCap);

            int spawnBudget = Mathf.Min(3, targetCount - _pool.ActiveCount);
            for (int i = 0; i < spawnBudget; i++)
            {
                _pool.Spawn(GetSpawnPosition(), _regularConfig, _target, _targetHealth);
            }

            if (_levelConfig.SpawnGiant
                && !_giantSpawned
                && _elapsed >= _levelConfig.GiantSpawnTime)
            {
                _giantSpawned = true;
                _pool.Spawn(GetSpawnPosition(), _giantConfig, _target, _targetHealth);
            }
        }
        #endregion

        #region API
        public void Configure(
            EnemyPool pool,
            EnemyConfig regularConfig,
            EnemyConfig giantConfig,
            LevelConfig levelConfig,
            Transform target,
            Health targetHealth,
            Camera worldCamera)
        {
            _pool = pool;
            _regularConfig = regularConfig;
            _giantConfig = giantConfig;
            _levelConfig = levelConfig;
            _target = target;
            _targetHealth = targetHealth;
            _camera = worldCamera;
        }
        #endregion

        #region Internal
        private Vector3 GetSpawnPosition()
        {
            float edge = Random.value < 0.5f ? -0.08f : 1.08f;
            Vector2 viewport = Random.value < 0.5f
                ? new Vector2(edge, Random.Range(-0.08f, 1.08f))
                : new Vector2(Random.Range(-0.08f, 1.08f), edge);

            Ray ray = _camera.ViewportPointToRay(viewport);
            Plane ground = new(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float enter))
            {
                Vector3 point = ray.GetPoint(enter);
                point += (point - _target.position).normalized * 3f;
                if (NavMesh.SamplePosition(point, out NavMeshHit edgeHit, 12f, NavMesh.AllAreas))
                {
                    return edgeHit.position;
                }
            }

            Vector2 circle = Random.insideUnitCircle.normalized * 14f;
            Vector3 fallback = _target.position + new Vector3(circle.x, 0f, circle.y);
            return NavMesh.SamplePosition(fallback, out NavMeshHit fallbackHit, 6f, NavMesh.AllAreas)
                ? fallbackHit.position
                : _target.position;
        }

        #endregion
    }
}
