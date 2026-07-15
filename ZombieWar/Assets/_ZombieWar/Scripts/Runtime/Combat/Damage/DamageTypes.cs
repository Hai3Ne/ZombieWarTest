using UnityEngine;

namespace ZombieWar.Combat
{
    public enum DamageType
    {
        Bullet,
        Explosion,
        Melee
    }

    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, Vector3 point, Vector3 impulse, GameObject instigator, DamageType type)
        {
            Amount = amount;
            Point = point;
            Impulse = impulse;
            Instigator = instigator;
            Type = type;
        }

        public float Amount { get; }
        public Vector3 Point { get; }
        public Vector3 Impulse { get; }
        public GameObject Instigator { get; }
        public DamageType Type { get; }
    }

    public interface IDamageable
    {
        void ApplyDamage(in DamageInfo damage);
    }
}
