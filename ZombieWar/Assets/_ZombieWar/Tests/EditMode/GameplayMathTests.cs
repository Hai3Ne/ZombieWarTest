using NUnit.Framework;
using ZombieWar.Core;

namespace ZombieWar.Tests
{
    public sealed class GameplayMathTests
    {
        [Test]
        public void DamageFalloff_IsFullAtCenter()
        {
            Assert.That(GameplayMath.CalculateDamageFalloff(0f, 5f, 0.35f), Is.EqualTo(1f));
        }

        [Test]
        public void DamageFalloff_UsesMinimumAtRadius()
        {
            Assert.That(GameplayMath.CalculateDamageFalloff(5f, 5f, 0.35f), Is.EqualTo(0.35f).Within(0.001f));
        }

        [Test]
        public void WaveIntensity_IsMonotonicAcrossPhases()
        {
            float previous = 0f;
            for (int i = 0; i <= 100; i++)
            {
                float current = GameplayMath.EvaluateWaveIntensity(i / 100f);
                Assert.That(current, Is.GreaterThanOrEqualTo(previous));
                previous = current;
            }
        }

        [Test]
        public void WaveIntensity_ReachesPeak()
        {
            Assert.That(GameplayMath.EvaluateWaveIntensity(1f), Is.EqualTo(1f).Within(0.001f));
        }

        [TestCase(2f, 2f, true)]
        [TestCase(1.99f, 2f, false)]
        public void Cooldown_OnlyAllowsFireAtReadyTime(float current, float ready, bool expected)
        {
            Assert.That(GameplayMath.IsCooldownReady(current, ready), Is.EqualTo(expected));
        }

        [Test]
        public void TargetSelection_RequiresMeaningfullyCloserCandidate()
        {
            Assert.That(GameplayMath.ShouldSwitchTarget(100f, 79f, 0.8f), Is.True);
            Assert.That(GameplayMath.ShouldSwitchTarget(100f, 81f, 0.8f), Is.False);
        }
    }
}
