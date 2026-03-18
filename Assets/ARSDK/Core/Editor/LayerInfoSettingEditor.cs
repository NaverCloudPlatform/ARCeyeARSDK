using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(LayerInfoSetting))]
    public class LayerInfoSettingEditor : Editor
    {
        private LayerInfoSetting m_LayerInfoSetting;

        private SerializedProperty m_LayerTreeProp;

        void OnEnable()
        {
            m_LayerInfoSetting = (LayerInfoSetting)target;
            m_LayerTreeProp = serializedObject.FindProperty("m_LayerTree");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLogo();

            EditorGUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            headerStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Configuration", headerStyle);

            LayerTreeEditorHelper.DrawLayerTree(m_LayerInfoSetting.layerTree, m_LayerTreeProp);
            LayerTreeEditorHelper.DrawPatterns(m_LayerInfoSetting.layerTree);
        }

        private void DrawLogo()
        {
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();
            style.fixedHeight = 30;
            style.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label(Resources.Load("Sprites/ARSDK-Logo") as Texture, style, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space();
        }
    }
}
