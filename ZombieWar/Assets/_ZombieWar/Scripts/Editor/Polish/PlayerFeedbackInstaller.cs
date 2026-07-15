using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieWar.Combat;
using ZombieWar.Levels;
using ZombieWar.Player;
using ZombieWar.UI;

namespace ZombieWar.Editor
{
    public static class PlayerFeedbackInstaller
    {
        private const string HudPrefabPath = "Assets/_ZombieWar/Prefabs/LandscapeHUD.prefab";
        private const string SoldierPrefabPath = "Assets/_ZombieWar/Prefabs/Soldier.prefab";

        [MenuItem("Zombie War/Polish/Install Player Feedback _F8")]
        public static void Install()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScenePath = SceneManager.GetActiveScene().path;
            InstallHudPrefab();
            InstallSoldierPrefab();
            InstallCameraListeners();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }
            Debug.Log("[Zombie War] Player feedback installed: camera impulse, damage, healing and low-health overlays.");
        }

        private static void InstallHudPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(HudPrefabPath);
            try
            {
                RuntimeHud hud = prefabRoot.GetComponent<RuntimeHud>();
                if (hud == null)
                {
                    throw new MissingComponentException("LandscapeHUD prefab has no RuntimeHud component.");
                }

                Transform existing = prefabRoot.transform.Find("Screen Feedback");
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                GameObject feedbackObject = new("Screen Feedback", typeof(RectTransform), typeof(ScreenFeedbackView));
                feedbackObject.transform.SetParent(prefabRoot.transform, false);
                RectTransform feedbackRect = feedbackObject.GetComponent<RectTransform>();
                Stretch(feedbackRect);
                feedbackRect.SetAsFirstSibling();

                CanvasGroup damage = CreateOverlay(feedbackRect, "Damage Flash", new Color(0.85f, 0.015f, 0.01f, 1f));
                CanvasGroup healing = CreateOverlay(feedbackRect, "Healing Flash", new Color(0.05f, 0.9f, 0.42f, 1f));
                CanvasGroup lowHealth = CreateOverlay(feedbackRect, "Low Health Pulse", new Color(0.55f, 0.005f, 0.005f, 1f));
                ScreenFeedbackView feedback = feedbackObject.GetComponent<ScreenFeedbackView>();
                feedback.SetViewReferences(damage, healing, lowHealth);
                hud.SetScreenFeedback(feedback);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, HudPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void InstallSoldierPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(SoldierPrefabPath);
            try
            {
                Health health = prefabRoot.GetComponent<Health>();
                WeaponController weapon = prefabRoot.GetComponent<WeaponController>();
                BombController bomb = prefabRoot.GetComponent<BombController>();
                if (health == null || weapon == null || bomb == null)
                {
                    throw new MissingComponentException("Soldier prefab is missing feedback dependencies.");
                }

                CinemachineImpulseSource impulse = prefabRoot.GetComponent<CinemachineImpulseSource>();
                if (impulse == null)
                {
                    impulse = prefabRoot.AddComponent<CinemachineImpulseSource>();
                }
                ConfigureImpulse(impulse);

                CameraShakeController shake = prefabRoot.GetComponent<CameraShakeController>();
                if (shake == null)
                {
                    shake = prefabRoot.AddComponent<CameraShakeController>();
                }
                shake.SetReferences(impulse, health, weapon, bomb);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, SoldierPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void InstallCameraListeners()
        {
            LevelCatalogConfig catalog = FindLevelCatalog();
            if (catalog.Levels == null)
            {
                return;
            }

            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                LevelDefinition level = catalog.Levels[i];
                if (level == null || string.IsNullOrWhiteSpace(level.SceneName))
                {
                    continue;
                }

                string scenePath = $"Assets/_ZombieWar/Scenes/{level.SceneName}.unity";
                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning($"[Zombie War] Feedback installer skipped missing scene: {scenePath}");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                CinemachineCamera camera = Object.FindFirstObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
                if (camera == null)
                {
                    Debug.LogWarning($"[Zombie War] No CinemachineCamera found in {level.SceneName}.");
                    continue;
                }

                CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
                if (listener == null)
                {
                    listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();
                }
                listener.ChannelMask = 1;
                listener.Gain = 0.55f;
                listener.UseCameraSpace = true;
                listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.UseLargest;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static LevelCatalogConfig FindLevelCatalog()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelCatalogConfig");
            if (guids.Length == 0)
            {
                throw new FileNotFoundException("No LevelCatalogConfig asset was found.");
            }
            return AssetDatabase.LoadAssetAtPath<LevelCatalogConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static CanvasGroup CreateOverlay(RectTransform parent, string name, Color color)
        {
            GameObject overlay = new(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            overlay.transform.SetParent(parent, false);
            Stretch(overlay.GetComponent<RectTransform>());
            CanvasGroup group = overlay.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            Image image = overlay.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return group;
        }

        private static void ConfigureImpulse(CinemachineImpulseSource source)
        {
            source.ImpulseDefinition.ImpulseChannel = 1;
            source.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            source.ImpulseDefinition.ImpulseDuration = 0.16f;
            source.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            source.ImpulseDefinition.DissipationDistance = 100f;
            source.ImpulseDefinition.DissipationRate = 0.25f;
            source.ImpulseDefinition.PropagationSpeed = 343f;
            source.DefaultVelocity = new Vector3(0.7f, -1f, 0.25f);
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
