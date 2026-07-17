using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using ZombieWar.Audio;
using ZombieWar.Combat;

namespace ZombieWar.Editor.Polish
{
    public static class BombPolishInstaller
    {
        private const string MenuPath = "Zombie War/Polish/Install Soldier Rotation And Bomb VFX";
        private const string SoldierPrefabPath = "Assets/_ZombieWar/Prefabs/Soldier.prefab";
        private const string CirclePrefabPath = "Assets/Matthew Guz/Select Character/Prefab/Circle Select.prefab";
        private const string ExplosionAudioSourcePath = "Assets/PostApocalypseGunsDemo/Z-Extrem/HeavyLaserLauncher1.wav";
        private const string ExplosionAudioPath = "Assets/_ZombieWar/Audio/Bombs/BombExplosion.wav";
        private const string AudioConfigFolder = "Assets/_ZombieWar/Configs/Audio/Bombs";
        private const string AudioCatalogPath = AudioConfigFolder + "/BombAudioCatalog.asset";
        private const string ExplosionVfxPrefabPath = "Assets/_ZombieWar/Prefabs/VFX/BombExplosion.prefab";
        private const string ExplosionVfxSourcePrefabPath = "Assets/JMO Assets/WarFX/_Effects (Mobile)/Explosions/WFXMR_Explosion Small.prefab";

        [MenuItem(MenuPath)]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Zombie War] Stop Play Mode before installing bomb polish.");
                return;
            }

            ValidateAssets();
            EnsureFolders();
            EnsureExplosionAudio();
            BombAudioCatalog catalog = ConfigureAudioAddressable();
            ConfigureExplosionVfxMaterials();
            ConfigureSoldierPrefab(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Zombie War] Soldier rotation isolation, bomb target VFX, trajectory polish and explosion audio installed.");
        }

        private static void ConfigureSoldierPrefab(BombAudioCatalog catalog)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SoldierPrefabPath);

            try
            {
                Rigidbody body = root.GetComponent<Rigidbody>();
                WeaponController weapon = root.GetComponent<WeaponController>();
                BombController bomb = root.GetComponent<BombController>();
                if (body == null || weapon == null || bomb == null)
                {
                    throw new MissingComponentException("Soldier prefab is missing Rigidbody, WeaponController or BombController.");
                }

                body.constraints = RigidbodyConstraints.FreezeRotation;
                Transform facingRoot = EnsureFacingRoot(root.transform);
                MoveUnderFacingRoot(root.transform, facingRoot, "Muzzle");
                MoveUnderFacingRoot(root.transform, facingRoot, "Soldier Visual");
                weapon.SetFacingRoot(facingRoot);

                GameObject targetIndicator = CreateTargetIndicator(root);
                bomb.SetTargetIndicator(targetIndicator);
                ConfigureTrajectory(root.transform);
                ConfigureExplosionAudio(root.transform, catalog, bomb);
                PrefabUtility.SaveAsPrefabAsset(root, SoldierPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureExplosionVfxMaterials()
        {
            GameObject sourceRoot = AssetDatabase.LoadAssetAtPath<GameObject>(ExplosionVfxSourcePrefabPath);
            if (sourceRoot == null)
            {
                throw new FileNotFoundException("The authored JMO explosion source prefab was not found.");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(ExplosionVfxPrefabPath);
            try
            {
                root.transform.localScale = Vector3.one * 1.15f;
                ParticleSystemRenderer[] renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
                ParticleSystemRenderer[] sourceRenderers = sourceRoot.GetComponentsInChildren<ParticleSystemRenderer>(true);
                if (renderers.Length != sourceRenderers.Length)
                {
                    throw new InvalidDataException("Bomb explosion renderer count no longer matches the authored JMO source.");
                }

                for (int index = 0; index < renderers.Length; index++)
                {
                    ParticleSystemRenderer renderer = renderers[index];
                    renderer.sharedMaterial = sourceRenderers[index].sharedMaterial;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.sortingOrder = 12;
                    renderer.gameObject.SetActive(true);
                }

                RemoveFallbackExplosionBurst(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, ExplosionVfxPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RemoveFallbackExplosionBurst(Transform root)
        {
            Transform existing = FindDirectChild(root, "URP Explosion Burst");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static Transform EnsureFacingRoot(Transform soldier)
        {
            Transform existing = FindDirectChild(soldier, "Soldier Facing");
            if (existing != null)
            {
                existing.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                existing.localScale = Vector3.one;
                return existing;
            }

            GameObject facing = new("Soldier Facing");
            facing.transform.SetParent(soldier, false);
            return facing.transform;
        }

        private static void MoveUnderFacingRoot(Transform soldier, Transform facingRoot, string childName)
        {
            Transform child = FindDescendant(soldier, childName);
            if (child == null)
            {
                throw new MissingReferenceException("Soldier prefab is missing " + childName + ".");
            }

            if (child.parent != facingRoot)
            {
                child.SetParent(facingRoot, true);
            }
        }

        private static GameObject CreateTargetIndicator(GameObject soldier)
        {
            Transform existing = FindDescendant(soldier.transform, "Bomb Target Indicator");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(CirclePrefabPath);
            GameObject indicator = PrefabUtility.InstantiatePrefab(source, soldier.scene) as GameObject;
            if (indicator == null)
            {
                throw new InvalidDataException("Could not instantiate Matthew Guz Circle Select.");
            }

            indicator.name = "Bomb Target Indicator";
            indicator.transform.SetParent(soldier.transform, false);
            indicator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));
            indicator.transform.localScale = Vector3.one;

            ParticleSystemRenderer[] renderers = indicator.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].shadowCastingMode = ShadowCastingMode.Off;
                renderers[index].receiveShadows = false;
                renderers[index].sortingOrder = 8;
            }

            ParticleSystem[] particleSystems = indicator.GetComponentsInChildren<ParticleSystem>(true);
            for (int index = 0; index < particleSystems.Length; index++)
            {
                ParticleSystem.MainModule main = particleSystems[index].main;
                main.startColor = new Color(1f, 0.18f, 0.035f, 0.92f);
            }

            indicator.SetActive(false);
            return indicator;
        }

        private static void ConfigureTrajectory(Transform soldier)
        {
            Transform trajectoryTransform = FindDescendant(soldier, "Bomb Trajectory");
            Transform rangeTransform = FindDescendant(soldier, "Bomb Range");
            Transform oldBlastTransform = FindDescendant(soldier, "Bomb Blast Radius");
            if (trajectoryTransform == null || rangeTransform == null)
            {
                throw new MissingReferenceException("Soldier prefab is missing authored bomb preview lines.");
            }

            LineRenderer trajectory = trajectoryTransform.GetComponent<LineRenderer>();
            trajectory.widthMultiplier = 0.12f;
            trajectory.widthCurve = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.18f, 1f),
                new Keyframe(0.78f, 0.68f),
                new Keyframe(1f, 0.08f));
            trajectory.colorGradient = CreateTrajectoryGradient();
            trajectory.numCornerVertices = 3;
            trajectory.numCapVertices = 4;
            trajectory.textureMode = LineTextureMode.Stretch;
            trajectory.shadowCastingMode = ShadowCastingMode.Off;
            trajectory.receiveShadows = false;

            LineRenderer range = rangeTransform.GetComponent<LineRenderer>();
            range.widthMultiplier = 0.045f;
            range.startColor = new Color(0.15f, 0.9f, 1f, 0.34f);
            range.endColor = new Color(0.15f, 0.9f, 1f, 0.34f);
            range.shadowCastingMode = ShadowCastingMode.Off;
            range.receiveShadows = false;

            if (oldBlastTransform != null)
            {
                LineRenderer oldBlast = oldBlastTransform.GetComponent<LineRenderer>();
                if (oldBlast != null)
                {
                    oldBlast.enabled = false;
                }
                oldBlastTransform.gameObject.SetActive(false);
            }
        }

        private static Gradient CreateTrajectoryGradient()
        {
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.1f, 0.82f, 1f), 0f),
                    new GradientColorKey(new Color(0.35f, 1f, 0.62f), 0.55f),
                    new GradientColorKey(new Color(1f, 0.72f, 0.12f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.45f, 0f),
                    new GradientAlphaKey(1f, 0.18f),
                    new GradientAlphaKey(0.9f, 0.78f),
                    new GradientAlphaKey(0.08f, 1f)
                });
            return gradient;
        }

        private static void ConfigureExplosionAudio(Transform soldier, BombAudioCatalog catalog, BombController bomb)
        {
            Transform existing = FindDirectChild(soldier, "Bomb Explosion Audio");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject audioObject = new("Bomb Explosion Audio");
            audioObject.transform.SetParent(soldier, false);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.volume = 0.92f;

            BombExplosionAudioPlayer player = audioObject.AddComponent<BombExplosionAudioPlayer>();
            player.SetReferences(catalog, bomb, source);
        }

        private static BombAudioCatalog ConfigureAudioAddressable()
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

            BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;

            string guid = AssetDatabase.AssetPathToGUID(ExplosionAudioPath);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = "audio/bombs/explosion";
            entry.SetLabel("audio-bombs", true, true);

            BombAudioCatalog catalog = AssetDatabase.LoadAssetAtPath<BombAudioCatalog>(AudioCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<BombAudioCatalog>();
                AssetDatabase.CreateAsset(catalog, AudioCatalogPath);
            }

            catalog.Configure(new AssetReferenceT<AudioClip>(guid));
            EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(settings);
            return catalog;
        }

        private static void EnsureExplosionAudio()
        {
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(ExplosionAudioPath) == null)
            {
                if (!AssetDatabase.CopyAsset(ExplosionAudioSourcePath, ExplosionAudioPath))
                {
                    throw new IOException("Could not copy the authored explosion audio asset.");
                }
                AssetDatabase.ImportAsset(ExplosionAudioPath, ImportAssetOptions.ForceSynchronousImport);
            }

            AudioImporter importer = AssetImporter.GetAtPath(ExplosionAudioPath) as AudioImporter;
            if (importer == null)
            {
                throw new InvalidDataException("Bomb explosion audio importer was not created.");
            }

            AudioImporterSampleSettings sampleSettings = importer.defaultSampleSettings;
            importer.forceToMono = true;
            sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            sampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            sampleSettings.quality = 0.72f;
            sampleSettings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            sampleSettings.preloadAudioData = true;
            importer.defaultSampleSettings = sampleSettings;
            importer.SaveAndReimport();
        }

        private static void ValidateAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SoldierPrefabPath) == null)
            {
                throw new FileNotFoundException("Soldier prefab was not found.");
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CirclePrefabPath) == null)
            {
                throw new FileNotFoundException("Matthew Guz Circle Select prefab was not found.");
            }
            if (AssetDatabase.LoadAssetAtPath<AudioClip>(ExplosionAudioSourcePath) == null)
            {
                throw new FileNotFoundException("The authored bomb explosion audio source was not found.");
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_ZombieWar/Audio", "Bombs");
            EnsureFolder("Assets/_ZombieWar/Configs/Audio", "Bombs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                {
                    return transforms[index];
                }
            }
            return null;
        }
    }
}
