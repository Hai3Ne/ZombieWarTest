using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.UI;

namespace ZombieWar.Editor
{
    public static class TwinStickAimInstaller
    {
        private const string MenuPath = "Zombie War/Gameplay/Install Twin Stick Aim";
        private const string ShootingIconPath = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Shooting.Png";

        [MenuItem(MenuPath)]
        public static void Install()
        {
            string hudPath = FindPrefabPath("LandscapeHUD");
            string joystickPath = FindPrefabPath("Play_Joystick_Direction");
            if (string.IsNullOrEmpty(hudPath) || string.IsNullOrEmpty(joystickPath))
            {
                Debug.LogError("[Zombie War] LandscapeHUD or Play_Joystick_Direction prefab was not found.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(hudPath);
            try
            {
                Transform safeArea = FindChild(root.transform, "Safe Area");
                RuntimeHud hud = root.GetComponent<RuntimeHud>();
                if (safeArea == null || hud == null)
                {
                    Debug.LogError("[Zombie War] LandscapeHUD is missing Safe Area or RuntimeHud.");
                    return;
                }

                Transform existing = FindChild(safeArea, "Aim Input Zone");
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                GameObject zoneObject = new("Aim Input Zone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                zoneObject.transform.SetParent(safeArea, false);
                RectTransform zone = zoneObject.GetComponent<RectTransform>();
                zone.anchorMin = new Vector2(0.52f, 0.02f);
                zone.anchorMax = new Vector2(0.98f, 0.82f);
                zone.offsetMin = Vector2.zero;
                zone.offsetMax = Vector2.zero;
                Image zoneImage = zoneObject.GetComponent<Image>();
                zoneImage.color = Color.clear;
                zoneImage.raycastTarget = true;

                GameObject joystickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(joystickPath);
                GameObject joystickObject = (GameObject)PrefabUtility.InstantiatePrefab(joystickPrefab, zone);
                joystickObject.name = "Aim Joystick";
                RectTransform visualRoot = joystickObject.GetComponent<RectTransform>();
                visualRoot.anchorMin = new Vector2(0.5f, 0.5f);
                visualRoot.anchorMax = new Vector2(0.5f, 0.5f);
                visualRoot.pivot = new Vector2(0.5f, 0.5f);
                visualRoot.anchoredPosition = Vector2.zero;
                visualRoot.localScale = Vector3.one * 0.72f;

                RectTransform handle = FindChild(joystickObject.transform, "Handle") as RectTransform;
                if (handle == null)
                {
                    Debug.LogError("[Zombie War] Aim joystick prefab is missing Handle.");
                    return;
                }

                handle.anchoredPosition = Vector2.zero;
                AddShootingIcon(handle);
                Graphic[] graphics = joystickObject.GetComponentsInChildren<Graphic>(true);
                for (int i = 0; i < graphics.Length; i++)
                {
                    graphics[i].raycastTarget = false;
                }

                VirtualJoystick aimJoystick = zoneObject.AddComponent<VirtualJoystick>();
                aimJoystick.Configure(zone, visualRoot, handle);
                hud.SetAimJoystick(aimJoystick);

                int movementIndex = FindChild(safeArea, "Joystick Input Zone")?.GetSiblingIndex() ?? 0;
                zone.SetSiblingIndex(movementIndex + 1);
                PrefabUtility.SaveAsPrefabAsset(root, hudPath);
                Debug.Log("[Zombie War] Twin-stick aim installed on LandscapeHUD.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string FindPrefabPath(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == prefabName)
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static void AddShootingIcon(RectTransform handle)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ShootingIconPath);
            if (sprite == null)
            {
                Debug.LogError("[Zombie War] Layer Lab shooting icon was not found.");
                return;
            }

            GameObject iconObject = new("Shooting Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(handle, false);
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(76f, 76f);
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        private static Transform FindChild(Transform root, string childName)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }
    }
}
