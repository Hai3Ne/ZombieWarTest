using System.IO;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ZombieWar.Combat;
using ZombieWar.Core;
using ZombieWar.Enemies;
using ZombieWar.Levels;
using ZombieWar.Player;
using ZombieWar.UI;

namespace ZombieWar.Editor
{
    public static class ZombieWarProjectSetup
    {
        private const string RootFolder = "Assets/_ZombieWar";
        private const string SceneFolder = RootFolder + "/Scenes";
        private const string PrefabFolder = RootFolder + "/Prefabs";
        private const string ConfigFolder = RootFolder + "/Configs";
        private const string MaterialFolder = RootFolder + "/Materials";

        [MenuItem("Zombie War/Author Project Assets")]
        public static void AuthorProjectAssets()
        {
            ConfigurePlayerSettings();
            EnsureFolders();
            EnsureTmpEssentials();

            Material ground = GetOrCreateMaterial("Ground", new Color(0.12f, 0.16f, 0.17f), "Universal Render Pipeline/Lit");
            Material obstacle = GetOrCreateMaterial("Obstacle", new Color(0.32f, 0.28f, 0.2f), "Universal Render Pipeline/Lit");
            Material soldierMaterial = GetOrCreateMaterial("Soldier", new Color(0.12f, 0.45f, 0.85f), "Universal Render Pipeline/Lit");
            Material zombieMaterial = GetOrCreateMaterial("Zombie", new Color(0.22f, 0.48f, 0.16f), "ZombieWar/ZombieDissolve");
            Material projectileMaterial = GetOrCreateMaterial("Projectile", new Color(1f, 0.75f, 0.12f), "Universal Render Pipeline/Unlit");
            Material bombMaterial = GetOrCreateMaterial("Bomb", new Color(0.15f, 0.15f, 0.16f), "Universal Render Pipeline/Lit");

            WeaponConfig rifle = GetOrCreateAsset<WeaponConfig>("Rifle");
            rifle.Configure("ASSAULT RIFLE", 18f, 0.12f, 20f, 36f, 1, 1.5f, 0.08f, new Color(1f, 0.72f, 0.12f));
            WeaponConfig shotgun = GetOrCreateAsset<WeaponConfig>("Shotgun");
            shotgun.Configure("SHOTGUN", 14f, 0.62f, 11f, 30f, 7, 10f, 0.24f, new Color(1f, 0.28f, 0.08f));

            EnemyConfig regular = GetOrCreateAsset<EnemyConfig>("Zombie");
            regular.Configure(55f, 2.45f, 7f, 1.35f, 0.82f, false);
            EnemyConfig giant = GetOrCreateAsset<EnemyConfig>("GiantZombie");
            giant.Configure(900f, 1.5f, 24f, 2.4f, 1.6f, true);

            LevelConfig levelOne = GetOrCreateAsset<LevelConfig>("Level01");
            levelOne.Configure("CONTAINMENT YARD", 180f, 25, 100, 120, false, 120f);
            LevelConfig levelTwo = GetOrCreateAsset<LevelConfig>("Level02");
            levelTwo.Configure("BROKEN OVERPASS", 180f, 30, 120, 120, true, 120f);

            Projectile projectilePrefab = CreateProjectilePrefab(projectileMaterial);
            BombProjectile bombPrefab = CreateBombPrefab(bombMaterial);
            ZombieAgent zombiePrefab = CreateZombiePrefab(zombieMaterial);
            SoldierController soldierPrefab = CreateSoldierPrefab(soldierMaterial, bombPrefab);
            RuntimeHud hudPrefab = CreateHudPrefab();

            CreateBootScene();
            CreateMainMenuScene();
            CreateLevelScene(
                "Level01",
                false,
                ground,
                obstacle,
                soldierPrefab,
                zombiePrefab,
                projectilePrefab,
                hudPrefab,
                new[] { rifle, shotgun },
                regular,
                giant,
                levelOne);
            CreateLevelScene(
                "Level02",
                true,
                ground,
                obstacle,
                soldierPrefab,
                zombiePrefab,
                projectilePrefab,
                hudPrefab,
                new[] { rifle, shotgun },
                regular,
                giant,
                levelTwo);

            SetBuildScenes();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(SceneFolder + "/Boot.unity", OpenSceneMode.Single);
            Debug.Log("[Zombie War] Authored prefabs, configs and scenes are ready. Runtime bootstrap is not used.");
        }

        [MenuItem("Zombie War/Build Android Development")]
        public static void BuildAndroidDevelopment()
        {
            AuthorProjectAssets();
            string outputDirectory = Path.GetFullPath("Builds/Android");
            Directory.CreateDirectory(outputDirectory);
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }

            BuildPlayerOptions options = new()
            {
                scenes = scenes,
                locationPathName = Path.Combine(outputDirectory, "ZombieWar-Development.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build failed: {report.summary.result}");
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Two Sleepy Cats";
            PlayerSettings.productName = "Zombie War";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.twosleepycats.zombiewar");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(RootFolder, "Prefabs");
            EnsureFolder(RootFolder, "Configs");
            EnsureFolder(RootFolder, "Materials");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Resources");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static T GetOrCreateAsset<T>(string name) where T : ScriptableObject
        {
            string path = $"{ConfigFolder}/{name}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material GetOrCreateMaterial(string name, Color color, string shaderName)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureTmpEssentials()
        {
            const string temporarySettings = RootFolder + "/Resources/TMP Settings.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(temporarySettings) != null)
            {
                AssetDatabase.DeleteAsset(temporarySettings);
            }

            const string settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(settingsPath) != null)
            {
                return;
            }

            UnityEditor.PackageManager.PackageInfo package =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly);
            if (package == null)
            {
                throw new BuildFailedException("Unable to locate the TextMeshPro package.");
            }

            string packagePath = Path.Combine(package.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static TMP_FontAsset GetProjectTmpFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null)
            {
                throw new BuildFailedException("TextMeshPro essential font asset was not imported.");
            }
            return font;
        }

        private static Projectile CreateProjectilePrefab(Material material)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "Projectile";
            root.transform.localScale = Vector3.one * 0.12f;
            Object.DestroyImmediate(root.GetComponent<Collider>());
            root.GetComponent<MeshRenderer>().sharedMaterial = material;
            root.AddComponent<Projectile>();
            Projectile result = SavePrefab<Projectile>(root, "Projectile");
            return result;
        }

        private static BombProjectile CreateBombPrefab(Material material)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = "Bomb";
            root.transform.localScale = Vector3.one * 0.36f;
            root.GetComponent<MeshRenderer>().sharedMaterial = material;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 1.2f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            root.AddComponent<BombProjectile>();
            return SavePrefab<BombProjectile>(root, "Bomb");
        }

        private static ZombieAgent CreateZombiePrefab(Material material)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Zombie";
            root.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            root.GetComponent<MeshRenderer>().sharedMaterial = material;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            root.AddComponent<Health>();
            root.AddComponent<ZombieAgent>();
            return SavePrefab<ZombieAgent>(root, "Zombie");
        }

        private static SoldierController CreateSoldierPrefab(Material material, BombProjectile bombPrefab)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Soldier";
            root.tag = "Player";
            root.GetComponent<MeshRenderer>().sharedMaterial = material;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 1.2f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            root.AddComponent<Health>();
            root.AddComponent<SoldierController>();
            root.AddComponent<WeaponController>();
            BombController bomb = root.AddComponent<BombController>();
            bomb.SetPrefab(bombPrefab, 4);
            GameObject muzzle = new("Muzzle");
            muzzle.transform.SetParent(root.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.9f, 0.9f);
            return SavePrefab<SoldierController>(root, "Soldier");
        }

        private static RuntimeHud CreateHudPrefab()
        {
            GameObject root = new("Portrait HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(RuntimeHud));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeObject = new("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
            safeObject.transform.SetParent(root.transform, false);
            RectTransform safe = safeObject.GetComponent<RectTransform>();
            Stretch(safe, Vector2.zero, Vector2.one);

            Image healthFill = CreateBar(safe, "Health", new Vector2(0.05f, 0.94f), new Vector2(0.42f, 0.975f), new Color(0.9f, 0.16f, 0.12f));
            TextMeshProUGUI timer = CreateText(safe, "03:00", 54, TextAlignmentOptions.Center, new Vector2(0.35f, 0.92f), new Vector2(0.65f, 0.99f));
            TextMeshProUGUI weapon = CreateText(safe, "ASSAULT RIFLE", 34, TextAlignmentOptions.MidlineRight, new Vector2(0.56f, 0.865f), new Vector2(0.95f, 0.92f));
            TextMeshProUGUI crowd = CreateText(safe, "THREAT  000", 28, TextAlignmentOptions.Center, new Vector2(0.33f, 0.855f), new Vector2(0.67f, 0.9f));

            Image joystickBackground = CreateImage(safe, "Joystick", new Color(0.05f, 0.08f, 0.12f, 0.58f), new Vector2(0.05f, 0.04f), new Vector2(0.42f, 0.25f));
            Image joystickHandle = CreateImage(joystickBackground.rectTransform, "Handle", new Color(0.25f, 0.65f, 1f, 0.9f), new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.7f));
            VirtualJoystick joystick = joystickBackground.gameObject.AddComponent<VirtualJoystick>();
            joystick.Configure(joystickBackground.rectTransform, joystickHandle.rectTransform);

            Button switchButton = CreateButton(safe, "SWITCH", new Vector2(0.68f, 0.16f), new Vector2(0.95f, 0.24f));
            Button bombButton = CreateButton(safe, "BOMB", new Vector2(0.72f, 0.045f), new Vector2(0.95f, 0.145f));
            Image bombFill = bombButton.GetComponent<Image>();
            bombFill.type = Image.Type.Filled;
            bombFill.fillMethod = Image.FillMethod.Radial360;

            Image panel = CreateImage(safe, "Result", new Color(0.025f, 0.035f, 0.055f, 0.94f), new Vector2(0.12f, 0.32f), new Vector2(0.88f, 0.68f));
            TextMeshProUGUI resultText = CreateText(panel.rectTransform, "AREA SECURED", 62, TextAlignmentOptions.Center, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.92f));
            Button retry = CreateButton(panel.rectTransform, "RETRY", new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.5f));
            Button next = CreateButton(panel.rectTransform, "NEXT AREA", new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.25f));
            panel.gameObject.SetActive(false);

            RuntimeHud hud = root.GetComponent<RuntimeHud>();
            hud.SetViewReferences(joystick, healthFill, bombFill, timer, weapon, crowd, panel.gameObject, resultText, switchButton, bombButton, retry, next);
            return SavePrefab<RuntimeHud>(root, "PortraitHUD");
        }

        private static T SavePrefab<T>(GameObject root, string name) where T : Component
        {
            string path = $"{PrefabFolder}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<T>();
        }

        private static void CreateBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Boot";
            new GameObject("Boot Scene Controller", typeof(BootSceneController));
            EditorSceneManager.SaveScene(scene, SceneFolder + "/Boot.unity");
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";
            MainMenuController controller = new GameObject("Main Menu Controller", typeof(MainMenuController)).GetComponent<MainMenuController>();
            CreateEventSystem();

            GameObject canvasObject = new("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform root = canvasObject.GetComponent<RectTransform>();

            CreateText(root, "ZOMBIE\nWAR", 118, TextAlignmentOptions.Center, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.9f));
            CreateText(root, "SURVIVE THE SWARM", 34, TextAlignmentOptions.Center, new Vector2(0.1f, 0.54f), new Vector2(0.9f, 0.62f));
            Button start = CreateButton(root, "START", new Vector2(0.14f, 0.3f), new Vector2(0.86f, 0.4f));
            Button levelTwo = CreateButton(root, "LEVEL 2", new Vector2(0.14f, 0.18f), new Vector2(0.86f, 0.28f));
            UnityEventTools.AddPersistentListener(start.onClick, controller.LoadLevelOne);
            UnityEventTools.AddPersistentListener(levelTwo.onClick, controller.LoadLevelTwo);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/MainMenu.unity");
        }

        private static void CreateLevelScene(
            string sceneName,
            bool levelTwo,
            Material ground,
            Material obstacle,
            SoldierController soldierPrefab,
            ZombieAgent zombiePrefab,
            Projectile projectilePrefab,
            RuntimeHud hudPrefab,
            WeaponConfig[] weapons,
            EnemyConfig regular,
            EnemyConfig giant,
            LevelConfig level)
        {
            ground = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Ground.mat");
            obstacle = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Obstacle.mat");
            soldierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Soldier.prefab").GetComponent<SoldierController>();
            zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Zombie.prefab").GetComponent<ZombieAgent>();
            projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Projectile.prefab").GetComponent<Projectile>();
            hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/PortraitHUD.prefab").GetComponent<RuntimeHud>();
            weapons = new[]
            {
                AssetDatabase.LoadAssetAtPath<WeaponConfig>(ConfigFolder + "/Rifle.asset"),
                AssetDatabase.LoadAssetAtPath<WeaponConfig>(ConfigFolder + "/Shotgun.asset")
            };
            regular = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigFolder + "/Zombie.asset");
            giant = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigFolder + "/GiantZombie.asset");
            level = AssetDatabase.LoadAssetAtPath<LevelConfig>($"{ConfigFolder}/{sceneName}.asset");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = sceneName;
            ground = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Ground.mat");
            obstacle = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/Obstacle.mat");
            soldierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Soldier.prefab").GetComponent<SoldierController>();
            zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Zombie.prefab").GetComponent<ZombieAgent>();
            projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Projectile.prefab").GetComponent<Projectile>();
            hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/PortraitHUD.prefab").GetComponent<RuntimeHud>();
            weapons = new[]
            {
                AssetDatabase.LoadAssetAtPath<WeaponConfig>(ConfigFolder + "/Rifle.asset"),
                AssetDatabase.LoadAssetAtPath<WeaponConfig>(ConfigFolder + "/Shotgun.asset")
            };
            regular = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigFolder + "/Zombie.asset");
            giant = AssetDatabase.LoadAssetAtPath<EnemyConfig>(ConfigFolder + "/GiantZombie.asset");
            level = AssetDatabase.LoadAssetAtPath<LevelConfig>($"{ConfigFolder}/{sceneName}.asset");
            CreateEventSystem();
            CreateDirectionalLight();

            GameObject arena = BuildArena(levelTwo, ground, obstacle);
            NavMeshSurface surface = arena.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.BuildNavMesh();

            SoldierController soldier = ((GameObject)PrefabUtility.InstantiatePrefab(soldierPrefab.gameObject)).GetComponent<SoldierController>();
            soldier.transform.position = Vector3.up;
            WeaponController weapon = soldier.GetComponent<WeaponController>();
            BombController bomb = soldier.GetComponent<BombController>();
            Transform muzzle = soldier.transform.Find("Muzzle");

            Camera worldCamera = CreateCamera(soldier.transform);

            GameObject systems = new("Gameplay Systems");
            ProjectilePool projectilePool = new GameObject("Projectile Pool", typeof(ProjectilePool)).GetComponent<ProjectilePool>();
            projectilePool.transform.SetParent(systems.transform);
            projectilePool.SetPrefab(projectilePrefab, 96);
            EnemyPool enemyPool = new GameObject("Enemy Pool", typeof(EnemyPool)).GetComponent<EnemyPool>();
            enemyPool.transform.SetParent(systems.transform);
            enemyPool.SetPrefab(zombiePrefab, 130);
            EnemySimulationScheduler scheduler = systems.AddComponent<EnemySimulationScheduler>();
            WaveDirector wave = systems.AddComponent<WaveDirector>();
            GameSessionController session = systems.AddComponent<GameSessionController>();
            LevelSceneController controller = systems.AddComponent<LevelSceneController>();

            RuntimeHud hud = ((GameObject)PrefabUtility.InstantiatePrefab(hudPrefab.gameObject)).GetComponent<RuntimeHud>();
            controller.SetReferences(weapons, regular, giant, level, soldier, weapon, bomb, muzzle, projectilePool, enemyPool, scheduler, wave, session, hud, worldCamera);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/{sceneName}.unity");
        }

        private static GameObject BuildArena(bool levelTwo, Material ground, Material obstacle)
        {
            GameObject arena = new("Environment");
            CreateCube(arena.transform, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(34f, 1f, 46f), ground);
            CreateCube(arena.transform, "North Wall", new Vector3(0f, 1f, 23f), new Vector3(35f, 3f, 1f), obstacle);
            CreateCube(arena.transform, "South Wall", new Vector3(0f, 1f, -23f), new Vector3(35f, 3f, 1f), obstacle);
            CreateCube(arena.transform, "East Wall", new Vector3(17f, 1f, 0f), new Vector3(1f, 3f, 47f), obstacle);
            CreateCube(arena.transform, "West Wall", new Vector3(-17f, 1f, 0f), new Vector3(1f, 3f, 47f), obstacle);
            Vector3[] positions =
            {
                new(-7f, 0.8f, -8f), new(7f, 0.8f, 8f), new(-8f, 0.8f, 10f),
                new(8f, 0.8f, -10f), new(0f, 0.8f, 14f), new(0f, 0.8f, -14f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                CreateCube(arena.transform, $"Barricade {i + 1}", positions[i], new Vector3(3f, 1.6f, 2f), obstacle);
            }
            if (levelTwo)
            {
                GameObject ramp = CreateCube(arena.transform, "Broken Ramp", new Vector3(0f, 1.3f, 4f), new Vector3(10f, 0.8f, 12f), obstacle);
                ramp.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            }
            return arena;
        }

        private static Camera CreateCamera(Transform target)
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CinemachineBrain));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120f;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.05f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            GameObject virtualCameraObject = new("Portrait Top-Down Camera", typeof(CinemachineCamera), typeof(CinemachineFollow));
            CinemachineCamera virtualCamera = virtualCameraObject.GetComponent<CinemachineCamera>();
            virtualCamera.Follow = target;
            virtualCamera.Lens.FieldOfView = 50f;
            CinemachineFollow follow = virtualCameraObject.GetComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 19f, -11f);
            TrackerSettings settings = follow.TrackerSettings;
            settings.BindingMode = BindingMode.WorldSpace;
            settings.PositionDamping = new Vector3(0.22f, 0.22f, 0.22f);
            follow.TrackerSettings = settings;
            return camera;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightObject = new("Directional Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions == null)
            {
                Debug.LogError("[Zombie War] Missing Assets/InputSystem_Actions.inputactions.", eventSystem);
                return;
            }
            inputModule.actionsAsset = actions;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.localScale = scale;
            instance.GetComponent<MeshRenderer>().sharedMaterial = material;
            return instance;
        }

        private static Image CreateBar(RectTransform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            Image background = CreateImage(parent, name + " BG", new Color(0.03f, 0.04f, 0.06f, 0.85f), min, max);
            Image fill = CreateImage(background.rectTransform, name + " Fill", color, Vector2.zero, Vector2.one);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            return fill;
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 min, Vector2 max)
        {
            Image image = CreateImage(parent, label, new Color(0.92f, 0.32f, 0.08f, 0.9f), min, max);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateText(image.rectTransform, label, 36, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            return button;
        }

        private static Image CreateImage(RectTransform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            GameObject instance = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            instance.transform.SetParent(parent, false);
            RectTransform rect = instance.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            Image image = instance.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string value, int size, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            GameObject instance = new("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            instance.transform.SetParent(parent, false);
            RectTransform rect = instance.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            TextMeshProUGUI text = instance.GetComponent<TextMeshProUGUI>();
            text.font = GetProjectTmpFont();
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetBuildScenes()
        {
            string[] names = { "Boot", "MainMenu", "Level01", "Level02" };
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene($"{SceneFolder}/{names[i]}.unity", true);
            }
            EditorBuildSettings.scenes = scenes;
        }
    }
}
