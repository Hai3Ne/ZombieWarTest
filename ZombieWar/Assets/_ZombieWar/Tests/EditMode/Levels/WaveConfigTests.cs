using NUnit.Framework;
using UnityEngine;
using ZombieWar.Enemies;
using ZombieWar.Levels;

namespace ZombieWar.Tests
{
    public sealed class WaveConfigTests
    {
        [Test]
        public void GetWaveAtTime_AdvancesAtConfiguredBoundaries()
        {
            WaveConfig waveOne = ScriptableObject.CreateInstance<WaveConfig>();
            WaveConfig waveTwo = ScriptableObject.CreateInstance<WaveConfig>();
            WaveSequenceConfig sequence = ScriptableObject.CreateInstance<WaveSequenceConfig>();
            try
            {
                waveOne.Configure("WAVE 1", 60f, 10, 20, 2, null);
                waveTwo.Configure("WAVE 2", 60f, 20, 40, 3, null);
                sequence.Configure("TEST", 100, null, new[] { waveOne, waveTwo });

                WaveConfig result = sequence.GetWaveAtTime(60f, out int index, out float normalizedTime);

                Assert.That(result, Is.SameAs(waveTwo));
                Assert.That(index, Is.EqualTo(1));
                Assert.That(normalizedTime, Is.EqualTo(0f).Within(0.001f));
                Assert.That(sequence.TotalDuration, Is.EqualTo(120f));
            }
            finally
            {
                Object.DestroyImmediate(waveOne);
                Object.DestroyImmediate(waveTwo);
                Object.DestroyImmediate(sequence);
            }
        }

        [Test]
        public void SelectEnemy_UsesAuthoredWeights()
        {
            EnemyConfig walker = ScriptableObject.CreateInstance<EnemyConfig>();
            EnemyConfig runner = ScriptableObject.CreateInstance<EnemyConfig>();
            WaveConfig wave = ScriptableObject.CreateInstance<WaveConfig>();
            try
            {
                walker.Configure("WALKER", EnemyArchetype.Walker, 50f, 2f, 5f, 1f, 1f, 1f, Color.white);
                runner.Configure("RUNNER", EnemyArchetype.Runner, 30f, 4f, 4f, 1f, 1f, 0.9f, Color.green);
                WaveEnemyEntry walkerEntry = new();
                walkerEntry.Configure(walker, 0.75f);
                WaveEnemyEntry runnerEntry = new();
                runnerEntry.Configure(runner, 0.25f);
                wave.Configure("WAVE", 60f, 10, 30, 2, new[] { walkerEntry, runnerEntry });

                Assert.That(wave.SelectEnemy(0.5f), Is.SameAs(walker));
                Assert.That(wave.SelectEnemy(0.9f), Is.SameAs(runner));
                Assert.That(wave.EvaluateTargetCount(0.5f), Is.EqualTo(20));
            }
            finally
            {
                Object.DestroyImmediate(walker);
                Object.DestroyImmediate(runner);
                Object.DestroyImmediate(wave);
            }
        }

        [Test]
        public void SpecialEncounters_KeepEliteAndBossReferences()
        {
            EnemyConfig elite = ScriptableObject.CreateInstance<EnemyConfig>();
            EnemyConfig boss = ScriptableObject.CreateInstance<EnemyConfig>();
            WaveConfig wave = ScriptableObject.CreateInstance<WaveConfig>();
            WaveSequenceConfig sequence = ScriptableObject.CreateInstance<WaveSequenceConfig>();
            try
            {
                wave.Configure("WAVE", 60f, 10, 20, 2, null, elite);
                sequence.Configure("LEVEL", 120, null, new[] { wave }, boss);

                Assert.That(wave.EliteEnemy, Is.SameAs(elite));
                Assert.That(sequence.BossEnemy, Is.SameAs(boss));
                Assert.That(sequence.WaveCount, Is.EqualTo(1));
                Assert.That(sequence.GetWave(0), Is.SameAs(wave));
            }
            finally
            {
                Object.DestroyImmediate(elite);
                Object.DestroyImmediate(boss);
                Object.DestroyImmediate(wave);
                Object.DestroyImmediate(sequence);
            }
        }

        [Test]
        public void LevelCatalog_UsesEnabledOrderedListForProgression()
        {
            WaveSequenceConfig sequence = ScriptableObject.CreateInstance<WaveSequenceConfig>();
            LevelCatalogConfig catalog = ScriptableObject.CreateInstance<LevelCatalogConfig>();
            try
            {
                LevelDefinition first = new();
                first.Configure("FIRST", "ArenaA", sequence, true);
                LevelDefinition disabled = new();
                disabled.Configure("DISABLED", "ArenaB", sequence, false);
                LevelDefinition third = new();
                third.Configure("THIRD", "ArenaC", sequence, true);
                catalog.Configure(new[] { first, disabled, third });

                Assert.That(catalog.EnabledLevelCount, Is.EqualTo(2));
                Assert.That(catalog.GetLevelBySceneName("ArenaA"), Is.SameAs(first));
                Assert.That(catalog.GetLevelBySceneName("ArenaB"), Is.Null);
                Assert.That(catalog.GetNextSceneName("ArenaA"), Is.EqualTo("ArenaC"));
                Assert.That(catalog.GetNextSceneName("ArenaC"), Is.EqualTo("ArenaA"));
            }
            finally
            {
                Object.DestroyImmediate(sequence);
                Object.DestroyImmediate(catalog);
            }
        }
    }
}
