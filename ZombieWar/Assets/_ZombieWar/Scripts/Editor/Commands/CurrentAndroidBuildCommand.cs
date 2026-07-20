using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieWar.Editor
{
    public static class CurrentAndroidBuildCommand
    {
        #region Config

        private const string OutputDirectory = "Builds/Android";
        private const string OutputFileName = "ZombieWar.apk";

        #endregion

        #region API

        [MenuItem("Zombie War/Build/Build Current Android")]
        public static void Build()
        {
            ValidateEditorState();
            string[] scenes = GetCurrentBuildScenes();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                throw new BuildFailedException("Addressables settings were not found.");
            }

            AssetFolderSnapshot addressablesSnapshot = AssetFolderSnapshot.Capture("Assets/AddressableAssetsData");
            AssetFolderSnapshot streamingAssetsSnapshot = AssetFolderSnapshot.Capture("Assets/StreamingAssets");
            AddressableAssetSettings.PlayerBuildOption originalPlayerBuildOption =
                settings.BuildAddressablesWithPlayerBuild;

            try
            {
                BuildAddressables();
                settings.BuildAddressablesWithPlayerBuild =
                    AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
                BuildPlayer(scenes);
            }
            finally
            {
                settings.BuildAddressablesWithPlayerBuild = originalPlayerBuildOption;
                streamingAssetsSnapshot.Restore();
                addressablesSnapshot.Restore();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }

        #endregion

        #region Internal

        private static void ValidateEditorState()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new BuildFailedException("Exit Play Mode before building.");
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                throw new BuildFailedException(
                    "Android must already be the active platform. Switch it manually in Build Profiles first.");
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    throw new BuildFailedException(
                        $"Scene '{scene.name}' has unsaved changes. Save it manually before building.");
                }
            }
        }

        private static string[] GetCurrentBuildScenes()
        {
            List<string> scenes = new();
            EditorBuildSettingsScene[] configuredScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < configuredScenes.Length; i++)
            {
                EditorBuildSettingsScene scene = configuredScenes[i];
                if (!scene.enabled)
                {
                    continue;
                }

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                {
                    throw new BuildFailedException($"Enabled build scene was not found: {scene.path}");
                }

                scenes.Add(scene.path);
            }

            if (scenes.Count == 0)
            {
                throw new BuildFailedException("Build Profiles has no enabled scenes.");
            }

            return scenes.ToArray();
        }

        private static void BuildAddressables()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new BuildFailedException($"Addressables build failed: {result.Error}");
            }
        }

        private static void BuildPlayer(string[] scenes)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new BuildFailedException("Unity project root could not be resolved.");
            }

            string outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, OutputDirectory));
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, OutputFileName);
            BuildPlayerOptions options = new()
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build failed: {report.summary.result}");
            }

            Debug.Log($"[Zombie War] Android build completed without changing project assets: {outputPath}");
        }

        #endregion
    }
}
