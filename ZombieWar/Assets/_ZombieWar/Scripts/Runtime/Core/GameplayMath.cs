using UnityEngine;

namespace ZombieWar.Core
{
    public static class GameplayMath
    {
        public static float CalculateDamageFalloff(float distance, float radius, float minimumMultiplier)
        {
            if (radius <= 0f)
            {
                return 0f;
            }

            float normalized = 1f - Mathf.Clamp01(distance / radius);
            return Mathf.Lerp(Mathf.Clamp01(minimumMultiplier), 1f, normalized);
        }

        public static float EvaluateWaveIntensity(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (normalized < 0.25f)
            {
                return Mathf.Lerp(0f, 0.28f, normalized / 0.25f);
            }
            if (normalized < 0.6667f)
            {
                return Mathf.Lerp(0.28f, 0.68f, (normalized - 0.25f) / 0.4167f);
            }
            if (normalized < 0.9167f)
            {
                return Mathf.Lerp(0.68f, 0.9f, (normalized - 0.6667f) / 0.25f);
            }
            return Mathf.Lerp(0.9f, 1f, (normalized - 0.9167f) / 0.0833f);
        }

        public static bool IsCooldownReady(float currentTime, float readyTime)
        {
            return currentTime >= readyTime;
        }

        public static bool ShouldSwitchTarget(
            float currentDistanceSquared,
            float candidateDistanceSquared,
            float switchThreshold)
        {
            return candidateDistanceSquared < currentDistanceSquared * Mathf.Clamp01(switchThreshold);
        }
    }
}
