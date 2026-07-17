using UnityEditor;
using UnityEngine;
using ZombieWar.Player;

namespace ZombieWar.Editor
{
    public static class SoldierHandPoseInstaller
    {
        private const string SoldierPrefabPath = "Assets/_ZombieWar/Prefabs/Soldier.prefab";

        [MenuItem("Zombie War/Gameplay/Fix Soldier Hand Pose")]
        public static void Install()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SoldierPrefabPath);
            try
            {
                Transform model = FindChild(root.transform, "Character Model");
                if (model == null
                    || !model.TryGetComponent(out SoldierWeaponIkController weaponIk)
                    || !model.TryGetComponent(out Animator animator))
                {
                    Debug.LogError("[Zombie War] Soldier prefab is missing Character Model, Animator or weapon IK.");
                    return;
                }

                SerializedObject serializedIk = new(weaponIk);
                serializedIk.Update();
                serializedIk.FindProperty("_positionWeight").floatValue = 1f;
                serializedIk.FindProperty("_rotationWeight").floatValue = 0f;
                serializedIk.ApplyModifiedPropertiesWithoutUndo();

                SoldierHeadAttachmentController attachments = model.GetComponent<SoldierHeadAttachmentController>();
                if (attachments == null)
                {
                    attachments = model.gameObject.AddComponent<SoldierHeadAttachmentController>();
                }
                attachments.SetAttachments(new[]
                {
                    FindChild(model, "Eyebrows"),
                    FindChild(model, "Eyes"),
                    FindChild(model, "Glasses")
                });

                PrefabUtility.SaveAsPrefabAsset(root, SoldierPrefabPath);
                Debug.Log("[Zombie War] Soldier hand rotation IK and head attachments were repaired.");
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
    }
}
