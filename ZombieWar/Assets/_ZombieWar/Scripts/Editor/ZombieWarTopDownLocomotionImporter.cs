using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZombieWar.Editor
{
    public static class ZombieWarTopDownLocomotionImporter
    {
        #region Config
        private const string SourceRoot = "Assets/TopDownEngine/Demos/Colonel";
        private const string DestinationRoot = "Assets/_ZombieWar/Art/TopDownLocomotion";

        private static readonly string[] SourceAssets =
        {
            SourceRoot + "/Models/FBI@T-Pose.fbx",
            SourceRoot + "/Animations/Run/FBI@RunForward.fbx",
            SourceRoot + "/Animations/Run/FBI@RunForwardRight.fbx",
            SourceRoot + "/Animations/Run/FBI@RunRight.fbx",
            SourceRoot + "/Animations/Run/FBI@RunBackwardRight.fbx",
            SourceRoot + "/Animations/Run/FBI@RunBackward.fbx",
            SourceRoot + "/Animations/Run/FBI@RunBackwardLeft.fbx",
            SourceRoot + "/Animations/Run/FBI@RunLeft.fbx",
            SourceRoot + "/Animations/Run/FBI@RunForwardLeft.fbx",
            SourceRoot + "/Animations/Various/FBI@RifleAimingIdle.fbx"
        };
        #endregion

        #region API
        [MenuItem("Zombie War/Import TopDown Locomotion Snapshot")]
        public static void Import()
        {
            EnsureDestinationFolder();
            for (int i = 0; i < SourceAssets.Length; i++)
            {
                CopyAsset(SourceAssets[i]);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string GetSnapshotPath(string fileName)
        {
            return DestinationRoot + "/" + fileName;
        }
        #endregion

        #region Internal
        private static void EnsureDestinationFolder()
        {
            string[] folders = DestinationRoot.Split('/');
            string current = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string next = current + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, folders[i]);
                }
                current = next;
            }
        }

        private static void CopyAsset(string sourcePath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
            {
                throw new FileNotFoundException($"TopDown locomotion source is missing: {sourcePath}");
            }

            string destinationPath = GetSnapshotPath(Path.GetFileName(sourcePath));
            if (AssetDatabase.LoadMainAssetAtPath(destinationPath) == null
                && !AssetDatabase.CopyAsset(sourcePath, destinationPath))
            {
                throw new IOException($"Could not copy TopDown locomotion asset to: {destinationPath}");
            }

            ConfigureHumanoidClip(destinationPath);
        }

        private static void ConfigureHumanoidClip(string assetPath)
        {
            if (AssetImporter.GetAtPath(assetPath) is not ModelImporter importer)
            {
                return;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = !assetPath.EndsWith("FBI@T-Pose.fbx");
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = true;
                clips[i].loopPose = true;
                clips[i].keepOriginalPositionXZ = true;
                clips[i].keepOriginalPositionY = true;
                clips[i].keepOriginalOrientation = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }
        #endregion
    }
}
