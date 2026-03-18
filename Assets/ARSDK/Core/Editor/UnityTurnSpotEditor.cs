using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(UnityTurnSpot), true)]
    public class UnityTurnSpotEditor : Editor
    {
        private SerializedProperty m_ScriptProp;

        private static readonly string[] ModelPathFieldNames = new string[]
        {
            "TurnSpotLeft",
            "TurnSpotRight",
            "TurnSpotStraight",
            "TurnSpotUp",
            "TurnSpotDown",
            "Destination",
        };

        private static readonly string[] ModelPathLabels = new string[]
        {
            "Turn Spot Left",
            "Turn Spot Right",
            "Turn Spot Straight",
            "Turn Spot Up",
            "Turn Spot Down",
            "Destination",
        };

        // turnspot type mapping: index → type
        // 0=left, 1=right, 2=straight, 3=up, 4=down, 5=destination
        private static readonly int[] TurnSpotTypes = new int[] { 0, 1, 2, 3, 4, 5 };

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

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ScriptProp);
            }

            // Model Paths
            EditorGUILayout.LabelField("Model Paths", EditorStyles.boldLabel);

            for (int i = 0; i < m_ModelPathProps.Length; i++)
            {
                string amprojPath = m_IndicatorInfo?.GetTurnSpotAsset(TurnSpotTypes[i]);
                ModelPathBrowseHelper.DrawModelPathField(serializedObject, m_ModelPathProps[i], ModelPathLabels[i], amprojPath);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
