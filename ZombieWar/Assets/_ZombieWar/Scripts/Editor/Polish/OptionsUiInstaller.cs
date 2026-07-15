using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieWar.UI;

namespace ZombieWar.Editor
{
    public static class OptionsUiInstaller
    {
        private const string OverlayPrefabPath = "Assets/_ZombieWar/Prefabs/UI/OptionsOverlay.prefab";
        private const string HudPrefabPath = "Assets/_ZombieWar/Prefabs/LandscapeHUD.prefab";
        private const string MainMenuScenePath = "Assets/_ZombieWar/Scenes/MainMenu.unity";
        private const string LayerLabRoot = "Assets/Layer Lab/GUI Pro-SurvivalClean";
        private const string ButtonPrefabPath = LayerLabRoot + "/Prefabs/Prefabs_Component_Buttons/Btn_IconTextButton_Square07_Blue.prefab";
        private const string TogglePrefabPath = LayerLabRoot + "/Prefabs/Prefabs_Component_UI_Etc/Toggle_Switch.prefab";

        [MenuItem("Zombie War/Polish/Install FPS And Options")]
        public static void Install()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScene = SceneManager.GetActiveScene().path;
            CreateOrReplaceOverlayPrefab();
            InstallIntoHudPrefab();
            InstallIntoMainMenu();
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(previousScene) && File.Exists(Path.GetFullPath(previousScene)))
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
            Debug.Log("[Zombie War] FPS display and Options UI installed into HUD and Main Menu.");
        }

        public static GameObject CreateOrReplaceOverlayPrefab()
        {
            EnsureFolder("Assets/_ZombieWar/Prefabs/UI");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(OverlayPrefabPath);
            }

            GameObject root = new("Options Overlay", typeof(RectTransform), typeof(OptionsPanelController));
            Stretch(root.GetComponent<RectTransform>());

            TMP_Text fpsLabel = CreateText(root.transform, "FPS", "FPS  --", 25, TextAlignmentOptions.Left);
            SetAnchoredRect(fpsLabel.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(190f, 44f), new Vector2(0f, 1f));
            fpsLabel.color = new Color(0.45f, 1f, 0.72f);
            fpsLabel.gameObject.AddComponent<FpsDisplay>().SetLabel(fpsLabel);

            Button openButton = CreateLayerLabButton(root.transform, "Options Button", "OPTIONS");
            SetAnchoredRect(openButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-66f, -62f), new Vector2(190f, 150f), new Vector2(1f, 1f));
            openButton.transform.localScale = Vector3.one * 0.62f;

            Image panel = CreateImage(root.transform, "Options Panel", new Color(0.018f, 0.045f, 0.075f, 0.98f));
            SetAnchoredRect(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 620f), new Vector2(0.5f, 0.5f));
            TMP_Text title = CreateText(panel.transform, "Title", "OPTIONS", 52, TextAlignmentOptions.Center);
            SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(620f, 80f), new Vector2(0.5f, 1f));
            title.color = new Color(0.48f, 0.93f, 1f);

            Toggle sfxToggle = CreateOptionRow(panel.transform, "Sound Effects", "SOUND EFFECTS", -170f, out TMP_Text sfxState);
            Toggle musicToggle = CreateOptionRow(panel.transform, "Music", "MUSIC", -285f, out TMP_Text musicState);
            Toggle shakeToggle = CreateOptionRow(panel.transform, "Camera Shake", "CAMERA SHAKE", -400f, out TMP_Text shakeState);

            Button closeButton = CreateLayerLabButton(panel.transform, "Close Button", "CLOSE");
            SetAnchoredRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(250f, 150f), new Vector2(0.5f, 0f));
            closeButton.transform.localScale = Vector3.one * 0.68f;

            OptionsPanelController controller = root.GetComponent<OptionsPanelController>();
            controller.SetViewReferences(
                openButton,
                panel.gameObject,
                closeButton,
                sfxToggle,
                musicToggle,
                shakeToggle,
                sfxState,
                musicState,
                shakeState);
            panel.gameObject.SetActive(false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, OverlayPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static GameObject InstantiateOverlay(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath);
            if (prefab == null)
            {
                prefab = CreateOrReplaceOverlayPrefab();
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = "Options Overlay";
            Stretch(instance.GetComponent<RectTransform>());
            instance.transform.SetAsLastSibling();
            return instance;
        }

        private static void InstallIntoHudPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);
            try
            {
                Transform safeArea = FindChild(root.transform, "Safe Area");
                ReplaceOverlay(safeArea);
                PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void InstallIntoMainMenu()
        {
            if (!File.Exists(Path.GetFullPath(MainMenuScenePath)))
            {
                return;
            }
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            GameObject canvas = GameObject.Find("Menu Canvas");
            if (canvas == null)
            {
                Debug.LogError("[Zombie War] MainMenu requires an authored Menu Canvas.");
                return;
            }
            ReplaceOverlay(canvas.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ReplaceOverlay(Transform parent)
        {
            Transform existing = FindChild(parent, "Options Overlay");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
            InstantiateOverlay(parent);
        }

        private static Toggle CreateOptionRow(
            Transform parent,
            string name,
            string label,
            float y,
            out TMP_Text state)
        {
            TMP_Text rowLabel = CreateText(parent, name + " Label", label, 31, TextAlignmentOptions.Left);
            SetAnchoredRect(rowLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(-115f, y), new Vector2(380f, 70f), new Vector2(0.5f, 1f));

            GameObject togglePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TogglePrefabPath);
            GameObject toggleObject = (GameObject)PrefabUtility.InstantiatePrefab(togglePrefab, parent);
            toggleObject.name = name + " Toggle";
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            if (toggle == null)
            {
                toggle = toggleObject.AddComponent<Toggle>();
            }
            Image[] graphics = toggleObject.GetComponentsInChildren<Image>(true);
            if (graphics.Length > 0)
            {
                toggle.targetGraphic = graphics[0];
                toggle.graphic = FindGraphic(graphics, "Handle");
            }
            toggle.navigation = new Navigation { mode = Navigation.Mode.None };
            SetAnchoredRect(toggleObject.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(190f, y - 3f), new Vector2(150f, 70f), new Vector2(0.5f, 1f));
            toggleObject.transform.localScale = Vector3.one * 0.82f;

            state = CreateText(parent, name + " State", "ON", 26, TextAlignmentOptions.Center);
            SetAnchoredRect(state.rectTransform, new Vector2(0.5f, 1f), new Vector2(300f, y), new Vector2(100f, 70f), new Vector2(0.5f, 1f));
            return toggle;
        }

        private static Graphic FindGraphic(Image[] graphics, string objectName)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i].name == objectName)
                {
                    return graphics[i];
                }
            }
            return graphics.Length > 1 ? graphics[graphics.Length - 1] : graphics[0];
        }

        private static Button CreateLayerLabButton(Transform parent, string name, string label)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            TMP_Text text = instance.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }
            Transform counter = FindChild(instance.transform, "Text");
            if (counter != null)
            {
                counter.gameObject.SetActive(false);
            }
            return instance.GetComponent<Button>();
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string value,
            float size,
            TextAlignmentOptions alignment)
        {
            GameObject instance = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            instance.transform.SetParent(parent, false);
            TextMeshProUGUI text = instance.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject instance = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            instance.transform.SetParent(parent, false);
            Image image = instance.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void SetAnchoredRect(
            RectTransform rect,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
    }
}
