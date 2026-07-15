using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZombieWar.Editor
{
    public static class ConfigStructureOrganizer
    {
        private const string ConfigRoot = "Assets/_ZombieWar/Configs";

        private static readonly string[] DuplicateRoots =
        {
            ConfigRoot + "/Levels",
            ConfigRoot + "/Weapons",
            ConfigRoot + "/Zombies"
        };

        private static readonly KeyValuePair<string, string>[] AssetMoves =
        {
            Move("WeaponAudioCatalog.asset", "Audio/Weapons/WeaponAudioCatalog.asset"),
            Move("ZombieAudioCatalog.asset", "Audio/Zombies/ZombieAudioCatalog.asset"),
            Move("EnemyPrefabCatalog.asset", "Enemies/EnemyPrefabCatalog.asset"),
            Move("ZombieAnimator.controller", "Enemies/Animation/ZombieAnimator.controller"),
            Move("Zombie.asset", "Enemies/Archetypes/Walker.asset"),
            Move("RunnerZombie.asset", "Enemies/Archetypes/Runner.asset"),
            Move("BruteZombie.asset", "Enemies/Archetypes/Brute.asset"),
            Move("GiantZombie.asset", "Enemies/Archetypes/Giant.asset"),
            Move("LevelCatalog.asset", "Levels/LevelCatalog.asset"),
            Move("Level01.asset", "Levels/Level01/LevelConfig.asset"),
            Move("Level01_Camera.asset", "Levels/Level01/CameraProfile.asset"),
            Move("Level01_Wave01.asset", "Levels/Level01/Waves/Wave01.asset"),
            Move("Level01_Wave02.asset", "Levels/Level01/Waves/Wave02.asset"),
            Move("Level01_Wave03.asset", "Levels/Level01/Waves/Wave03.asset"),
            Move("Level01_Waves_Wave.asset", "Levels/Level01/Waves/Wave04.asset"),
            Move("Level01_Waves.asset", "Levels/Level01/Waves/WaveSequence.asset"),
            Move("Level02.asset", "Levels/Level02/LevelConfig.asset"),
            Move("Level02_Camera.asset", "Levels/Level02/CameraProfile.asset"),
            Move("Level02_Wave01.asset", "Levels/Level02/Waves/Wave01.asset"),
            Move("Level02_Wave02.asset", "Levels/Level02/Waves/Wave02.asset"),
            Move("Level02_Wave03.asset", "Levels/Level02/Waves/Wave03.asset"),
            Move("Level02_Waves.asset", "Levels/Level02/Waves/WaveSequence.asset"),
            Move("SoldierAnimator.controller", "Player/Animation/SoldierAnimator.controller"),
            Move("SoldierUpperBody.mask", "Player/Animation/SoldierUpperBody.mask"),
            Move("Rifle.asset", "Weapons/Rifle.asset"),
            Move("Shotgun.asset", "Weapons/Shotgun.asset")
        };

        [MenuItem("Zombie War/Tools/Organize Config Assets")]
        public static void Organize()
        {
            bool requiresMigration = AssetDatabase.LoadMainAssetAtPath(ConfigRoot + "/LevelCatalog.asset") != null;
            if (requiresMigration)
            {
                ValidateDuplicateFoldersAreUnused();
                DeleteDuplicateFolders();
            }

            EnsureCanonicalFolders();
            for (int i = 0; i < AssetMoves.Length; i++)
            {
                MoveAsset(AssetMoves[i].Key, AssetMoves[i].Value);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Zombie War] Config assets organized by feature with GUID references preserved.");
        }

        private static void ValidateDuplicateFoldersAreUnused()
        {
            HashSet<string> duplicateAssets = new();
            for (int i = 0; i < DuplicateRoots.Length; i++)
            {
                string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { DuplicateRoots[i] });
                for (int j = 0; j < guids.Length; j++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[j]);
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        duplicateAssets.Add(path);
                    }
                }
            }

            string[] projectGuids = AssetDatabase.FindAssets(string.Empty, new[] { "Assets/_ZombieWar" });
            for (int i = 0; i < projectGuids.Length; i++)
            {
                string ownerPath = AssetDatabase.GUIDToAssetPath(projectGuids[i]);
                if (duplicateAssets.Contains(ownerPath) || IsInsideDuplicateRoot(ownerPath))
                {
                    continue;
                }

                string[] dependencies = AssetDatabase.GetDependencies(ownerPath, false);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    if (duplicateAssets.Contains(dependencies[j]))
                    {
                        throw new InvalidOperationException(
                            $"Cannot remove duplicate config because {ownerPath} references {dependencies[j]}.");
                    }
                }
            }
        }

        private static bool IsInsideDuplicateRoot(string path)
        {
            for (int i = 0; i < DuplicateRoots.Length; i++)
            {
                if (path.StartsWith(DuplicateRoots[i] + "/", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static void DeleteDuplicateFolders()
        {
            for (int i = 0; i < DuplicateRoots.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(DuplicateRoots[i]) && !AssetDatabase.DeleteAsset(DuplicateRoots[i]))
                {
                    throw new InvalidOperationException($"Unable to remove duplicate config folder: {DuplicateRoots[i]}");
                }
            }
        }

        private static void EnsureCanonicalFolders()
        {
            EnsureFolder(ConfigRoot, "Audio");
            EnsureFolder(ConfigRoot + "/Audio", "Weapons");
            EnsureFolder(ConfigRoot + "/Audio", "Zombies");
            EnsureFolder(ConfigRoot, "Enemies");
            EnsureFolder(ConfigRoot + "/Enemies", "Animation");
            EnsureFolder(ConfigRoot + "/Enemies", "Archetypes");
            EnsureFolder(ConfigRoot, "Levels");
            EnsureFolder(ConfigRoot + "/Levels", "Level01");
            EnsureFolder(ConfigRoot + "/Levels/Level01", "Waves");
            EnsureFolder(ConfigRoot + "/Levels", "Level02");
            EnsureFolder(ConfigRoot + "/Levels/Level02", "Waves");
            EnsureFolder(ConfigRoot, "Player");
            EnsureFolder(ConfigRoot + "/Player", "Animation");
            EnsureFolder(ConfigRoot, "Weapons");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void MoveAsset(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(destinationPath) != null)
            {
                return;
            }
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
            {
                throw new InvalidOperationException($"Config asset was not found: {sourcePath}");
            }

            string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private static KeyValuePair<string, string> Move(string sourceRelative, string destinationRelative)
        {
            return new KeyValuePair<string, string>(
                ConfigRoot + "/" + sourceRelative,
                ConfigRoot + "/" + destinationRelative);
        }
    }
}
