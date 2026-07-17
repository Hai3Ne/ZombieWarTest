using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ZombieWar.Editor.Environment
{
    public static class MegaCityEnvironmentInstaller
    {
        private const string MenuPath = "Zombie War/Environment/Install Low Poly Mega City";
        private const string PackRoot = "Assets/JC_LP_MegaCity/Prefabs";
        private const string OutputRoot = "Assets/_ZombieWar/Prefabs/Environment/MegaCity";
        private const string EnvironmentRootName = "Mega City Environment";
        private const string LevelOneScenePath = "Assets/_ZombieWar/Scenes/Level01.unity";
        private const string LevelTwoScenePath = "Assets/_ZombieWar/Scenes/Level02.unity";
        private const string LevelOnePrefabPath = OutputRoot + "/Level01_MegaCity.prefab";
        private const string LevelTwoPrefabPath = OutputRoot + "/Level02_MegaCity.prefab";

        [MenuItem(MenuPath)]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Mega City] Stop Play Mode before installing the environment.");
                return;
            }

            ValidateRequiredAssets();
            EnsureOutputFolders();

            string previousScenePath = SceneManager.GetActiveScene().path;

            try
            {
                BuildLevelOnePrefab();
                BuildLevelTwoPrefab();
                InstallPrefabIntoScene(LevelOneScenePath, LevelOnePrefabPath);
                InstallPrefabIntoScene(LevelTwoScenePath, LevelTwoPrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[Mega City] Installed authored city environments into Level01 and Level02.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousScenePath))
                {
                    EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
                }
            }
        }

        private static void BuildLevelOnePrefab()
        {
            GameObject root = CreateTemporaryRoot();

            try
            {
                Transform roads = CreateGroup(root.transform, "Road Surface");
                Transform skyline = CreateGroup(root.transform, "City Skyline");
                Transform props = CreateGroup(root.transform, "Gameplay Props");
                Transform dressing = CreateGroup(root.transform, "Street Dressing");

                AddRoadGrid(roads, "Floor/SM_Floor_Road_01_M.prefab");

                AddPrefab(skyline, "Buildings/SM_Buildings_Commercial_01.prefab", new Vector3(-22.5f, 0f, -15f), 90f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Commercial_04.prefab", new Vector3(-22.5f, 0f, 1f), 90f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Hospital_01.prefab", new Vector3(-22.5f, 0f, 16f), 90f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Commercial_02.prefab", new Vector3(22.5f, 0f, -15f), 270f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Bank.prefab", new Vector3(22.5f, 0f, 1f), 270f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_FireStation_01.prefab", new Vector3(22.5f, 0f, 16f), 270f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_CityHall_01.prefab", new Vector3(-8f, 0f, 30f), 180f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_CityMall_01.prefab", new Vector3(9f, 0f, 31f), 180f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Car_Service.prefab", new Vector3(-9f, 0f, -30f), 0f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Gas_Station_01.prefab", new Vector3(9f, 0f, -30f), 0f, Vector3.one, false);

                AddPrefab(props, "Vehicles/SM_Vehicles_Police_01.prefab", new Vector3(-7f, 0.06f, -8f), 90f, new Vector3(0.9f, 0.9f, 0.9f), true);
                AddPrefab(props, "Vehicles/SM_Vehicles_Taxi_01.prefab", new Vector3(7f, 0.06f, 8f), 90f, new Vector3(0.9f, 0.9f, 0.9f), true);
                AddPrefab(props, "Vehicles/SM_Vehicles_Ambulance.prefab", new Vector3(-8f, 0.06f, 10f), 90f, new Vector3(0.82f, 0.82f, 0.82f), true);
                AddPrefab(props, "Vehicles/SM_Vehicles_04_V1.prefab", new Vector3(8f, 0.06f, -10f), 90f, new Vector3(0.9f, 0.9f, 0.9f), true);
                AddPrefab(props, "IndustrialProps/SM_IndustrialProps_Construction_03.prefab", new Vector3(0f, 0.06f, 14f), 0f, new Vector3(0.65f, 1f, 1f), true);
                AddPrefab(props, "IndustrialProps/SM_IndustrialProps_Construction_03.prefab", new Vector3(0f, 0.06f, -14f), 180f, new Vector3(0.65f, 1f, 1f), true);

                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Barrel_01.prefab", new Vector3(-12f, 0.06f, -18f), 0f, Vector3.one, true);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Barrel_02.prefab", new Vector3(-13.2f, 0.06f, -18.4f), 20f, Vector3.one, true);
                AddPrefab(dressing, "Props/SM_Props_Bench_01.prefab", new Vector3(12.5f, 0.06f, 17f), 270f, Vector3.one, true);
                AddPrefab(dressing, "Props/SM_Props_Box_03.prefab", new Vector3(13f, 0.06f, -17f), 25f, Vector3.one, true);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Light_01.prefab", new Vector3(-14.5f, 0.06f, 17f), 0f, Vector3.one, false);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Light_01.prefab", new Vector3(14.5f, 0.06f, -17f), 180f, Vector3.one, false);

                SavePrefab(root, LevelOnePrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void BuildLevelTwoPrefab()
        {
            GameObject root = CreateTemporaryRoot();

            try
            {
                Transform roads = CreateGroup(root.transform, "Road Surface");
                Transform skyline = CreateGroup(root.transform, "Industrial Skyline");
                Transform props = CreateGroup(root.transform, "Gameplay Props");
                Transform dressing = CreateGroup(root.transform, "Construction Dressing");

                AddRoadGrid(roads, "Floor/SM_Floor_Road_02.prefab");
                AddPrefab(roads, "Floor/SM_Floor_Road_09_UP.prefab", new Vector3(0f, 0.45f, 9.87f), 0f, new Vector3(1f, 1f, 1.2f), false, 12f);

                AddPrefab(skyline, "Buildings/SM_Buildings_Factory_01.prefab", new Vector3(-23f, 0f, -15f), 90f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Factory_03.prefab", new Vector3(-23f, 0f, 2f), 90f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Construction_01.prefab", new Vector3(-23f, 0f, 18f), 90f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Factory_02.prefab", new Vector3(23f, 0f, -15f), 270f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Factory_05.prefab", new Vector3(23f, 0f, 2f), 270f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Construction_02.prefab", new Vector3(23f, 0f, 18f), 270f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Factory_06.prefab", new Vector3(-9f, 0f, 31f), 180f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Factory_07.prefab", new Vector3(10f, 0f, 31f), 180f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Gas_Station_03.prefab", new Vector3(-9f, 0f, -30f), 0f, Vector3.one, false);
                AddPrefab(skyline, "Buildings/SM_Buildings_Car_Service.prefab", new Vector3(10f, 0f, -30f), 0f, Vector3.one, false);

                AddPrefab(props, "Vehicles/SM_Vehicles_Truck_01_V1.prefab", new Vector3(-7f, 0.06f, -8f), 90f, new Vector3(0.72f, 0.72f, 0.72f), true);
                AddPrefab(props, "Vehicles/SM_Vehicles_Military_V1.prefab", new Vector3(7f, 0.06f, 8f), 90f, new Vector3(0.82f, 0.82f, 0.82f), true);
                AddPrefab(props, "IndustrialProps/SM_IndustrialProps_Construction_03.prefab", new Vector3(-8f, 0.06f, 10f), 0f, new Vector3(0.65f, 1f, 1f), true);
                AddPrefab(props, "Vehicles/SM_Vehicles_Police_02.prefab", new Vector3(8f, 0.06f, -10f), 90f, new Vector3(0.9f, 0.9f, 0.9f), true);
                AddPrefab(props, "IndustrialProps/SM_IndustrialProps_Construction_03.prefab", new Vector3(0f, 0.06f, 14f), 0f, new Vector3(0.65f, 1f, 1f), true);
                AddPrefab(props, "IndustrialProps/SM_IndustrialProps_Construction_03.prefab", new Vector3(0f, 0.06f, -14f), 180f, new Vector3(0.65f, 1f, 1f), true);

                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_ShippingContainer_02.prefab", new Vector3(-14f, 0.06f, 17f), 0f, new Vector3(0.55f, 0.55f, 0.55f), true);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Generator_01.prefab", new Vector3(13f, 0.06f, 17f), 30f, Vector3.one, true);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Barrel_03.prefab", new Vector3(13.5f, 0.06f, -17f), 0f, Vector3.one, true);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Barrel_04.prefab", new Vector3(14.6f, 0.06f, -17.4f), 0f, Vector3.one, true);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_TowerCrane_01.prefab", new Vector3(-25f, 0f, 25f), 35f, new Vector3(0.8f, 0.8f, 0.8f), false);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Light_01.prefab", new Vector3(-14.5f, 0.06f, -17f), 0f, Vector3.one, false);
                AddPrefab(dressing, "IndustrialProps/SM_IndustrialProps_Light_01.prefab", new Vector3(14.5f, 0.06f, 17f), 180f, Vector3.one, false);

                SavePrefab(root, LevelTwoPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AddRoadGrid(Transform parent, string prefabPath)
        {
            for (int column = 0; column < 4; column++)
            {
                for (int row = 0; row < 5; row++)
                {
                    float centerX = -15f + (column * 10f);
                    float centerZ = -20f + (row * 10f);
                    AddPrefab(parent, prefabPath, new Vector3(centerX, 0.015f, centerZ + 5f), 0f, Vector3.one, false);
                }
            }
        }

        private static GameObject AddPrefab(
            Transform parent,
            string relativePath,
            Vector3 position,
            float yaw,
            Vector3 scale,
            bool castShadows,
            float pitch = 0f)
        {
            string assetPath = PackRoot + "/" + relativePath;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                throw new InvalidOperationException("Missing Mega City prefab: " + assetPath);
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("Could not instantiate Mega City prefab: " + assetPath);
            }

            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            instance.transform.localScale = scale;

            DisableColliders(instance);
            ConfigureRenderers(instance, castShadows);
            MarkStatic(instance);
            return instance;
        }

        private static void DisableColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = false;
            }
        }

        private static void ConfigureRenderers(GameObject root, bool castShadows)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            ShadowCastingMode shadowMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;

            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].shadowCastingMode = shadowMode;
                renderers[index].receiveShadows = true;
            }
        }

        private static void MarkStatic(GameObject root)
        {
            StaticEditorFlags flags = StaticEditorFlags.BatchingStatic |
                                      StaticEditorFlags.OccludeeStatic |
                                      StaticEditorFlags.ReflectionProbeStatic;

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[index].gameObject, flags);
            }
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static GameObject CreateTemporaryRoot()
        {
            GameObject root = new GameObject(EnvironmentRootName);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (savedPrefab == null)
            {
                throw new InvalidOperationException("Could not save Mega City environment prefab: " + path);
            }
        }

        private static void InstallPrefabIntoScene(string scenePath, string prefabPath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject existing = FindRoot(scene, EnvironmentRootName);

            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("Could not install environment prefab into scene: " + scenePath);
            }

            instance.name = EnvironmentRootName;
            HideGrayboxRenderers(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void HideGrayboxRenderers(Scene scene)
        {
            GameObject environment = FindRoot(scene, "Environment");
            if (environment == null)
            {
                throw new InvalidOperationException("Scene is missing the authored Environment root: " + scene.path);
            }

            foreach (Transform child in environment.transform)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == name)
                {
                    return roots[index];
                }
            }

            return null;
        }

        private static void ValidateRequiredAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/JC_LP_MegaCity"))
            {
                throw new InvalidOperationException("Low Poly Mega City was not found at Assets/JC_LP_MegaCity.");
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PackRoot + "/Floor/SM_Floor_Road_01_M.prefab") == null)
            {
                throw new InvalidOperationException("Low Poly Mega City prefabs have not finished importing.");
            }
        }

        private static void EnsureOutputFolders()
        {
            EnsureFolder("Assets/_ZombieWar/Prefabs", "Environment");
            EnsureFolder("Assets/_ZombieWar/Prefabs/Environment", "MegaCity");
        }

        private static void EnsureFolder(string parent, string folderName)
        {
            string path = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
