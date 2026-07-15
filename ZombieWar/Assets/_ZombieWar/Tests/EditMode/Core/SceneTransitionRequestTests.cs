using NUnit.Framework;
using ZombieWar.Core;

namespace ZombieWar.Tests
{
    public sealed class SceneTransitionRequestTests
    {
        [SetUp]
        public void SetUp()
        {
            SceneTransitionRequest.ConsumeTarget(string.Empty);
        }

        [Test]
        public void ConsumeTarget_ReturnsRequestedSceneOnce()
        {
            SceneTransitionRequest.SetTarget("Level02");

            Assert.That(SceneTransitionRequest.ConsumeTarget("MainMenu"), Is.EqualTo("Level02"));
            Assert.That(SceneTransitionRequest.ConsumeTarget("MainMenu"), Is.EqualTo("MainMenu"));
        }

        [Test]
        public void ConsumeTarget_UsesFallbackWhenRequestIsEmpty()
        {
            SceneTransitionRequest.SetTarget(string.Empty);

            Assert.That(SceneTransitionRequest.ConsumeTarget("MainMenu"), Is.EqualTo("MainMenu"));
        }
    }
}
