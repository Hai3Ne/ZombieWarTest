using System;
using UnityEngine;
using UnityEngine.AI;
using ZombieWar.Combat;
using ZombieWar.Core;
using ZombieWar.Enemies;
using Random = UnityEngine.Random;

namespace ZombieWar.Levels
{
    public sealed class WaveDirector : MonoBehaviour
    {
        #region Refs
        private EnemyPool _pool;
        private WaveSequenceConfig _sequence;
        private Transform _target;
        private Health _targetHealth;
        private Camera _camera;
        #endregion

        #region State
        private float _elapsed;
        private int _currentWaveIndex = -1;
        public float Elapsed => _elapsed;
        public float Remaining => _sequence != null ? Mathf.Max(0f, _sequence.TotalDuration - _elapsed) : 0f;
        public int CurrentWaveNumber => _currentWaveIndex + 1;
        public string CurrentWaveName { get; private set; } = string.Empty;
        public event Action<int, string> WaveChanged;
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_sequence == null
                || _pool == null
                || !_pool.IsReady
                || _targetHealth == null
                || _targetHealth.IsDead)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= _sequence.TotalDuration)
            {
                return;
            }

            WaveConfig wave = _sequence.GetWaveAtTime(_elapsed, out int waveIndex, out float normalizedWaveTime);
            if (wave == null)
            {
                return;
            }
            if (waveIndex != _currentWaveIndex)
            {
                _currentWaveIndex = waveIndex;
                CurrentWaveName = wave.DisplayName;
                WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
            }

            int targetCount = Mathf.Min(wave.EvaluateTargetCount(normalizedWaveTime), _sequence.HardCap);

            int spawnBudget = Mathf.Min(wave.SpawnPerFrame, targetCount - _pool.ActiveCount);
            for (int i = 0; i < spawnBudget; i++)
            {
                EnemyConfig enemy = wave.SelectEnemy(Random.value, _pool);
                if (enemy != null)
                {
                    _pool.Spawn(GetSpawnPosition(), enemy, _target, _targetHealth);
                }
            }
        }
        #endregion

        #region API
        public void Configure(
            EnemyPool pool,
            WaveSequenceConfig sequence,
            Transform target,
            Health targetHealth,
            Camera worldCamera)
        {
            _pool = pool;
            _sequence = sequence;
            _target = target;
            _targetHealth = targetHealth;
            _camera = worldCamera;
            WaveConfig firstWave = _sequence.GetWaveAtTime(0f, out _currentWaveIndex, out _);
            CurrentWaveName = firstWave != null ? firstWave.DisplayName : string.Empty;
            WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
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
