using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(NextStep))]
    public class NextStepEditor : Editor
    {
        private SerializedProperty m_ScriptProp;

        private static readonly string[] ModelPathFieldNames = new string[]
        {
            "NextStepArrow",
            "NextStepDot",
            "NextStepText",
        };

        private static readonly string[] ModelPathLabels = new string[]
        {
            "Next Step Arrow",
            "Next Step Dot",
            "Next Step Text",
        };

        // nextstep name mapping
        private static readonly string[] NextStepNames = new string[] { "arrow", "dot", "text" };

        private SerializedProperty[] m_ModelPathProps;
        private AMProjIndicatorInfo m_IndicatorInfo;

        void OnEnable()
        {
            m_ScriptProp = serializedObject.FindProperty("m_Script");

            m_ModelPathProps = new SerializedProperty[ModelPathFieldNames.Length];
            for (int i = 0; i < ModelPathFieldNames.Length; i++)
            {
                m_ModelPathProps[i] = serializedObject.FindProperty(ModelPathFieldNames[i]);
            }

            m_IndicatorInfo = ModelPathBrowseHelper.ParseAMProjIndicators();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ScriptProp);
            }

            // Model Paths
            EditorGUILayout.LabelField("Model Paths", EditorStyles.boldLabel);

            for (int i = 0; i < m_ModelPathProps.Length; i++)
            {
                string amprojPath = m_IndicatorInfo?.GetNextStepAsset(NextStepNames[i]);
                ModelPathBrowseHelper.DrawModelPathField(serializedObject, m_ModelPathProps[i], ModelPathLabels[i], amprojPath);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
