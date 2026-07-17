using UnityEditor;
using UnityEngine;
using ZombieWar.Enemies;
using ZombieWar.Levels;

namespace ZombieWar.Editor
{
    public static class EliteBossWaveInstaller
    {
        private const string ArchetypeFolder = "Assets/_ZombieWar/Configs/Enemies/Archetypes";
        private const string ConfigRoot = "Assets/_ZombieWar/Configs";

        [MenuItem("Zombie War/Gameplay/Install Elite And Boss Waves")]
        public static void Install()
        {
            EnemyConfig elite = GetOrCreateEnemy(
                "Elite",
                "ELITE",
                EnemyArchetype.Brute,
                360f,
                2.15f,
                20f,
                1.7f,
                0.95f,
                1.55f,
                new Color(0.68f, 0.24f, 0.92f));
            EnemyConfig boss = GetOrCreateEnemy(
                "Boss",
                "LEVEL BOSS",
                EnemyArchetype.Giant,
                1800f,
                1.45f,
                34f,
                2.65f,
                1.45f,
                2.75f,
                new Color(0.82f, 0.1f, 0.08f));

            int waveCount = AssignEliteToWaves(elite);
            int levelCount = AssignBossToSequences(boss);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Zombie War] Elite assigned to {waveCount} waves; Boss assigned to {levelCount} level sequences.");
        }

        private static EnemyConfig GetOrCreateEnemy(
            string assetName,
            string displayName,
            EnemyArchetype archetype,
            float health,
            float speed,
            float damage,
            float range,
            float interval,
            float scale,
            Color tint)
        {
            string path = $"{ArchetypeFolder}/{assetName}.asset";
            EnemyConfig config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EnemyConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.Configure(displayName, archetype, health, speed, damage, range, interval, scale, tint);
            EditorUtility.SetDirty(config);
            return config;
        }

        private static int AssignEliteToWaves(EnemyConfig elite)
        {
            string[] guids = AssetDatabase.FindAssets("t:WaveConfig", new[] { ConfigRoot });
            int assigned = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                WaveConfig wave = AssetDatabase.LoadAssetAtPath<WaveConfig>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (wave == null)
                {
                    continue;
                }

                SerializedObject serializedWave = new(wave);
                serializedWave.Update();
                SerializedProperty eliteProperty = serializedWave.FindProperty("_eliteEnemy");
                if (eliteProperty.objectReferenceValue == null)
                {
                    eliteProperty.objectReferenceValue = elite;
                    serializedWave.ApplyModifiedProperties();
                    EditorUtility.SetDirty(wave);
                }
                assigned++;
            }

            return assigned;
        }

        private static int AssignBossToSequences(EnemyConfig boss)
        {
            string[] guids = AssetDatabase.FindAssets("t:WaveSequenceConfig", new[] { ConfigRoot });
            int assigned = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                WaveSequenceConfig sequence = AssetDatabase.LoadAssetAtPath<WaveSequenceConfig>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (sequence == null)
                {
                    continue;
                }

                SerializedObject serializedSequence = new(sequence);
                serializedSequence.Update();
                SerializedProperty bossProperty = serializedSequence.FindProperty("_bossEnemy");
                if (bossProperty.objectReferenceValue == null)
                {
                    bossProperty.objectReferenceValue = boss;
                    serializedSequence.ApplyModifiedProperties();
                    EditorUtility.SetDirty(sequence);
                }
                assigned++;
            }

            return assigned;
        }
    }
}
