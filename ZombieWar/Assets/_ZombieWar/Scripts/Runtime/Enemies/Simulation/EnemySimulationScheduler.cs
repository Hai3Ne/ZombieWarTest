using UnityEngine;

namespace ZombieWar.Enemies
{
    public sealed class EnemySimulationScheduler : MonoBehaviour
    {
        #region Refs
        private EnemyPool _pool;
        private Transform _target;
        #endregion

        #region API
        public void Configure(EnemyPool pool, Transform target)
        {
            _pool = pool;
            _target = target;
        }
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (_pool == null || _target == null)
            {
                return;
            }

            int frame = Time.frameCount;
            for (int i = _pool.Active.Count - 1; i >= 0; i--)
            {
                ZombieAgent zombie = _pool.Active[i];
                if (!zombie.isActiveAndEnabled)
                {
                    continue;
                }

                if (!zombie.IsAlive)
                {
                    zombie.Simulate(Time.deltaTime, false);
                    continue;
                }

                float distanceSquared = (zombie.transform.position - _target.position).sqrMagnitude;
                int interval = distanceSquared < 144f ? 6 : distanceSquared < 625f ? 15 : 30;
                if ((frame + zombie.SimulationSlot) % interval != 0)
                {
                    continue;
                }

                zombie.Simulate(Time.deltaTime * interval, interval == 6);
            }
        }
        #endregion
    }
}
