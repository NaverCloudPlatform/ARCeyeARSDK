using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(LayerInfoConverter), true)]
    public class LayerInfoConverterEditor : Editor
    {
        private SerializedProperty m_ScriptProp;
        private SerializedProperty m_LayerInfoSettingProp;

        void OnEnable()
        {
            m_ScriptProp = serializedObject.FindProperty("m_Script");
            m_LayerInfoSettingProp = serializedObject.FindProperty("m_LayerInfoSetting");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ScriptProp);
            }

            // LayerInfoSetting은 v1/v2 amproj일 때만 표시
            int version = ModelPathBrowseHelper.GetAMProjVersion();
            if (version >= 1 && version < 3)
            {
                EditorGUILayout.PropertyField(m_LayerInfoSettingProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
