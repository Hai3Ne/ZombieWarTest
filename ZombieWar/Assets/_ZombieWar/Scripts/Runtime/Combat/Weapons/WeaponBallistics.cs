using UnityEngine;

namespace ZombieWar.Combat
{
    public static class WeaponBallistics
    {
        public static Vector3 CalculateDirection(Quaternion muzzleRotation, float yaw, float pitch)
        {
            return (muzzleRotation * Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward).normalized;
        }
    }
}
