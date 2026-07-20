using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Core;
using ZombieWar.Levels;

namespace ZombieWar.Editor
{
    public static class LevelTransitionInstaller
    {
        private const string SceneFolder = "Assets/_ZombieWar/Scenes";
        private const string PrefabFolder = "Assets/_ZombieWar/Prefabs";
        private const string MaterialFolder = "Assets/_ZombieWar/Materials";
        private const string PortalPrefabPath = PrefabFolder + "/LevelExitPortal.prefab";
        private const string PortalMaterialPath = MaterialFolder + "/ExitPortal.mat";
        private const string BootScenePath = SceneFolder + "/Boot.unity";
        private const string LoadingScenePath = SceneFolder + "/Loading.unity";

        [MenuItem("Zombie War/Levels/Install Level Transitions")]
        public static void InstallLevelTransitions()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScene = SceneManager.GetActiveScene().path;
            GetOrCreatePortalPrefab();
            EnsureLoadingScene(false);

            LevelCatalogConfig catalog = LoadLevelCatalog();
            if (catalog == null)
            {
                Debug.LogError("[Zombie War] Level transition install requires a LevelCatalogConfig asset.");
                return;
            }

            LevelDefinition[] levels = catalog.Levels;
            for (int i = 0; i < levels.Length; i++)
            {
                LevelDefinition level = levels[i];
                if (level == null || !level.IsValid)
                {
                    continue;
                }

                string scenePath = $"{SceneFolder}/{level.SceneName}.unity";
                if (!File.Exists(Path.GetFullPath(scenePath)))
                {
                    Debug.LogWarning($"[Zombie War] Skipped missing level scene: {scenePath}");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                LevelSceneController controller = Object.FindFirstObjectByType<LevelSceneController>();
                if (controller == null)
                {
                    Debug.LogError($"[Zombie War] {level.SceneName} has no LevelSceneController.");
                    continue;
                }

                LevelExitPortal portal = Object.FindFirstObjectByType<LevelExitPortal>(FindObjectsInactive.Include);
                if (portal == null)
                {
                    portal = InstantiatePortal(scene);
                }
                controller.SetExitPortal(portal);
                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(Path.GetFullPath(previousScene)))
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
            Debug.Log("[Zombie War] Level portals and Loading scene are ready.");
        }

        public static GameObject GetOrCreatePortalPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PortalPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            Material material = GetOrCreatePortalMaterial();
            GameObject root = new("Level Exit Portal", typeof(LevelExitPortal));
            CapsuleCollider trigger = root.AddComponent<CapsuleCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.35f;
            trigger.height = 3.2f;
            trigger.center = new Vector3(0f, 1.55f, 0f);

            GameObject visualRoot = new("Portal Visuals");
            visualRoot.transform.SetParent(root.transform, false);
            visualRoot.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            Transform[] rings =
            {
                CreateRing(visualRoot.transform, "Outer Ring", 1.32f, 0.11f, material, Vector3.zero),
                CreateRing(visualRoot.transform, "Inner Ring", 0.92f, 0.075f, material, new Vector3(0f, 20f, 0f)),
                CreateRing(visualRoot.transform, "Core Ring", 0.55f, 0.045f, material, new Vector3(0f, -30f, 0f))
            };

            Light portalLight = new GameObject("Portal Light", typeof(Light)).GetComponent<Light>();
            portalLight.transform.SetParent(visualRoot.transform, false);
            portalLight.type = LightType.Point;
            portalLight.color = new Color(0.15f, 0.92f, 1f);
            portalLight.range = 7f;
            portalLight.intensity = 4f;

            ParticleSystem particles = new GameObject("Portal Particles", typeof(ParticleSystem)).GetComponent<ParticleSystem>();
            particles.transform.SetParent(visualRoot.transform, false);
            ConfigureParticles(particles, material);

            LevelExitPortal portal = root.GetComponent<LevelExitPortal>();
            portal.SetReferences(visualRoot, trigger, rings, portalLight, particles);
            visualRoot.SetActive(false);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PortalPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static LevelExitPortal InstantiatePortal(Scene scene)
        {
            GameObject prefab = GetOrCreatePortalPrefab();
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "Level Exit Portal";
            instance.transform.position = new Vector3(0f, 0f, 18f);
            instance.transform.rotation = Quaternion.identity;
            return instance.GetComponent<LevelExitPortal>();
        }

        public static void EnsureLoadingScene(bool overwrite)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<SceneAsset>(LoadingScenePath) != null)
            {
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath) == null)
            {
                throw new FileNotFoundException("Boot scene must be authored before the Loading scene.", BootScenePath);
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LoadingScenePath) != null)
            {
                AssetDatabase.DeleteAsset(LoadingScenePath);
            }
            if (!AssetDatabase.CopyAsset(BootScenePath, LoadingScenePath))
            {
                throw new IOException("Could not create the authored Loading scene.");
            }
            AssetDatabase.ImportAsset(LoadingScenePath, ImportAssetOptions.ForceUpdate);
        }

        private static Transform CreateRing(
            Transform parent,
            string name,
            float radius,
            float width,
            Material material,
            Vector3 localEulerAngles)
        {
            LineRenderer line = new GameObject(name, typeof(LineRenderer)).GetComponent<LineRenderer>();
            line.transform.SetParent(parent, false);
            line.transform.localEulerAngles = localEulerAngles;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 72;
            line.startWidth = width;
            line.endWidth = width;
            line.material = material;
            line.startColor = Color.white;
            line.endColor = new Color(0.2f, 0.85f, 1f);
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
            return line.transform;
        }

        private static void ConfigureParticles(ParticleSystem particles, Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.25f, 0.9f, 1f), Color.white);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 80;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 24f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.25f;
            shape.radiusThickness = 0.15f;
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Material GetOrCreatePortalMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(PortalMaterialPath);
            if (material != null)
            {
                return material;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            material = new Material(shader)
            {
                name = "ExitPortal",
                color = new Color(0.08f, 0.88f, 1f, 1f)
            };
            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", new Color(0.1f, 2.5f, 3.5f));
            }
            AssetDatabase.CreateAsset(material, PortalMaterialPath);
            return material;
        }

        private static LevelCatalogConfig LoadLevelCatalog()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelCatalogConfig", new[] { "Assets/_ZombieWar/Configs" });
            return guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<LevelCatalogConfig>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : null;
        }

    }
}
