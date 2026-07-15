using NUnit.Framework;
using ZombieWar.Core;

namespace ZombieWar.Tests
{
    public sealed class GameOptionsTests
    {
        private bool _sfx;
        private bool _music;
        private bool _shake;

        [SetUp]
        public void SetUp()
        {
            _sfx = GameOptions.SfxEnabled;
            _music = GameOptions.MusicEnabled;
            _shake = GameOptions.CameraShakeEnabled;
        }

        [TearDown]
        public void TearDown()
        {
            GameOptions.SetSfxEnabled(_sfx);
            GameOptions.SetMusicEnabled(_music);
            GameOptions.SetCameraShakeEnabled(_shake);
        }

        [Test]
        public void Options_PersistIndependentToggleStates()
        {
            GameOptions.SetSfxEnabled(false);
            GameOptions.SetMusicEnabled(true);
            GameOptions.SetCameraShakeEnabled(false);

            Assert.That(GameOptions.SfxEnabled, Is.False);
            Assert.That(GameOptions.MusicEnabled, Is.True);
            Assert.That(GameOptions.CameraShakeEnabled, Is.False);
        }
    }
}
