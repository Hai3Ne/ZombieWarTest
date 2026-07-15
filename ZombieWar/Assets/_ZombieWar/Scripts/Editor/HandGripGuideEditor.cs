using UnityEditor;
using UnityEngine;
using ZombieWar.Player;

namespace ZombieWar.Editor
{
    [CustomEditor(typeof(HandGripGuide))]
    public sealed class HandGripGuideEditor : UnityEditor.Editor
    {
        #region Lifecycle
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "HAND GRIP DRAWING MODE\n"
                + "Cyan / FORWARD: fingers and weapon direction\n"
                + "Yellow / UP: palm normal\n"
                + "Magenta / RIGHT or LEFT: thumb direction\n"
                + "Rotate this Grip transform until the drawing matches the hand pose.",
                MessageType.Info);
            DrawDefaultInspector();
        }

        private void OnSceneGUI()
        {
            HandGripGuide guide = (HandGripGuide)target;
            Transform grip = guide.transform;
            float size = Mathf.Max(0.03f, guide.GuideSize);
            Vector3 position = grip.position;

            DrawPalm(guide, position, size);
            DrawDirection(position, guide.FingerDirection, size, new Color(0.1f, 0.9f, 1f), "FINGERS / FORWARD");
            DrawDirection(position, guide.PalmNormal, size * 0.72f, new Color(1f, 0.82f, 0.12f), "PALM / UP");
            DrawDirection(position, guide.ThumbDirection, size * 0.72f, new Color(1f, 0.2f, 0.75f), guide.IsLeftHand ? "THUMB / LEFT" : "THUMB / RIGHT");

            EditorGUI.BeginChangeCheck();
            Quaternion rotation = Handles.RotationHandle(grip.rotation, position);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(grip, "Rotate Hand Grip");
                grip.rotation = rotation;
                EditorUtility.SetDirty(grip);
            }
        }
        #endregion

        #region Internal
        private static void DrawPalm(HandGripGuide guide, Vector3 position, float size)
        {
            Vector3 fingers = guide.FingerDirection;
            Vector3 thumb = guide.ThumbDirection;
            Vector3 normal = guide.PalmNormal;
            Vector3 wrist = position - fingers * size * 0.32f;
            Vector3 palmCenter = position + fingers * size * 0.08f;
            Vector3 halfWidth = thumb * size * 0.3f;
            Vector3 halfLength = fingers * size * 0.34f;
            Vector3[] palm =
            {
                palmCenter - halfWidth - halfLength,
                palmCenter - halfWidth + halfLength,
                palmCenter + halfWidth + halfLength,
                palmCenter + halfWidth - halfLength
            };

            Handles.color = new Color(0.2f, 0.75f, 1f, 0.18f);
            Handles.DrawSolidRectangleWithOutline(palm, new Color(0.2f, 0.75f, 1f, 0.13f), new Color(0.7f, 0.95f, 1f, 0.9f));
            Handles.DrawLine(wrist, palmCenter - halfLength);
            Handles.DrawLine(position, position + normal * size * 0.15f);

            for (int i = -2; i <= 2; i++)
            {
                Vector3 fingerRoot = palmCenter + halfLength + thumb * (i * size * 0.11f);
                Handles.DrawLine(fingerRoot, fingerRoot + fingers * size * (0.27f - Mathf.Abs(i) * 0.025f));
            }
        }

        private static void DrawDirection(Vector3 position, Vector3 direction, float length, Color color, string label)
        {
            Handles.color = color;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Handles.ArrowHandleCap(0, position, rotation, length, EventType.Repaint);
            GUIStyle style = new(EditorStyles.boldLabel);
            style.normal.textColor = color;
            Handles.Label(position + direction * length, label, style);
        }
        #endregion
    }
}
