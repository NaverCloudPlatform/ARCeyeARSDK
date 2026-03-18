using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(NaviItemGenerator))]
    public class NaviItemGeneratorEditor : Editor
    {
        private SerializedProperty m_ScriptProp;
        private SerializedProperty m_TurnSpotPrefabProp;

        void OnEnable()
        {
            m_ScriptProp = serializedObject.FindProperty("m_Script");
            m_TurnSpotPrefabProp = serializedObject.FindProperty("TurnSpotPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ScriptProp);
            }

            EditorGUILayout.PropertyField(m_TurnSpotPrefabProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
