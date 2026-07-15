using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Enemies;
using ZombieWar.Player;
using ZombieWar.UI;

namespace ZombieWar.Editor
{
    public static class CombatTextInstaller
    {
        private const int PoolCapacity = 96;
        private const string PrefabFolder = "Assets/_ZombieWar/Prefabs/UI";
        private const string CombatTextPrefabPath = PrefabFolder + "/FloatingCombatText.prefab";
        private const string MaterialFolder = "Assets/_ZombieWar/Materials/UI";
        private const string CombatTextMaterialPath = MaterialFolder + "/FloatingCombatText.mat";
        private const string SoldierPrefabPath = "Assets/_ZombieWar/Prefabs/Soldier.prefab";
        private const string ZombiePrefabPath = "Assets/_ZombieWar/Prefabs/Zombie.prefab";
        private const string SceneFolder = "Assets/_ZombieWar/Scenes";

        [MenuItem("Zombie War/Polish/Install Floating Combat Text")]
        public static void Install()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScene = SceneManager.GetActiveScene().path;
            CreateOrReplacePrefab();
            ConfigureGameplayPrefabs();
            InstallIntoScene("Level01");
            InstallIntoScene("Level02");
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(Path.GetFullPath(previousScene)))
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
            Debug.Log("[Zombie War] Floating damage and healing text installed for soldier and zombies.");
        }

        public static FloatingCombatText CreateOrReplacePrefab()
        {
            EnsureFolder(PrefabFolder);
            Material combatTextMaterial = GetOrCreateMaterial();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CombatTextPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(CombatTextPrefabPath);
            }

            GameObject root = new("Floating Combat Text", typeof(TextMeshPro), typeof(FloatingCombatText));
            root.transform.localScale = Vector3.one * 0.65f;
            TextMeshPro label = root.GetComponent<TextMeshPro>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 5.5f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.text = "-18";
            label.color = new Color(1f, 0.65f, 0.12f);
            label.fontSharedMaterial = combatTextMaterial;
            label.rectTransform.sizeDelta = new Vector2(5f, 1.5f);
            label.GetComponent<MeshRenderer>().sortingOrder = 120;

            FloatingCombatText combatText = root.GetComponent<FloatingCombatText>();
            combatText.SetLabel(label);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CombatTextPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<FloatingCombatText>();
        }

        public static void ConfigureGameplayPrefabs()
        {
            ConfigureEmitter(SoldierPrefabPath, true, 2.15f, false);
            ConfigureEmitter(ZombiePrefabPath, false, 2f, true);
        }

        public static FloatingCombatTextPool CreateScenePool(Transform parent, Camera worldCamera)
        {
            Transform existing = FindChild(parent, "Floating Combat Text Pool");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            FloatingCombatText prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CombatTextPrefabPath)
                .GetComponent<FloatingCombatText>();
            GameObject poolObject = new("Floating Combat Text Pool", typeof(FloatingCombatTextPool));
            poolObject.transform.SetParent(parent, false);
            FloatingCombatText[] entries = new FloatingCombatText[PoolCapacity];
            for (int i = 0; i < entries.Length; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, poolObject.transform);
                instance.name = $"Combat Text {i + 1:00}";
                instance.SetActive(false);
                entries[i] = instance.GetComponent<FloatingCombatText>();
            }

            FloatingCombatTextPool pool = poolObject.GetComponent<FloatingCombatTextPool>();
            pool.SetReferences(worldCamera, entries);
            return pool;
        }

        private static void InstallIntoScene(string sceneName)
        {
            string scenePath = $"{SceneFolder}/{sceneName}.unity";
            if (!File.Exists(Path.GetFullPath(scenePath)))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject systems = GameObject.Find("Gameplay Systems");
            Camera worldCamera = Object.FindFirstObjectByType<Camera>();
            EnemyPool enemyPool = Object.FindFirstObjectByType<EnemyPool>();
            SoldierController soldier = Object.FindFirstObjectByType<SoldierController>();
            if (systems == null || worldCamera == null || enemyPool == null || soldier == null)
            {
                Debug.LogError($"[Zombie War] {sceneName} is missing a combat text dependency.");
                return;
            }

            FloatingCombatTextPool pool = CreateScenePool(systems.transform, worldCamera);
            enemyPool.SetCombatTextPool(pool);
            if (!soldier.TryGetComponent(out FloatingCombatTextEmitter emitter))
            {
                emitter = soldier.gameObject.AddComponent<FloatingCombatTextEmitter>();
                emitter.Configure(true, 2.15f, false);
            }
            emitter.SetPool(pool);
            EditorUtility.SetDirty(enemyPool);
            EditorUtility.SetDirty(emitter);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureEmitter(
            string prefabPath,
            bool isPlayer,
            float height,
            bool useDamagePoint)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (!root.TryGetComponent(out FloatingCombatTextEmitter emitter))
                {
                    emitter = root.AddComponent<FloatingCombatTextEmitter>();
                }
                emitter.Configure(isPlayer, height, useDamagePoint);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == name)
                {
                    return children[i];
                }
            }
            return null;
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }
                current = next;
            }
        }

        private static Material GetOrCreateMaterial()
        {
            EnsureFolder(MaterialFolder);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(CombatTextMaterialPath);
            if (material == null)
            {
                material = new Material(TMP_Settings.defaultFontAsset.material)
                {
                    name = "FloatingCombatText"
                };
                AssetDatabase.CreateAsset(material, CombatTextMaterialPath);
            }

            material.DisableKeyword("OUTLINE_ON");
            if (material.HasProperty("_OutlineWidth"))
            {
                material.SetFloat("_OutlineWidth", 0f);
            }
            if (material.HasProperty("unity_GUIZTestMode"))
            {
                material.SetFloat("unity_GUIZTestMode", 4f);
            }
            if (material.HasProperty("_ZTestMode"))
            {
                material.SetFloat("_ZTestMode", 4f);
            }
            material.renderQueue = TMP_Settings.defaultFontAsset.material.renderQueue;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
