using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZombieWar.Levels;
using ZombieWar.Player;

namespace ZombieWar.Editor
{
    public sealed class WaveEditorWindow : EditorWindow
    {
        #region State
        private LevelCatalogConfig _levelCatalog;
        private WaveSequenceConfig _sequence;
        private WaveSequenceConfig _durationSequence;
        private float _desiredDuration = 180f;
        private Vector2 _scrollPosition;
        #endregion

        #region Lifecycle
        [MenuItem("Level Editor/Wave Editor")]
        public static void Open()
        {
            GetWindow<WaveEditorWindow>("Zombie War Waves");
        }

        private void OnEnable()
        {
            if (_sequence != null)
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:WaveSequenceConfig", new[] { "Assets/_ZombieWar/Configs" });
            if (guids.Length > 0)
            {
                _sequence = AssetDatabase.LoadAssetAtPath<WaveSequenceConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            string[] catalogGuids = AssetDatabase.FindAssets("t:LevelCatalogConfig", new[] { "Assets/_ZombieWar/Configs" });
            if (catalogGuids.Length > 0)
            {
                _levelCatalog = AssetDatabase.LoadAssetAtPath<LevelCatalogConfig>(AssetDatabase.GUIDToAssetPath(catalogGuids[0]));
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("ZOMBIE WAR — WAVE EDITOR", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Mỗi WaveConfig quản lý thời lượng, mật độ và trọng số loại zombie. Camera được quản lý bằng CameraProfileConfig riêng.",
                MessageType.Info);

            _levelCatalog = (LevelCatalogConfig)EditorGUILayout.ObjectField(
                "Level Catalog",
                _levelCatalog,
                typeof(LevelCatalogConfig),
                false);
            DrawLevelCatalog();

            WaveSequenceConfig selectedSequence = (WaveSequenceConfig)EditorGUILayout.ObjectField(
                "Wave Sequence",
                _sequence,
                typeof(WaveSequenceConfig),
                false);
            if (selectedSequence != _sequence)
            {
                _sequence = selectedSequence;
                SyncDesiredDuration();
            }
            if (_sequence == null)
            {
                EditorGUILayout.HelpBox("Chọn một WaveSequenceConfig để bắt đầu.", MessageType.Warning);
                return;
            }

            DrawSceneShortcuts();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawSequence();
            DrawCameraProfile();
            DrawWaves();
            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Internal
        private void DrawSceneShortcuts()
        {
            if (_levelCatalog == null || _levelCatalog.Levels == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            LevelDefinition[] levels = _levelCatalog.Levels;
            for (int i = 0; i < levels.Length; i++)
            {
                LevelDefinition level = levels[i];
                if (level != null && level.IsValid && GUILayout.Button($"Open {level.DisplayName}"))
                {
                    OpenScene(level.SceneName);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLevelCatalog()
        {
            if (_levelCatalog == null)
            {
                EditorGUILayout.HelpBox("Assign a LevelCatalogConfig to manage the level list.", MessageType.Warning);
                return;
            }

            SerializedObject serializedCatalog = new(_levelCatalog);
            serializedCatalog.Update();
            SerializedProperty levels = serializedCatalog.FindProperty("_levels");
            EditorGUILayout.PropertyField(levels, true);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Level"))
            {
                levels.InsertArrayElementAtIndex(levels.arraySize);
                SerializedProperty newLevel = levels.GetArrayElementAtIndex(levels.arraySize - 1);
                newLevel.FindPropertyRelative("_displayName").stringValue = "NEW AREA";
                newLevel.FindPropertyRelative("_sceneName").stringValue = string.Empty;
                newLevel.FindPropertyRelative("_waveSequence").objectReferenceValue = null;
                newLevel.FindPropertyRelative("_enabled").boolValue = true;
            }
            using (new EditorGUI.DisabledScope(levels.arraySize == 0))
            {
                if (GUILayout.Button("- Remove Last Level"))
                {
                    levels.DeleteArrayElementAtIndex(levels.arraySize - 1);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (serializedCatalog.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_levelCatalog);
            }
            if (_levelCatalog.Levels != null && _levelCatalog.Levels.Length > 6)
            {
                EditorGUILayout.HelpBox("Main Menu currently has 6 authored level button slots.", MessageType.Warning);
            }

            LevelDefinition[] catalogLevels = _levelCatalog.Levels;
            if (catalogLevels == null)
            {
                return;
            }
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < catalogLevels.Length; i++)
            {
                LevelDefinition level = catalogLevels[i];
                if (level == null)
                {
                    continue;
                }
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(120f));
                EditorGUILayout.LabelField($"Level {i + 1}", EditorStyles.miniBoldLabel);
                if (level.WaveSequence != null && GUILayout.Button("Edit Waves"))
                {
                    _sequence = level.WaveSequence;
                    SyncDesiredDuration();
                }
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i == 0))
                {
                    if (GUILayout.Button("▲"))
                    {
                        MoveLevel(i, i - 1);
                    }
                }
                using (new EditorGUI.DisabledScope(i >= catalogLevels.Length - 1))
                {
                    if (GUILayout.Button("▼"))
                    {
                        MoveLevel(i, i + 1);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSequence()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Sequence", EditorStyles.boldLabel);
            SerializedObject serializedSequence = new(_sequence);
            serializedSequence.Update();
            EditorGUILayout.PropertyField(serializedSequence.FindProperty("_displayName"));
            EditorGUILayout.PropertyField(serializedSequence.FindProperty("_hardCap"));
            EditorGUILayout.PropertyField(serializedSequence.FindProperty("_cameraProfile"));
            EditorGUILayout.PropertyField(serializedSequence.FindProperty("_waves"));
            serializedSequence.ApplyModifiedProperties();

            EditorGUILayout.LabelField("Total Duration", $"{_sequence.TotalDuration:0} seconds");
            EditorGUILayout.LabelField("Hard Cap", _sequence.HardCap.ToString());
            if (_durationSequence != _sequence)
            {
                SyncDesiredDuration();
            }
            _desiredDuration = EditorGUILayout.FloatField("Level Duration (seconds)", _desiredDuration);
            if (GUILayout.Button("Apply Duration Proportionally"))
            {
                ApplyTotalDuration();
            }
        }

        private void DrawCameraProfile()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Camera Profile", EditorStyles.boldLabel);
            CameraProfileConfig profile = _sequence.CameraProfile;
            if (profile == null)
            {
                EditorGUILayout.HelpBox("Sequence chưa có CameraProfileConfig.", MessageType.Warning);
                return;
            }

            SerializedObject serializedProfile = new(profile);
            serializedProfile.Update();
            SerializedProperty iterator = serializedProfile.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath != "m_Script")
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
            if (serializedProfile.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(profile);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview In Scene View"))
            {
                PreviewCamera(profile);
            }
            if (GUILayout.Button("Apply To Open Scene"))
            {
                ApplyCamera(profile);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawWaves()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Wave Timeline", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Wave"))
            {
                AddWave();
            }
            using (new EditorGUI.DisabledScope(_sequence.Waves == null || _sequence.Waves.Length == 0))
            {
                if (GUILayout.Button("- Remove Last Wave"))
                {
                    RemoveLastWave();
                }
            }
            EditorGUILayout.EndHorizontal();
            WaveConfig[] waves = _sequence.Waves;
            if (waves == null || waves.Length == 0)
            {
                EditorGUILayout.HelpBox("Sequence chưa có wave.", MessageType.Warning);
                return;
            }

            float startTime = 0f;
            for (int i = 0; i < waves.Length; i++)
            {
                WaveConfig wave = waves[i];
                if (wave == null)
                {
                    EditorGUILayout.HelpBox($"Wave slot {i + 1} đang trống.", MessageType.Error);
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(
                    $"{i + 1}. {wave.DisplayName}   [{startTime:0}s — {startTime + wave.DurationSeconds:0}s]",
                    EditorStyles.boldLabel);
                SerializedObject serializedWave = new(wave);
                serializedWave.Update();
                SerializedProperty iterator = serializedWave.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.propertyPath != "m_Script")
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                if (serializedWave.ApplyModifiedProperties())
                {
                    EditorUtility.SetDirty(wave);
                }
                EditorGUILayout.EndVertical();
                startTime += wave.DurationSeconds;
            }
        }

        private static void PreviewCamera(CameraProfileConfig profile)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            SoldierController soldier = Object.FindFirstObjectByType<SoldierController>(FindObjectsInactive.Include);
            if (sceneView == null || soldier == null)
            {
                Debug.LogWarning("[Zombie War] Open a level scene and Scene View before previewing the camera.");
                return;
            }

            sceneView.LookAtDirect(
                soldier.transform.position,
                Quaternion.Euler(profile.RotationEuler),
                profile.PreviewSize);
            sceneView.Repaint();
        }

        private static void ApplyCamera(CameraProfileConfig profile)
        {
            CinemachineCamera virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>(FindObjectsInactive.Include);
            if (virtualCamera == null || !virtualCamera.TryGetComponent(out CinemachineFollow follow))
            {
                Debug.LogWarning("[Zombie War] The open scene has no authored Cinemachine follow camera.");
                return;
            }

            Undo.RecordObjects(new Object[] { virtualCamera, follow, virtualCamera.transform }, "Apply Zombie War Camera Profile");
            LensSettings lens = virtualCamera.Lens;
            lens.FieldOfView = profile.FieldOfView;
            virtualCamera.Lens = lens;
            virtualCamera.transform.rotation = Quaternion.Euler(profile.RotationEuler);
            follow.FollowOffset = profile.FollowOffset;
            TrackerSettings tracker = follow.TrackerSettings;
            tracker.PositionDamping = profile.PositionDamping;
            follow.TrackerSettings = tracker;
            EditorUtility.SetDirty(virtualCamera);
            EditorUtility.SetDirty(follow);
            EditorSceneManager.MarkSceneDirty(virtualCamera.gameObject.scene);
            SceneView.RepaintAll();
        }

        private static void OpenScene(string sceneName)
        {
            EditorSceneManager.OpenScene($"Assets/_ZombieWar/Scenes/{sceneName}.unity", OpenSceneMode.Single);
        }

        private void SyncDesiredDuration()
        {
            _durationSequence = _sequence;
            _desiredDuration = _sequence != null ? _sequence.TotalDuration : 0f;
        }

        private void MoveLevel(int fromIndex, int toIndex)
        {
            SerializedObject serializedCatalog = new(_levelCatalog);
            serializedCatalog.Update();
            serializedCatalog.FindProperty("_levels").MoveArrayElement(fromIndex, toIndex);
            serializedCatalog.ApplyModifiedProperties();
            EditorUtility.SetDirty(_levelCatalog);
            AssetDatabase.SaveAssets();
            GUIUtility.ExitGUI();
        }

        private void ApplyTotalDuration()
        {
            WaveConfig[] waves = _sequence.Waves;
            if (waves == null || waves.Length == 0 || _desiredDuration <= 0f)
            {
                return;
            }

            float currentDuration = Mathf.Max(0.01f, _sequence.TotalDuration);
            float scale = _desiredDuration / currentDuration;
            for (int i = 0; i < waves.Length; i++)
            {
                if (waves[i] == null)
                {
                    continue;
                }
                SerializedObject serializedWave = new(waves[i]);
                serializedWave.Update();
                SerializedProperty duration = serializedWave.FindProperty("_durationSeconds");
                duration.floatValue = Mathf.Max(1f, duration.floatValue * scale);
                serializedWave.ApplyModifiedProperties();
                EditorUtility.SetDirty(waves[i]);
            }
            AssetDatabase.SaveAssets();
            SyncDesiredDuration();
        }

        private void AddWave()
        {
            string sequencePath = AssetDatabase.GetAssetPath(_sequence);
            string directory = System.IO.Path.GetDirectoryName(sequencePath)?.Replace('\\', '/');
            string baseName = System.IO.Path.GetFileNameWithoutExtension(sequencePath);
            string wavePath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{baseName}_Wave.asset");
            WaveConfig wave = CreateInstance<WaveConfig>();
            int waveNumber = _sequence.Waves?.Length + 1 ?? 1;
            wave.Configure($"WAVE {waveNumber}", 60f, 10, 25, 2, System.Array.Empty<WaveEnemyEntry>());
            AssetDatabase.CreateAsset(wave, wavePath);

            SerializedObject serializedSequence = new(_sequence);
            serializedSequence.Update();
            SerializedProperty waves = serializedSequence.FindProperty("_waves");
            waves.InsertArrayElementAtIndex(waves.arraySize);
            waves.GetArrayElementAtIndex(waves.arraySize - 1).objectReferenceValue = wave;
            serializedSequence.ApplyModifiedProperties();
            EditorUtility.SetDirty(_sequence);
            AssetDatabase.SaveAssets();
            SyncDesiredDuration();
        }

        private void RemoveLastWave()
        {
            SerializedObject serializedSequence = new(_sequence);
            serializedSequence.Update();
            SerializedProperty waves = serializedSequence.FindProperty("_waves");
            if (waves.arraySize == 0)
            {
                return;
            }
            int last = waves.arraySize - 1;
            waves.GetArrayElementAtIndex(last).objectReferenceValue = null;
            waves.DeleteArrayElementAtIndex(last);
            serializedSequence.ApplyModifiedProperties();
            EditorUtility.SetDirty(_sequence);
            AssetDatabase.SaveAssets();
            SyncDesiredDuration();
        }
        #endregion
    }
}
