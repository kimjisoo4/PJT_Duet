using UnityEditor;
using UnityEngine;
using StudioScor.Utilities.Editor;

namespace StudioScor.BodySystem.Editor
{
    [CustomEditor(typeof(BodySystemComponent))]
    [CanEditMultipleObjects]
    public sealed class BodySystemComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                GUILayout.Space(5f);
                SEditorUtility.GUI.DrawLine(4f);
                GUILayout.Space(5f);

                var bodySystem = (BodySystemComponent)target;

                var bodyParts = bodySystem.BodyParts;

                GUIStyle title = new();
                GUIStyle equip = new();
                GUIStyle none = new();

                title.normal.textColor = Color.white;
                title.alignment = TextAnchor.MiddleCenter;
                title.fontStyle = FontStyle.Bold;

                equip.normal.textColor = Color.green;
                none.normal.textColor = Color.gray;

                GUILayout.Label("[ BodyParts ]", title);

                if (bodyParts is not null)
                {
                    foreach (var bodypart in bodyParts)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(bodypart.Key.Name);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(bodypart.Value.CanEquiped ? "[ CanEquip ]" : "[ Equiping ]", bodypart.Value.CanEquiped ? equip : none);
                        GUILayout.Space(10f);
                        GUILayout.EndHorizontal();

                        SEditorUtility.GUI.DrawLine(1f);
                    }
                }
            }
        }
    }
}
