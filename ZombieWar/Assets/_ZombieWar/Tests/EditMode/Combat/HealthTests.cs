using NUnit.Framework;
using UnityEngine;
using ZombieWar.Combat;

namespace ZombieWar.Tests
{
    public sealed class HealthTests
    {
        [Test]
        public void Heal_RestoresOnlyMissingHealth_AndRaisesActualAmount()
        {
            GameObject owner = new("Health Test", typeof(Health));
            try
            {
                Health health = owner.GetComponent<Health>();
                health.Configure(100f);
                DamageInfo damage = new(35f, Vector3.zero, Vector3.zero, owner, DamageType.Melee);
                health.ApplyDamage(in damage);
                float healedEventAmount = 0f;
                health.Healed += amount => healedEventAmount = amount;

                float restored = health.Heal(50f);

                Assert.That(restored, Is.EqualTo(35f));
                Assert.That(healedEventAmount, Is.EqualTo(35f));
                Assert.That(health.CurrentHealth, Is.EqualTo(100f));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void Heal_DoesNotReviveDeadHealth()
        {
            GameObject owner = new("Health Test", typeof(Health));
            try
            {
                Health health = owner.GetComponent<Health>();
                health.Configure(100f);
                DamageInfo damage = new(100f, Vector3.zero, Vector3.zero, owner, DamageType.Melee);
                health.ApplyDamage(in damage);

                Assert.That(health.Heal(25f), Is.EqualTo(0f));
                Assert.That(health.IsDead, Is.True);
                Assert.That(health.CurrentHealth, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }
    }
}
