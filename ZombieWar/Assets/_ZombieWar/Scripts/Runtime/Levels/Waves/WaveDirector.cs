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
        private float _waveElapsed;
        private int _currentWaveIndex = -1;
        private ZombieAgent _specialEnemy;
        private WavePhase _phase = WavePhase.Completed;
        public float Elapsed => _elapsed;
        public float Remaining => _sequence != null ? Mathf.Max(0f, _sequence.TotalDuration - _elapsed) : 0f;
        public int CurrentWaveNumber => _currentWaveIndex + 1;
        public string CurrentWaveName { get; private set; } = string.Empty;
        public WavePhase Phase => _phase;
        public bool IsCompleted => _phase == WavePhase.Completed;
        public string EncounterLabel => _phase switch
        {
            WavePhase.Elite => $"WAVE {CurrentWaveNumber} ELITE",
            WavePhase.Boss => "LEVEL BOSS",
            WavePhase.Completed => "AREA SECURED",
            _ => $"WAVE {CurrentWaveNumber}"
        };
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

            switch (_phase)
            {
                case WavePhase.Wave:
                    UpdateWave();
                    break;
                case WavePhase.Elite:
                    UpdateElite();
                    break;
                case WavePhase.Boss:
                    UpdateBoss();
                    break;
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
            _elapsed = 0f;
            _waveElapsed = 0f;
            _currentWaveIndex = 0;
            _specialEnemy = null;
            _phase = _sequence != null && _sequence.GetWave(0) != null
                ? WavePhase.Wave
                : WavePhase.Completed;
            WaveConfig firstWave = _sequence?.GetWave(0);
            CurrentWaveName = firstWave != null ? firstWave.DisplayName : string.Empty;
            WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
        }
        #endregion

        #region Internal
        private void UpdateWave()
        {
            WaveConfig wave = _sequence.GetWave(_currentWaveIndex);
            if (wave == null)
            {
                AdvanceAfterElite();
                return;
            }

            _waveElapsed = Mathf.Min(wave.DurationSeconds, _waveElapsed + Time.deltaTime);
            _elapsed = Mathf.Min(_sequence.TotalDuration, _elapsed + Time.deltaTime);
            if (_waveElapsed >= wave.DurationSeconds)
            {
                if (wave.EliteEnemy == null)
                {
                    AdvanceAfterElite();
                    return;
                }

                TrySpawnSpecial(wave.EliteEnemy, WavePhase.Elite);
                return;
            }

            float normalizedWaveTime = _waveElapsed / wave.DurationSeconds;
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

        private void UpdateElite()
        {
            if (_specialEnemy != null && _specialEnemy.IsAlive)
            {
                return;
            }

            _specialEnemy = null;
            AdvanceAfterElite();
        }

        private void AdvanceAfterElite()
        {
            if (_currentWaveIndex >= _sequence.WaveCount - 1)
            {
                BeginBoss();
                return;
            }

            _currentWaveIndex++;
            _waveElapsed = 0f;
            _phase = WavePhase.Wave;
            WaveConfig nextWave = _sequence.GetWave(_currentWaveIndex);
            CurrentWaveName = nextWave != null ? nextWave.DisplayName : string.Empty;
            WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
        }

        private void BeginBoss()
        {
            if (_sequence.BossEnemy == null)
            {
                _phase = WavePhase.Completed;
                CurrentWaveName = "AREA SECURED";
                return;
            }

            _phase = WavePhase.Boss;
            CurrentWaveName = _sequence.BossEnemy.DisplayName;
            WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
            TrySpawnSpecial(_sequence.BossEnemy, WavePhase.Boss);
        }

        private void UpdateBoss()
        {
            if (_specialEnemy == null)
            {
                TrySpawnSpecial(_sequence.BossEnemy, WavePhase.Boss);
                return;
            }

            if (_specialEnemy.State != ZombieState.Inactive)
            {
                return;
            }

            _specialEnemy = null;
            _phase = WavePhase.Completed;
            CurrentWaveName = "AREA SECURED";
            WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
        }

        private void TrySpawnSpecial(EnemyConfig config, WavePhase phase)
        {
            ZombieAgent spawned = _pool.Spawn(GetSpawnPosition(), config, _target, _targetHealth);
            if (spawned == null)
            {
                return;
            }

            _specialEnemy = spawned;
            _phase = phase;
            CurrentWaveName = config.DisplayName;
            WaveChanged?.Invoke(CurrentWaveNumber, CurrentWaveName);
        }

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
