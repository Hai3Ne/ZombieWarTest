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
using UnityEditor.Animations;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using ZombieWar.Audio;
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
        private const string AudioFolder = RootFolder + "/Audio/Weapons";
        private const string ZombieAudioFolder = RootFolder + "/Audio/Zombies";
        private const string LayerLabRoot = "Assets/Layer Lab/GUI Pro-SurvivalClean";
        private const string LayerLabPlayPrefabs = LayerLabRoot + "/Prefabs/Prefabs_Demo_Play";
        private const string LayerLabButtonPrefabs = LayerLabRoot + "/Prefabs/Prefabs_Component_Buttons";
        private const string LayerLabSliderPrefabs = LayerLabRoot + "/Prefabs/Prefabs_Component_Sliders";
        private const string LayerLabBackgrounds = LayerLabRoot + "/ResourcesData/Sprites/Demo/Demo_Backgound";
        private const string LayerLabIcons = LayerLabRoot + "/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128";
        private const string ColonelSourceFolder = "Assets/TopDownEngine/Demos/Colonel";
        private const string SoldierCharacterFolder = RootFolder + "/Characters/Soldier";
        private const string SoldierModelFolder = SoldierCharacterFolder + "/Model";
        private const string SoldierAnimationFolder = SoldierCharacterFolder + "/Animations";
        private const string SoldierModelPath = SoldierModelFolder + "/SoldierColonel.fbx";
        private const string ZombieSourceFolder = "Assets/ArtStore3D/Zombie";
        private const string ZombieCharacterFolder = RootFolder + "/Characters/Zombie";
        private const string ZombieModelFolder = ZombieCharacterFolder + "/Model";
        private const string ZombieAnimationFolder = ZombieCharacterFolder + "/Animations";
        private const string ZombieTextureFolder = ZombieCharacterFolder + "/Textures";
        private const string ZombieModelPath = ZombieModelFolder + "/Zombie.fbx";
        private const string ZombieAnimationRigPath = ZombieModelFolder + "/AnimationRig.fbx";
        private const string ZombieBaseMapPath = ZombieTextureFolder + "/Zombie_BaseMap.png";
        private const string StarterAnimationFolder = "Assets/Survivalist/StarterAssets/ThirdPersonController/Character/Animations";
        private const string StarterAvatarPath = "Assets/Survivalist/StarterAssets/ThirdPersonController/Character/Models/Armature.fbx";
        private const string LowPolyGunFolder = "Assets/Low Poly Guns/Models/Guns";
        private const string PostApocalypseGunAudio = "Assets/PostApocalypseGunsDemo";
        private const string ZombieVoiceSource = "Assets/Tybug Studios/Zombie Voice Pack - Free";
        private const string RifleFireAudioPath = AudioFolder + "/RifleFire.wav";
        private const string ShotgunFireAudioPath = AudioFolder + "/ShotgunFire.wav";

        [MenuItem("Zombie War/Open Level 01 _F8")]
        public static void OpenLevel01()
        {
            EditorSceneManager.OpenScene(SceneFolder + "/Level01.unity", OpenSceneMode.Single);
        }

        [MenuItem("Zombie War/Open Boot _F7")]
        public static void OpenBoot()
        {
            EditorSceneManager.OpenScene(SceneFolder + "/Boot.unity", OpenSceneMode.Single);
        }

        [MenuItem("Zombie War/Open Main Menu _F9")]
        public static void OpenMainMenu()
        {
            EditorSceneManager.OpenScene(SceneFolder + "/MainMenu.unity", OpenSceneMode.Single);
        }

        [MenuItem("Zombie War/Author Project Assets _F6")]
        public static void AuthorProjectAssets()
        {
            ConfigurePlayerSettings();
            EnsureFolders();
            EnsureIndependentSoldierAssets();
            EnsureIndependentZombieAssets();
            EnsureIndependentWeaponAudioAssets();
            EnsureIndependentZombieAudioAssets();
            EnsureTmpEssentials();

            Material ground = GetOrCreateMaterial("Ground", new Color(0.12f, 0.16f, 0.17f), "Universal Render Pipeline/Lit");
            Material obstacle = GetOrCreateMaterial("Obstacle", new Color(0.32f, 0.28f, 0.2f), "Universal Render Pipeline/Lit");
            Material soldierMaterial = GetOrCreateMaterial("Soldier", new Color(0.12f, 0.45f, 0.85f), "Universal Render Pipeline/Lit");
            Material zombieMaterial = GetOrCreateMaterial("Zombie", new Color(0.22f, 0.48f, 0.16f), "ZombieWar/ZombieDissolve");
            ConfigureZombieMaterial(zombieMaterial);
            Material projectileMaterial = GetOrCreateMaterial("Projectile", new Color(1f, 0.75f, 0.12f), "Universal Render Pipeline/Unlit");
            Material bombMaterial = GetOrCreateMaterial("Bomb", new Color(0.15f, 0.15f, 0.16f), "Universal Render Pipeline/Lit");

            WeaponConfig rifle = GetOrCreateAsset<WeaponConfig>("Rifle");
            rifle.Configure("ASSAULT RIFLE", 18f, 0.12f, 20f, 36f, 1, 1.5f, 0.08f, new Color(1f, 0.72f, 0.12f));
            WeaponConfig shotgun = GetOrCreateAsset<WeaponConfig>("Shotgun");
            shotgun.Configure("SHOTGUN", 14f, 0.62f, 11f, 30f, 7, 10f, 0.24f, new Color(1f, 0.28f, 0.08f));
            ConfigureWeaponPresentation(rifle, shotgun);
            EditorUtility.SetDirty(rifle);
            EditorUtility.SetDirty(shotgun);

            EnemyConfig regular = GetOrCreateAsset<EnemyConfig>("Zombie");
            regular.Configure(55f, 2.45f, 7f, 1.35f, 0.82f, false);
            EnemyConfig giant = GetOrCreateAsset<EnemyConfig>("GiantZombie");
            giant.Configure(900f, 1.5f, 24f, 2.4f, 1.6f, true);
            EditorUtility.SetDirty(regular);
            EditorUtility.SetDirty(giant);

            LevelConfig levelOne = GetOrCreateAsset<LevelConfig>("Level01");
            levelOne.Configure("CONTAINMENT YARD", 180f, 25, 100, 120, false, 120f);
            LevelConfig levelTwo = GetOrCreateAsset<LevelConfig>("Level02");
            levelTwo.Configure("BROKEN OVERPASS", 180f, 30, 120, 120, true, 120f);
            EditorUtility.SetDirty(levelOne);
            EditorUtility.SetDirty(levelTwo);
            AssetDatabase.SaveAssets();
            WeaponAudioCatalog weaponAudioCatalog = GetOrCreateWeaponAudioCatalog();
            ZombieAudioCatalog zombieAudioCatalog = GetOrCreateZombieAudioCatalog();

            Projectile projectilePrefab = CreateProjectilePrefab(projectileMaterial);
            BombProjectile bombPrefab = CreateBombPrefab(bombMaterial);
            ZombieAgent zombiePrefab = CreateZombiePrefab(zombieMaterial);
            EnemyPrefabCatalog enemyPrefabCatalog = GetOrCreateEnemyPrefabCatalog(zombiePrefab);
            SoldierController soldierPrefab = CreateSoldierPrefab(soldierMaterial, bombPrefab, weaponAudioCatalog);
            RuntimeHud hudPrefab = CreateHudPrefab();

            CreateBootScene();
            CreateMainMenuScene();
            CreateLevelScene(
                "Level01",
                false,
                ground,
                obstacle,
                soldierPrefab,
                enemyPrefabCatalog,
                zombieAudioCatalog,
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
                enemyPrefabCatalog,
                zombieAudioCatalog,
                projectilePrefab,
                hudPrefab,
                new[] { rifle, shotgun },
                regular,
                giant,
                levelTwo);

            SetBuildScenes();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(SceneFolder + "/Level01.unity", OpenSceneMode.Single);
            Debug.Log("[Zombie War] Authored prefabs, configs and scenes are ready. Runtime bootstrap is not used.");
        }

        [MenuItem("Zombie War/Build Android Development")]
        public static void BuildAndroidDevelopment()
        {
            AuthorProjectAssets();
            BuildAddressablesContent();
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

        [MenuItem("Zombie War/Build Addressables Content")]
        public static void BuildAddressablesContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new BuildFailedException($"Addressables build failed: {result.Error}");
            }
        }

        private static void ConfigurePlayerSettings()
        {
            RenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
            if (pipeline == null)
            {
                throw new FileNotFoundException("Mobile URP asset was not found in Assets/Settings.");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.companyName = "Two Sleepy Cats";
            PlayerSettings.productName = "Zombie War";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultScreenWidth = 2560;
            PlayerSettings.defaultScreenHeight = 1440;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.twosleepycats.zombiewar");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        private static void ConfigureWeaponPresentation(WeaponConfig rifle, WeaponConfig shotgun)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            AddressableAssetGroup group = settings.FindGroup("ZombieWar-Weapons");
            if (group == null)
            {
                group = settings.CreateGroup(
                    "ZombieWar-Weapons",
                    false,
                    false,
                    false,
                    null,
                    typeof(BundledAssetGroupSchema),
                    typeof(ContentUpdateGroupSchema));
            }

            BundledAssetGroupSchema bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

            string rifleIcon = ConfigureAddressableEntry(settings, group, LayerLabIcons + "/Icon_Gun_0.Png", "weapons/rifle/icon", "weapons");
            string rifleView = ConfigureAddressableEntry(settings, group, LowPolyGunFolder + "/assault1/assault1.fbx", "weapons/rifle/view", "weapons");
            string shotgunIcon = ConfigureAddressableEntry(settings, group, LayerLabIcons + "/Icon_Gun_1.Png", "weapons/shotgun/icon", "weapons");
            string shotgunView = ConfigureAddressableEntry(settings, group, LowPolyGunFolder + "/shotgun2/shotgun2.fbx", "weapons/shotgun/view", "weapons");

            rifle.ConfigurePresentation(new AssetReferenceSprite(rifleIcon), new AssetReferenceT<GameObject>(rifleView));
            shotgun.ConfigurePresentation(new AssetReferenceSprite(shotgunIcon), new AssetReferenceT<GameObject>(shotgunView));
            EditorUtility.SetDirty(settings);
        }

        private static string ConfigureAddressableEntry(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            string assetPath,
            string address,
            string label)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                throw new FileNotFoundException($"Addressable asset was not found: {assetPath}.");
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = address;
            entry.SetLabel(label, true, true);
            return guid;
        }

        private static void EnsureFolders()
        {
            EnsureFolder(RootFolder, "Prefabs");
            EnsureFolder(RootFolder, "Configs");
            EnsureFolder(RootFolder, "Materials");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Resources");
            EnsureFolder(RootFolder, "Audio");
            EnsureFolder(RootFolder + "/Audio", "Weapons");
            EnsureFolder(RootFolder + "/Audio", "Zombies");
            EnsureFolder(RootFolder, "Characters");
            EnsureFolder(RootFolder + "/Characters", "Soldier");
            EnsureFolder(SoldierCharacterFolder, "Model");
            EnsureFolder(SoldierCharacterFolder, "Animations");
            EnsureFolder(RootFolder + "/Characters", "Zombie");
            EnsureFolder(ZombieCharacterFolder, "Model");
            EnsureFolder(ZombieCharacterFolder, "Animations");
            EnsureFolder(ZombieCharacterFolder, "Textures");
        }

        private static void EnsureIndependentWeaponAudioAssets()
        {
            CopyAssetIfMissing(
                PostApocalypseGunAudio + "/AssaultRifles/AutoGun_3p_01.wav",
                RifleFireAudioPath);
            CopyAssetIfMissing(
                PostApocalypseGunAudio + "/Shotguns/JackHammer_3p_01.wav",
                ShotgunFireAudioPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void EnsureIndependentZombieAudioAssets()
        {
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Moan/zombie_moan_001.wav", ZombieAudioFolder + "/AmbientMoan.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Hiss/zombie_hiss_010.wav", ZombieAudioFolder + "/AmbientHiss.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Growl/zombie_growl_010.wav", ZombieAudioFolder + "/AmbientGrowl01.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Growl/zombie_growl_023.wav", ZombieAudioFolder + "/AmbientGrowl02.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Aggressive/zombie_agressive_039.wav", ZombieAudioFolder + "/AttackAggressive01.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Aggressive/zombie_agressive_044.wav", ZombieAudioFolder + "/AttackAggressive02.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Grunt/zombie_grunt_006.wav", ZombieAudioFolder + "/HitGrunt.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Death/zombie_death_004.wav", ZombieAudioFolder + "/Death01.wav");
            CopyAssetIfMissing(ZombieVoiceSource + "/Zombie Death/zombie_death_010.wav", ZombieAudioFolder + "/Death02.wav");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            string[] paths =
            {
                ZombieAudioFolder + "/AmbientMoan.wav",
                ZombieAudioFolder + "/AmbientHiss.wav",
                ZombieAudioFolder + "/AmbientGrowl01.wav",
                ZombieAudioFolder + "/AmbientGrowl02.wav",
                ZombieAudioFolder + "/AttackAggressive01.wav",
                ZombieAudioFolder + "/AttackAggressive02.wav",
                ZombieAudioFolder + "/HitGrunt.wav",
                ZombieAudioFolder + "/Death01.wav",
                ZombieAudioFolder + "/Death02.wav"
            };
            for (int i = 0; i < paths.Length; i++)
            {
                ConfigureZombieAudioImporter(paths[i]);
            }
        }

        private static void ConfigureZombieAudioImporter(string path)
        {
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
            {
                throw new FileNotFoundException($"Zombie audio importer was not found: {path}.");
            }

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            bool requiresReimport = !importer.forceToMono
                || settings.loadType != AudioClipLoadType.DecompressOnLoad
                || settings.compressionFormat != AudioCompressionFormat.Vorbis
                || !Mathf.Approximately(settings.quality, 0.7f)
                || settings.sampleRateSetting != AudioSampleRateSetting.OptimizeSampleRate
                || !settings.preloadAudioData;
            if (!requiresReimport)
            {
                return;
            }

            importer.forceToMono = true;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
            settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        private static WeaponAudioCatalog GetOrCreateWeaponAudioCatalog()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            AddressableAssetGroup group = settings.FindGroup("ZombieWar-Audio");
            if (group == null)
            {
                group = settings.CreateGroup(
                    "ZombieWar-Audio",
                    false,
                    false,
                    false,
                    null,
                    typeof(BundledAssetGroupSchema),
                    typeof(ContentUpdateGroupSchema));
            }

            BundledAssetGroupSchema bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

            string rifleGuid = ConfigureAddressableAudioEntry(
                settings,
                group,
                RifleFireAudioPath,
                "audio/weapons/rifle/fire",
                "audio-weapons");
            string shotgunGuid = ConfigureAddressableAudioEntry(
                settings,
                group,
                ShotgunFireAudioPath,
                "audio/weapons/shotgun/fire",
                "audio-weapons");

            WeaponAudioCatalog catalog = GetOrCreateAsset<WeaponAudioCatalog>("WeaponAudioCatalog");
            catalog.Configure(
                new AssetReferenceT<AudioClip>(rifleGuid),
                new AssetReferenceT<AudioClip>(shotgunGuid));
            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(settings);
            return catalog;
        }

        private static ZombieAudioCatalog GetOrCreateZombieAudioCatalog()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            AddressableAssetGroup group = settings.FindGroup("ZombieWar-ZombieAudio");
            if (group == null)
            {
                group = settings.CreateGroup(
                    "ZombieWar-ZombieAudio",
                    false,
                    false,
                    false,
                    null,
                    typeof(BundledAssetGroupSchema),
                    typeof(ContentUpdateGroupSchema));
            }

            BundledAssetGroupSchema bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

            string[] paths =
            {
                ZombieAudioFolder + "/AmbientMoan.wav",
                ZombieAudioFolder + "/AmbientHiss.wav",
                ZombieAudioFolder + "/AmbientGrowl01.wav",
                ZombieAudioFolder + "/AmbientGrowl02.wav",
                ZombieAudioFolder + "/AttackAggressive01.wav",
                ZombieAudioFolder + "/AttackAggressive02.wav",
                ZombieAudioFolder + "/HitGrunt.wav",
                ZombieAudioFolder + "/Death01.wav",
                ZombieAudioFolder + "/Death02.wav"
            };
            string[] addresses =
            {
                "audio/zombies/ambient/moan",
                "audio/zombies/ambient/hiss",
                "audio/zombies/ambient/growl-01",
                "audio/zombies/ambient/growl-02",
                "audio/zombies/attack/aggressive-01",
                "audio/zombies/attack/aggressive-02",
                "audio/zombies/hit/grunt",
                "audio/zombies/death/01",
                "audio/zombies/death/02"
            };
            for (int i = 0; i < paths.Length; i++)
            {
                ConfigureAddressableAudioEntry(settings, group, paths[i], addresses[i], "audio-zombies");
            }

            AssetLabelReference label = new() { labelString = "audio-zombies" };
            ZombieAudioCatalog catalog = GetOrCreateAsset<ZombieAudioCatalog>("ZombieAudioCatalog");
            catalog.Configure(label);
            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(settings);
            return catalog;
        }

        private static string ConfigureAddressableAudioEntry(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            string assetPath,
            string address,
            string label)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                throw new FileNotFoundException($"Addressable audio asset was not found: {assetPath}.");
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = address;
            entry.SetLabel(label, true, true);
            return guid;
        }

        private static EnemyPrefabCatalog GetOrCreateEnemyPrefabCatalog(ZombieAgent zombiePrefab)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            AddressableAssetGroup group = settings.FindGroup("ZombieWar-Enemies");
            if (group == null)
            {
                group = settings.CreateGroup(
                    "ZombieWar-Enemies",
                    false,
                    false,
                    false,
                    null,
                    typeof(BundledAssetGroupSchema),
                    typeof(ContentUpdateGroupSchema));
            }

            BundledAssetGroupSchema bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

            string prefabPath = AssetDatabase.GetAssetPath(zombiePrefab.gameObject);
            string guid = AssetDatabase.AssetPathToGUID(prefabPath);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = "prefabs/enemies/zombie";
            entry.SetLabel("enemies", true, true);

            EnemyPrefabCatalog catalog = GetOrCreateAsset<EnemyPrefabCatalog>("EnemyPrefabCatalog");
            catalog.Configure(new AssetReferenceT<GameObject>(guid));
            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(settings);
            return catalog;
        }

        private static void EnsureIndependentSoldierAssets()
        {
            CopyAssetIfMissing(ColonelSourceFolder + "/Models/FBI@T-Pose.fbx", SoldierModelPath);
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Various/FBI@RifleAimingIdle.fbx", SoldierAnimationFolder + "/RifleAimIdle.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Various/FBI@Gunplay.fbx", SoldierAnimationFolder + "/RifleFire.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Various/FBI@HitReaction.fbx", SoldierAnimationFolder + "/HitReaction.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunForward.fbx", SoldierAnimationFolder + "/RunForward.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunBackward.fbx", SoldierAnimationFolder + "/RunBackward.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunLeft.fbx", SoldierAnimationFolder + "/RunLeft.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunRight.fbx", SoldierAnimationFolder + "/RunRight.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunForwardLeft.fbx", SoldierAnimationFolder + "/RunForwardLeft.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunForwardRight.fbx", SoldierAnimationFolder + "/RunForwardRight.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunBackwardLeft.fbx", SoldierAnimationFolder + "/RunBackwardLeft.fbx");
            CopyAssetIfMissing(ColonelSourceFolder + "/Animations/Run/FBI@RunBackwardRight.fbx", SoldierAnimationFolder + "/RunBackwardRight.fbx");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureIndependentSoldierRig();
        }

        private static void ConfigureIndependentSoldierRig()
        {
            Avatar avatar = LoadAvatar(SoldierModelPath);
            string[] animationPaths =
            {
                SoldierAnimationFolder + "/RifleAimIdle.fbx",
                SoldierAnimationFolder + "/RifleFire.fbx",
                SoldierAnimationFolder + "/HitReaction.fbx",
                SoldierAnimationFolder + "/RunForward.fbx",
                SoldierAnimationFolder + "/RunBackward.fbx",
                SoldierAnimationFolder + "/RunLeft.fbx",
                SoldierAnimationFolder + "/RunRight.fbx",
                SoldierAnimationFolder + "/RunForwardLeft.fbx",
                SoldierAnimationFolder + "/RunForwardRight.fbx",
                SoldierAnimationFolder + "/RunBackwardLeft.fbx",
                SoldierAnimationFolder + "/RunBackwardRight.fbx"
            };

            for (int i = 0; i < animationPaths.Length; i++)
            {
                ModelImporter importer = AssetImporter.GetAtPath(animationPaths[i]) as ModelImporter;
                if (importer == null)
                {
                    throw new FileNotFoundException($"Soldier animation importer was not found: {animationPaths[i]}.");
                }
                if (importer.animationType != ModelImporterAnimationType.Human
                    || importer.avatarSetup != ModelImporterAvatarSetup.CopyFromOther
                    || importer.sourceAvatar != avatar)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                    importer.sourceAvatar = avatar;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void EnsureIndependentZombieAssets()
        {
            CopyAssetIfMissing(ZombieSourceFolder + "/Model/Zombie.fbx", ZombieModelPath);
            CopyAssetIfMissing(StarterAvatarPath, ZombieAnimationRigPath);
            CopyAssetIfMissing(ZombieSourceFolder + "/Texture/Zombie_BaseMap.png", ZombieBaseMapPath);
            CopyAssetIfMissing(StarterAnimationFolder + "/Stand--Idle.anim.fbx", ZombieAnimationFolder + "/Idle.fbx");
            CopyAssetIfMissing(StarterAnimationFolder + "/Locomotion--Walk_N.anim.fbx", ZombieAnimationFolder + "/Walk.fbx");
            CopyAssetIfMissing(StarterAnimationFolder + "/Locomotion--Run_N.anim.fbx", ZombieAnimationFolder + "/Run.fbx");
            CopyAssetIfMissing(SoldierAnimationFolder + "/HitReaction.fbx", ZombieAnimationFolder + "/HitReaction.fbx");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureIndependentZombieRig();
        }

        private static void ConfigureIndependentZombieRig()
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(ZombieModelPath) as ModelImporter;
            if (modelImporter == null)
            {
                throw new FileNotFoundException($"Zombie model importer was not found: {ZombieModelPath}.");
            }

            if (modelImporter.animationType != ModelImporterAnimationType.Human
                || modelImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel
                || modelImporter.importAnimation)
            {
                modelImporter.animationType = ModelImporterAnimationType.Human;
                modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                modelImporter.importAnimation = false;
                modelImporter.SaveAndReimport();
            }

            ModelImporter animationRigImporter = AssetImporter.GetAtPath(ZombieAnimationRigPath) as ModelImporter;
            if (animationRigImporter == null)
            {
                throw new FileNotFoundException($"Zombie animation source rig was not found: {ZombieAnimationRigPath}.");
            }
            if (animationRigImporter.animationType != ModelImporterAnimationType.Human
                || animationRigImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel
                || animationRigImporter.importAnimation)
            {
                animationRigImporter.animationType = ModelImporterAnimationType.Human;
                animationRigImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                animationRigImporter.importAnimation = false;
                animationRigImporter.SaveAndReimport();
            }

            Avatar starterAvatar = LoadAvatar(ZombieAnimationRigPath);
            Avatar soldierAvatar = LoadAvatar(SoldierModelPath);
            ConfigureZombieAnimationClip(ZombieAnimationFolder + "/Idle.fbx", starterAvatar, true);
            ConfigureZombieAnimationClip(ZombieAnimationFolder + "/Walk.fbx", starterAvatar, true);
            ConfigureZombieAnimationClip(ZombieAnimationFolder + "/Run.fbx", starterAvatar, true);
            ConfigureZombieAnimationClip(ZombieAnimationFolder + "/HitReaction.fbx", soldierAvatar, false);
        }

        private static void ConfigureZombieAnimationClip(string path, Avatar sourceAvatar, bool loopTime)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException($"Zombie animation importer was not found: {path}.");
            }

            bool requiresReimport = importer.animationType != ModelImporterAnimationType.Human
                || importer.avatarSetup != ModelImporterAvatarSetup.CopyFromOther
                || importer.sourceAvatar != sourceAvatar;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = sourceAvatar;

            ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0
                ? importer.clipAnimations
                : importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].loopTime != loopTime
                    || !clips[i].lockRootRotation
                    || !clips[i].lockRootHeightY
                    || !clips[i].lockRootPositionXZ)
                {
                    requiresReimport = true;
                }

                clips[i].loopTime = loopTime;
                clips[i].loopPose = loopTime;
                clips[i].lockRootRotation = true;
                clips[i].lockRootHeightY = true;
                clips[i].lockRootPositionXZ = true;
            }
            importer.clipAnimations = clips;

            if (requiresReimport)
            {
                importer.SaveAndReimport();
            }
        }

        private static Avatar LoadAvatar(string assetPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar)
                {
                    return avatar;
                }
            }
            throw new FileNotFoundException($"Humanoid avatar was not found at {assetPath}.");
        }

        private static void CopyAssetIfMissing(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(destinationPath) != null)
            {
                return;
            }
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null || !AssetDatabase.CopyAsset(sourcePath, destinationPath))
            {
                throw new FileNotFoundException($"Unable to duplicate project asset from {sourcePath}.");
            }
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

        private static void ConfigureZombieMaterial(Material material)
        {
            Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(ZombieBaseMapPath);
            if (baseMap == null)
            {
                throw new FileNotFoundException($"Zombie base map was not found: {ZombieBaseMapPath}.");
            }

            material.SetTexture("_BaseMap", baseMap);
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
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
            root.GetComponent<MeshRenderer>().enabled = false;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            root.AddComponent<Health>();
            ZombieVisualController visual = root.AddComponent<ZombieVisualController>();
            ZombieAnimationController animation = root.AddComponent<ZombieAnimationController>();
            root.AddComponent<ZombieAgent>();

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ZombieModelPath);
            if (modelPrefab == null)
            {
                throw new FileNotFoundException("The independent zombie model was not authored.");
            }

            GameObject visualRoot = new("Zombie Visual");
            visualRoot.transform.SetParent(root.transform, false);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            model.name = "Character Model";
            model.transform.SetParent(visualRoot.transform, false);
            model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                throw new MissingComponentException("The zombie model requires an Animator.");
            }
            animator.runtimeAnimatorController = CreateZombieAnimatorController();
            animator.applyRootMotion = false;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            NormalizeModelHeight(model.transform, renderers, 1.8f);
            CenterModelOnPhysicsRoot(model.transform, renderers);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
                renderers[i].shadowCastingMode = ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                if (renderers[i] is SkinnedMeshRenderer skinnedRenderer)
                {
                    skinnedRenderer.updateWhenOffscreen = false;
                    skinnedRenderer.quality = SkinQuality.Bone2;
                }
            }

            visual.SetRenderers(renderers);
            animation.SetAnimator(animator);
            return SavePrefab<ZombieAgent>(root, "Zombie");
        }

        private static RuntimeAnimatorController CreateZombieAnimatorController()
        {
            string controllerPath = ConfigFolder + "/ZombieAnimator.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            AnimationClip idle = LoadAnimationClip(ZombieAnimationFolder + "/Idle.fbx");
            AnimationClip hitReaction = LoadAnimationClip(ZombieAnimationFolder + "/HitReaction.fbx");
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            BlendTree locomotion = new()
            {
                name = "Zombie Locomotion",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "MoveSpeed",
                useAutomaticThresholds = false,
                children = new[]
                {
                    ThresholdMotion(idle, 0f, 1f),
                    ThresholdMotion(LoadAnimationClip(ZombieAnimationFolder + "/Walk.fbx"), 0.45f, 0.85f),
                    ThresholdMotion(LoadAnimationClip(ZombieAnimationFolder + "/Run.fbx"), 1f, 0.9f)
                }
            };
            AssetDatabase.AddObjectToAsset(locomotion, controller);

            AnimatorState locomotionState = machine.AddState("Locomotion");
            locomotionState.motion = locomotion;
            machine.defaultState = locomotionState;
            AnimatorState attackState = machine.AddState("Attack");
            attackState.motion = hitReaction;
            attackState.speed = 1.7f;
            AnimatorState hitState = machine.AddState("Hit");
            hitState.motion = hitReaction;
            hitState.speed = 1.2f;
            AnimatorState deadState = machine.AddState("Dead");

            AddTriggeredTransition(machine, attackState, "Attack", 0.03f);
            AddTriggeredTransition(machine, hitState, "Hit", 0.02f);
            AddExitTransition(attackState, locomotionState, 0.55f);
            AddExitTransition(hitState, locomotionState, 0.72f);
            AnimatorStateTransition deadTransition = machine.AddAnyStateTransition(deadState);
            deadTransition.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
            deadTransition.hasExitTime = false;
            deadTransition.duration = 0.05f;

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static ChildMotion ThresholdMotion(AnimationClip clip, float threshold, float timeScale)
        {
            return new ChildMotion
            {
                motion = clip,
                threshold = threshold,
                timeScale = timeScale
            };
        }

        private static void AddTriggeredTransition(
            AnimatorStateMachine machine,
            AnimatorState destination,
            string parameter,
            float duration)
        {
            AnimatorStateTransition transition = machine.AddAnyStateTransition(destination);
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
        }

        private static void AddExitTransition(AnimatorState source, AnimatorState destination, float exitTime)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = 0.08f;
        }

        private static SoldierController CreateSoldierPrefab(
            Material material,
            BombProjectile bombPrefab,
            WeaponAudioCatalog weaponAudioCatalog)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Soldier";
            root.tag = "Player";
            root.GetComponent<MeshRenderer>().enabled = false;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 1.2f;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            root.AddComponent<Health>();
            SoldierAnimationController animation = root.AddComponent<SoldierAnimationController>();
            root.AddComponent<SoldierController>();
            root.AddComponent<WeaponController>();
            SoldierWeaponVisualController weaponVisual = root.AddComponent<SoldierWeaponVisualController>();
            SoldierWeaponAudioController weaponAudioController = root.AddComponent<SoldierWeaponAudioController>();
            weaponAudioController.SetCatalog(weaponAudioCatalog);
            AudioSource weaponAudio = root.GetComponent<AudioSource>();
            weaponAudio.playOnAwake = false;
            weaponAudio.spatialBlend = 0.55f;
            weaponAudio.dopplerLevel = 0f;
            weaponAudio.volume = 0.72f;
            BombController bomb = root.AddComponent<BombController>();
            bomb.SetPrefab(bombPrefab, 4);
            Material aimMaterial = GetOrCreateMaterial("BombAim", new Color(0.2f, 0.95f, 0.55f, 0.9f), "Universal Render Pipeline/Unlit");
            LineRenderer trajectory = CreateBombPreviewLine(root.transform, "Bomb Trajectory", aimMaterial, 0.075f, false);
            LineRenderer rangeRing = CreateBombPreviewLine(root.transform, "Bomb Range", aimMaterial, 0.035f, true);
            bomb.SetPreviewReferences(trajectory, rangeRing);
            GameObject muzzle = new("Muzzle");
            muzzle.transform.SetParent(root.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 1.2f, 0.65f);

            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SoldierModelPath);
            if (modelPrefab == null)
            {
                throw new FileNotFoundException("The independent SoldierColonel model was not authored.");
            }

            GameObject visualRoot = new("Soldier Visual");
            visualRoot.transform.SetParent(root.transform, false);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            model.name = "Character Model";
            model.transform.SetParent(visualRoot.transform, false);
            model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Animator animator = model.GetComponent<Animator>();
            animator.runtimeAnimatorController = CreateSoldierAnimatorController();
            animator.applyRootMotion = false;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            NormalizeModelHeight(model.transform, renderers, 1.8f);
            CenterModelOnPhysicsRoot(model.transform, renderers);
            SoldierWeaponIkController weaponIk = model.AddComponent<SoldierWeaponIkController>();
            model.AddComponent<SoldierFootstepRelay>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
            animation.SetViewReferences(animator, visualRoot.transform, renderers, weaponIk);
            Material rifleWeaponMaterial = GetOrCreateWeaponMaterial(
                "SoldierRifle",
                LowPolyGunFolder + "/assault1/assault1_diffuse.png",
                LowPolyGunFolder + "/assault1/assault1_normal.png");
            Material shotgunWeaponMaterial = GetOrCreateWeaponMaterial(
                "SoldierShotgun",
                LowPolyGunFolder + "/shotgun2/shotgun2_diffuse.png",
                LowPolyGunFolder + "/shotgun2/shotgun2_normal.png");
            CreateWeaponMount(
                visualRoot.transform,
                "Assault Rifle",
                LowPolyGunFolder + "/assault1/assault1.fbx",
                new Vector3(0f, 0.34f, 0.32f),
                new Vector3(0.09f, -0.015f, -0.05f),
                new Vector3(-0.09f, -0.015f, 0.22f),
                0.86f,
                rifleWeaponMaterial,
                out GameObject rifleModel,
                out Transform rifleRightGrip,
                out Transform rifleLeftGrip,
                out Transform rifleMuzzle);
            CreateWeaponMount(
                visualRoot.transform,
                "Shotgun",
                LowPolyGunFolder + "/shotgun2/shotgun2.fbx",
                new Vector3(0f, 0.32f, 0.34f),
                new Vector3(0.09f, -0.02f, -0.06f),
                new Vector3(-0.09f, -0.015f, 0.27f),
                0.96f,
                shotgunWeaponMaterial,
                out GameObject shotgunModel,
                out Transform shotgunRightGrip,
                out Transform shotgunLeftGrip,
                out Transform shotgunMuzzle);
            weaponVisual.SetViewReferences(
                new[] { rifleModel, shotgunModel },
                new[] { rifleRightGrip, shotgunRightGrip },
                new[] { rifleLeftGrip, shotgunLeftGrip },
                new[] { rifleMuzzle, shotgunMuzzle },
                muzzle.transform,
                weaponIk);
            return SavePrefab<SoldierController>(root, "Soldier");
        }

        private static LineRenderer CreateBombPreviewLine(Transform parent, string name, Material material, float width, bool loop)
        {
            GameObject lineObject = new(name, typeof(LineRenderer));
            lineObject.transform.SetParent(parent, false);
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 3;
            line.numCornerVertices = 2;
            line.loop = loop;
            line.enabled = false;
            return line;
        }

        private static RuntimeAnimatorController CreateSoldierAnimatorController()
        {
            string controllerPath = ConfigFolder + "/SoldierAnimator.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            AnimationClip idle = LoadAnimationClip(SoldierAnimationFolder + "/RifleAimIdle.fbx");
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Fire", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine baseMachine = controller.layers[0].stateMachine;
            BlendTree locomotion = new()
            {
                name = "Locomotion",
                blendType = BlendTreeType.FreeformCartesian2D,
                blendParameter = "MoveX",
                blendParameterY = "MoveY",
                useAutomaticThresholds = false
            };
            locomotion.children = new[]
            {
                DirectionalMotion(idle, 0f, 0f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunForward.fbx"), 0f, 1f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunBackward.fbx"), 0f, -1f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunLeft.fbx"), -1f, 0f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunRight.fbx"), 1f, 0f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunForwardLeft.fbx"), -0.7f, 0.7f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunForwardRight.fbx"), 0.7f, 0.7f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunBackwardLeft.fbx"), -0.7f, -0.7f),
                DirectionalMotion(LoadAnimationClip(SoldierAnimationFolder + "/RunBackwardRight.fbx"), 0.7f, -0.7f)
            };
            AssetDatabase.AddObjectToAsset(locomotion, controller);
            AnimatorState locomotionState = baseMachine.AddState("Locomotion");
            locomotionState.motion = locomotion;
            baseMachine.defaultState = locomotionState;

            AvatarMask upperBodyMask = GetOrCreateUpperBodyMask();
            AnimatorStateMachine fireMachine = new() { name = "Upper Body Fire" };
            AssetDatabase.AddObjectToAsset(fireMachine, controller);
            AnimatorState readyState = fireMachine.AddState("Ready");
            AnimatorState fireState = fireMachine.AddState("Fire");
            fireState.motion = LoadAnimationClip(SoldierAnimationFolder + "/RifleFire.fbx");
            fireState.speed = 2.5f;
            fireMachine.defaultState = readyState;
            AnimatorStateTransition enterFire = fireMachine.AddAnyStateTransition(fireState);
            enterFire.AddCondition(AnimatorConditionMode.If, 0f, "Fire");
            enterFire.hasExitTime = false;
            enterFire.duration = 0.02f;
            enterFire.canTransitionToSelf = false;
            AnimatorStateTransition leaveFire = fireState.AddTransition(readyState);
            leaveFire.hasExitTime = true;
            leaveFire.exitTime = 0.12f;
            leaveFire.duration = 0.05f;
            controller.AddLayer(new AnimatorControllerLayer
            {
                name = "Upper Body Fire",
                defaultWeight = 1f,
                avatarMask = upperBodyMask,
                blendingMode = AnimatorLayerBlendingMode.Override,
                stateMachine = fireMachine
            });

            EnableAnimatorIk(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static ChildMotion DirectionalMotion(AnimationClip clip, float x, float y)
        {
            return new ChildMotion
            {
                motion = clip,
                position = new Vector2(x, y),
                timeScale = 1f
            };
        }

        private static void EnableAnimatorIk(AnimatorController controller)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].iKPass = true;
            }
            controller.layers = layers;
            EditorUtility.SetDirty(controller);
        }

        private static void CreateWeaponMount(
            Transform visualRoot,
            string weaponName,
            string modelPath,
            Vector3 mountPosition,
            Vector3 rightGripPosition,
            Vector3 leftGripPosition,
            float targetLength,
            Material weaponMaterial,
            out GameObject mount,
            out Transform rightGrip,
            out Transform leftGrip,
            out Transform muzzle)
        {
            mount = new GameObject(weaponName + " Mount");
            mount.transform.SetParent(visualRoot, false);
            mount.transform.localPosition = mountPosition;

            GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (weaponPrefab == null)
            {
                throw new FileNotFoundException($"Weapon model was not found: {modelPath}");
            }

            GameObject weaponModel = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
            weaponModel.name = weaponName + " Model";
            weaponModel.transform.SetParent(mount.transform, false);
            AlignWeaponModel(weaponModel.transform, targetLength);
            Renderer[] weaponRenderers = weaponModel.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < weaponRenderers.Length; i++)
            {
                weaponRenderers[i].sharedMaterial = weaponMaterial;
            }

            rightGrip = CreateWeaponPoint(mount.transform, "Right Hand Grip", rightGripPosition);
            leftGrip = CreateWeaponPoint(mount.transform, "Left Hand Grip", leftGripPosition);
            muzzle = CreateWeaponPoint(mount.transform, "Muzzle", new Vector3(0f, 0f, targetLength * 0.56f));
            rightGrip.localRotation = Quaternion.Euler(0f, 90f, 75f);
            leftGrip.localRotation = Quaternion.Euler(0f, 90f, 85f);
        }

        private static Transform CreateWeaponPoint(Transform parent, string name, Vector3 localPosition)
        {
            Transform point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.localPosition = localPosition;
            return point;
        }

        private static void AlignWeaponModel(Transform weapon, float targetLength)
        {
            Renderer[] renderers = weapon.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (bounds.size.x > bounds.size.z)
            {
                weapon.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            float currentLength = Mathf.Max(bounds.size.x, bounds.size.z);
            weapon.localScale = Vector3.one * (targetLength / Mathf.Max(0.001f, currentLength));

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            weapon.position += weapon.parent.position - bounds.center;
        }

        private static Material GetOrCreateWeaponMaterial(string name, string diffusePath, string normalPath)
        {
            string materialPath = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            material.SetFloat("_BumpScale", 0.7f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static AvatarMask GetOrCreateUpperBodyMask()
        {
            string maskPath = ConfigFolder + "/SoldierUpperBody.mask";
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            if (mask != null)
            {
                return mask;
            }

            mask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            }
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            AssetDatabase.CreateAsset(mask, maskPath);
            return mask;
        }

        private static AnimationClip LoadAnimationClip(string assetPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    return clip;
                }
            }
            throw new FileNotFoundException($"Animation clip was not found at {assetPath}.");
        }

        private static void CenterModelOnPhysicsRoot(Transform model, Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i].gameObject.activeInHierarchy)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            Vector3 offset = new(-bounds.center.x, -1f - bounds.min.y, -bounds.center.z);
            model.position += offset;
        }

        private static void NormalizeModelHeight(Transform model, Renderer[] renderers, float targetHeight)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float scale = targetHeight / Mathf.Max(0.001f, bounds.size.y);
            model.localScale *= scale;
        }

        private static RuntimeHud CreateHudPrefab()
        {
            GameObject root = new("Landscape HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(RuntimeHud), typeof(WeaponRadialMenu));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560f, 1440f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeObject = new("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
            safeObject.transform.SetParent(root.transform, false);
            RectTransform safe = safeObject.GetComponent<RectTransform>();
            Stretch(safe, Vector2.zero, Vector2.one);

            GameObject healthObject = InstantiateLayerLabPrefab(LayerLabSliderPrefabs + "/Slider06_Red.prefab", safe, "Health");
            ConfigureAnchoredPrefab(healthObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(285f, -86f), 0.82f);
            Slider healthSlider = healthObject.GetComponent<Slider>();
            healthSlider.interactable = false;
            Image healthFill = healthSlider.fillRect.GetComponent<Image>();
            TMP_Text healthValue = FindChildComponent<TMP_Text>(healthObject.transform, "Text_Value");
            healthValue.gameObject.SetActive(false);
            TMP_Text healthLabel = FindChildComponent<TMP_Text>(healthObject.transform, "Text");
            healthLabel.text = "HP";

            GameObject timerObject = InstantiateLayerLabPrefab(LayerLabPlayPrefabs + "/Play_Time.prefab", safe, "Mission Timer");
            ConfigureAnchoredPrefab(timerObject.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -72f), 0.66f);
            SetChildActive(timerObject.transform, "Score 1", false);
            SetChildActive(timerObject.transform, "Score 2", false);
            TextMeshProUGUI timer = FindChildComponent<TextMeshProUGUI>(timerObject.transform, "Text_Value");
            timer.text = "03:00";

            TextMeshProUGUI weapon = CreateText(safe, "ASSAULT RIFLE", 34, TextAlignmentOptions.MidlineRight, new Vector2(0.74f, 0.91f), new Vector2(0.96f, 0.965f));
            TextMeshProUGUI crowd = CreateText(safe, "THREAT  000", 28, TextAlignmentOptions.Center, new Vector2(0.42f, 0.875f), new Vector2(0.58f, 0.92f));
            weapon.font = timer.font;
            crowd.font = timer.font;

            Image joystickZoneImage = CreateImage(
                safe,
                "Joystick Input Zone",
                Color.clear,
                Vector2.zero,
                new Vector2(0.48f, 0.62f));
            joystickZoneImage.raycastTarget = true;
            RectTransform joystickZone = joystickZoneImage.rectTransform;

            GameObject joystickObject = InstantiateLayerLabPrefab(
                LayerLabPlayPrefabs + "/Play_Joystick_Direction.prefab",
                joystickZone,
                "Movement Joystick");
            RectTransform joystickRect = joystickObject.GetComponent<RectTransform>();
            ConfigureAnchoredPrefab(joystickRect, new Vector2(0.5f, 0.5f), Vector2.zero, 0.82f);
            RectTransform joystickHandle = FindChildComponent<RectTransform>(joystickObject.transform, "Handle");
            joystickHandle.anchoredPosition = Vector2.zero;
            Graphic[] joystickGraphics = joystickObject.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < joystickGraphics.Length; i++)
            {
                joystickGraphics[i].raycastTarget = false;
            }
            VirtualJoystick joystick = joystickZoneImage.gameObject.AddComponent<VirtualJoystick>();
            joystick.Configure(joystickZone, joystickRect, joystickHandle);

            Button switchButton = CreateLayerLabButton(
                safe,
                "Switch Weapon",
                "SWITCH",
                LayerLabButtonPrefabs + "/Btn_IconTextButton_Square07_Blue.prefab",
                LayerLabIcons + "/Icon_Change.Png",
                new Vector2(1f, 0.5f),
                new Vector2(-175f, 185f),
                0.58f);

            GameObject bombJoystickObject = InstantiateLayerLabPrefab(LayerLabPlayPrefabs + "/Play_Joystick_Direction.prefab", safe, "Bomb Aim Joystick");
            RectTransform bombJoystickRect = bombJoystickObject.GetComponent<RectTransform>();
            ConfigureAnchoredPrefab(bombJoystickRect, new Vector2(1f, 0f), new Vector2(-245f, 235f), 0.68f);
            RectTransform bombHandle = FindChildComponent<RectTransform>(bombJoystickObject.transform, "Handle");
            Image bombInput = CreateImage(bombJoystickRect, "Bomb Input", Color.clear, Vector2.zero, Vector2.one);
            bombInput.raycastTarget = true;
            BombAimJoystick bombJoystick = bombInput.gameObject.AddComponent<BombAimJoystick>();
            bombJoystick.Configure(bombJoystickRect, bombHandle);
            Image bombHandleIcon = bombHandle.GetComponent<Image>();
            bombHandleIcon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LayerLabIcons + "/Icon_Grenade_1.Png");
            bombHandleIcon.preserveAspect = true;
            Image bombFill = CreateImage(bombJoystickRect, "Cooldown", new Color(0.01f, 0.02f, 0.04f, 0.72f), Vector2.zero, Vector2.one);
            bombFill.raycastTarget = false;
            bombFill.type = Image.Type.Filled;
            bombFill.fillMethod = Image.FillMethod.Radial360;

            GameObject slotsRoot = new("Weapon Slots", typeof(RectTransform));
            slotsRoot.transform.SetParent(safe, false);
            Stretch(slotsRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            Button[] slotButtons = new Button[3];
            Image[] slotIcons = new Image[3];
            Vector2[] slotOffsets = { new(-330f, 325f), new(-400f, 185f), new(-330f, 45f) };
            for (int i = 0; i < slotButtons.Length; i++)
            {
                slotButtons[i] = CreateLayerLabButton(
                    slotsRoot.GetComponent<RectTransform>(),
                    $"Weapon Slot {i + 1}",
                    string.Empty,
                    LayerLabButtonPrefabs + "/Btn_IconTextButton_Square07_Blue.prefab",
                    LayerLabIcons + "/Icon_Gun_3.Png",
                    new Vector2(1f, 0.5f),
                    slotOffsets[i],
                    0.43f);
                slotIcons[i] = FindChildComponent<Image>(slotButtons[i].transform, "Icon");
            }
            WeaponRadialMenu weaponMenu = root.GetComponent<WeaponRadialMenu>();
            weaponMenu.SetViewReferences(switchButton, slotsRoot, slotButtons, slotIcons);

            Image panel = CreateImage(safe, "Result", new Color(0.025f, 0.055f, 0.095f, 0.97f), new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.71f));
            TextMeshProUGUI resultText = CreateText(panel.rectTransform, "AREA SECURED", 62, TextAlignmentOptions.Center, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.92f));
            resultText.font = timer.font;
            Button retry = CreateLayerLabButton(panel.rectTransform, "Retry", "RETRY", LayerLabButtonPrefabs + "/Btn_IconTextButton_Square07_Blue.prefab", null, new Vector2(0.5f, 0.38f), Vector2.zero, 0.85f);
            Button next = CreateLayerLabButton(panel.rectTransform, "Next", "NEXT AREA", LayerLabButtonPrefabs + "/Btn_IconTextButton_Square07_Green.prefab", null, new Vector2(0.5f, 0.14f), Vector2.zero, 0.85f);
            panel.gameObject.SetActive(false);

            RuntimeHud hud = root.GetComponent<RuntimeHud>();
            hud.SetViewReferences(joystick, bombJoystick, weaponMenu, healthFill, bombFill, timer, weapon, crowd, panel.gameObject, resultText, retry, next);
            return SavePrefab<RuntimeHud>(root, "LandscapeHUD");
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
            CreateUiCamera();
            BootSceneController controller = new GameObject("Boot Scene Controller", typeof(BootSceneController)).GetComponent<BootSceneController>();

            GameObject canvasObject = CreateLandscapeCanvas("Loading Canvas");
            RectTransform root = canvasObject.GetComponent<RectTransform>();
            CreateLayerLabBackground(root, LayerLabBackgrounds + "/Background_02.png", new Color(0.65f, 0.78f, 0.95f, 1f));
            Image shade = CreateImage(root, "Landscape Shade", new Color(0.015f, 0.035f, 0.075f, 0.35f), Vector2.zero, Vector2.one);
            shade.raycastTarget = false;

            TextMeshProUGUI title = CreateText(root, "ZOMBIE WAR", 86, TextAlignmentOptions.Center, new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.76f));
            title.color = new Color(0.55f, 0.92f, 1f);
            TextMeshProUGUI subtitle = CreateText(root, "PREPARING THE SAFE ZONE", 30, TextAlignmentOptions.Center, new Vector2(0.08f, 0.57f), new Vector2(0.92f, 0.63f));
            subtitle.color = new Color(0.7f, 0.82f, 0.9f);

            GameObject loadingObject = InstantiateLayerLabPrefab(LayerLabSliderPrefabs + "/Slider06_Blue.prefab", root, "Loading Progress");
            ConfigureAnchoredPrefab(loadingObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.18f), Vector2.zero, 1.85f);
            Slider slider = loadingObject.GetComponent<Slider>();
            slider.interactable = false;
            Image fill = slider.fillRect.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            TMP_Text loadingText = FindChildComponent<TMP_Text>(loadingObject.transform, "Text_Value");
            loadingText.text = "LOADING... 00%";
            TMP_Text label = FindChildComponent<TMP_Text>(loadingObject.transform, "Text");
            label.gameObject.SetActive(false);
            controller.SetViewReferences(fill, loadingText);
            EditorSceneManager.SaveScene(scene, SceneFolder + "/Boot.unity");
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";
            CreateUiCamera();
            MainMenuController controller = new GameObject("Main Menu Controller", typeof(MainMenuController)).GetComponent<MainMenuController>();
            CreateEventSystem();

            GameObject canvasObject = CreateLandscapeCanvas("Menu Canvas");
            RectTransform root = canvasObject.GetComponent<RectTransform>();

            CreateLayerLabBackground(root, LayerLabBackgrounds + "/Background_01.png", Color.white);
            Image shade = CreateImage(root, "Menu Shade", new Color(0.01f, 0.02f, 0.04f, 0.38f), Vector2.zero, Vector2.one);
            shade.raycastTarget = false;
            TextMeshProUGUI title = CreateText(root, "ZOMBIE\nWAR", 112, TextAlignmentOptions.Center, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.88f));
            title.color = new Color(0.55f, 0.94f, 1f);
            TextMeshProUGUI subtitle = CreateText(root, "SURVIVE THE SWARM", 34, TextAlignmentOptions.Center, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.65f));
            subtitle.color = new Color(0.78f, 0.86f, 0.92f);
            Button start = CreateLayerLabButton(root, "Start Level 1", "START", LayerLabButtonPrefabs + "/Btn_IconTextButton_Square07_Green.prefab", null, new Vector2(0.5f, 0.36f), Vector2.zero, 1.35f);
            Button levelTwo = CreateLayerLabButton(root, "Start Level 2", "LEVEL 2", LayerLabButtonPrefabs + "/Btn_IconTextButton_Square07_Blue.prefab", null, new Vector2(0.5f, 0.24f), Vector2.zero, 1.15f);
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
            EnemyPrefabCatalog enemyPrefabs,
            ZombieAudioCatalog zombieAudio,
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
            enemyPrefabs = AssetDatabase.LoadAssetAtPath<EnemyPrefabCatalog>(ConfigFolder + "/EnemyPrefabCatalog.asset");
            zombieAudio = AssetDatabase.LoadAssetAtPath<ZombieAudioCatalog>(ConfigFolder + "/ZombieAudioCatalog.asset");
            projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Projectile.prefab").GetComponent<Projectile>();
            hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/LandscapeHUD.prefab").GetComponent<RuntimeHud>();
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
            enemyPrefabs = AssetDatabase.LoadAssetAtPath<EnemyPrefabCatalog>(ConfigFolder + "/EnemyPrefabCatalog.asset");
            zombieAudio = AssetDatabase.LoadAssetAtPath<ZombieAudioCatalog>(ConfigFolder + "/ZombieAudioCatalog.asset");
            projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Projectile.prefab").GetComponent<Projectile>();
            hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/LandscapeHUD.prefab").GetComponent<RuntimeHud>();
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
            enemyPool.SetCatalog(enemyPrefabs, 130);
            ZombieAudioService zombieAudioService = enemyPool.GetComponent<ZombieAudioService>();
            zombieAudioService.SetReferences(zombieAudio, CreateZombieAudioSources(enemyPool.transform, 8));
            EnemySimulationScheduler scheduler = systems.AddComponent<EnemySimulationScheduler>();
            WaveDirector wave = systems.AddComponent<WaveDirector>();
            GameSessionController session = systems.AddComponent<GameSessionController>();
            LevelSceneController controller = systems.AddComponent<LevelSceneController>();

            RuntimeHud hud = ((GameObject)PrefabUtility.InstantiatePrefab(hudPrefab.gameObject)).GetComponent<RuntimeHud>();
            controller.SetReferences(weapons, regular, giant, level, soldier, weapon, bomb, muzzle, projectilePool, enemyPool, scheduler, wave, session, hud, worldCamera);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/{sceneName}.unity");
        }

        private static AudioSource[] CreateZombieAudioSources(Transform parent, int capacity)
        {
            AudioSource[] sources = new AudioSource[capacity];
            for (int i = 0; i < capacity; i++)
            {
                GameObject sourceObject = new($"Zombie Voice {i + 1:00}");
                sourceObject.transform.SetParent(parent, false);
                AudioSource source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0.9f;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                source.minDistance = 1.5f;
                source.maxDistance = 22f;
                source.priority = 80;
                source.volume = 0.72f;
                sources[i] = source;
            }
            return sources;
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

            GameObject virtualCameraObject = new(
                "Landscape Top-Down Camera",
                typeof(CinemachineCamera),
                typeof(CinemachineFollow),
                typeof(CinemachineHardLookAt));
            CinemachineCamera virtualCamera = virtualCameraObject.GetComponent<CinemachineCamera>();
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
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

        private static GameObject CreateLandscapeCanvas(string name)
        {
            GameObject canvasObject = new(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560f, 1440f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        private static void CreateUiCamera()
        {
            GameObject cameraObject = new("UI Camera", typeof(Camera), typeof(AudioListener));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.012f, 0.02f, 1f);
            camera.cullingMask = 0;
        }

        private static GameObject InstantiateLayerLabPrefab(string path, Transform parent, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new FileNotFoundException($"Layer Lab prefab was not found: {path}");
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static Button CreateLayerLabButton(
            RectTransform parent,
            string name,
            string label,
            string prefabPath,
            string iconPath,
            Vector2 anchor,
            Vector2 anchoredPosition,
            float scale)
        {
            GameObject instance = InstantiateLayerLabPrefab(prefabPath, parent, name);
            ConfigureAnchoredPrefab(instance.GetComponent<RectTransform>(), anchor, anchoredPosition, scale);
            Button button = instance.GetComponent<Button>();
            TMP_Text text = FindChildComponent<TMP_Text>(instance.transform, "Text (TMP)");
            text.text = label;
            Transform counter = FindChild(instance.transform, "Text");
            if (counter != null)
            {
                counter.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(iconPath))
            {
                Image icon = FindChildComponent<Image>(instance.transform, "Icon");
                icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                icon.preserveAspect = true;
            }
            return button;
        }

        private static void CreateLayerLabBackground(RectTransform parent, string spritePath, Color color)
        {
            Image background = CreateImage(parent, "Layer Lab Background", color, Vector2.zero, Vector2.one);
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            background.preserveAspect = false;
            background.raycastTarget = false;
        }

        private static void ConfigureAnchoredPrefab(RectTransform rect, Vector2 anchor, Vector2 anchoredPosition, float scale)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one * scale;
        }

        private static T FindChildComponent<T>(Transform root, string name) where T : Component
        {
            Transform child = FindChild(root, name);
            if (child == null || !child.TryGetComponent(out T component))
            {
                throw new MissingComponentException($"{root.name} requires a child named '{name}' with {typeof(T).Name}.");
            }
            return component;
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

        private static void SetChildActive(Transform root, string name, bool active)
        {
            Transform child = FindChild(root, name);
            if (child != null)
            {
                child.gameObject.SetActive(active);
            }
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
