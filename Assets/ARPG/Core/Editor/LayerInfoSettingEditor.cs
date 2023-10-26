using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(LayerInfoSetting))]
    public class LayerInfoSettingEditor : Editor
    {
        private LayerInfoSetting m_LayerInfoSetting;

        private SerializedProperty m_LayerTreeProp;

        private Color m_OriginalContentColor;
        private Color m_OriginalBackgroundColor;


        const int kMAX_DEPTH = 7;

        void OnEnable()
        {
            m_LayerInfoSetting = (LayerInfoSetting) target;
            m_LayerTreeProp = serializedObject.FindProperty("m_LayerTree");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLogo();
            DrawAllLayers();
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

        private void DrawAllLayers()
        {
            SerializedLayerTree layerTree = m_LayerInfoSetting.layerTree;
            Layer layer = layerTree.Deserialize();

            DrawLayer(layer, 0);

            layerTree.SerializeFromLayer(layer);

            EditorUtility.SetDirty(m_LayerTreeProp.serializedObject.targetObject);
            m_LayerTreeProp.serializedObject.ApplyModifiedProperties();
        }

        private void DrawLayer(Layer layer, int depth)
        {
            if(layer == null) return;
            if(depth == kMAX_DEPTH) return;

            m_OriginalContentColor = GUI.contentColor;
            m_OriginalBackgroundColor = GUI.backgroundColor;

            
            EditorGUI.indentLevel = depth;

            EditorGUILayout.BeginHorizontal();

            // FoldOut 타이틀 바.
            DrawFoldOutLabel(layer, depth);

            // 새 계층 추가 버튼
            DrawAddLayerButton(layer, depth);

            // 계층 삭제 버튼
            DrawRemoveLayerButton(layer);

            EditorGUILayout.EndHorizontal();


            // Foldout 컨텐츠.
            if(layer.foldout)
            {
                DrawFoldOutContents(layer, depth);
            }
        }

        private void DrawFoldOutLabel(Layer layer, int depth)
        {
            string foldoutLabel;

            if(layer.linkToStage)
            {
                GUI.contentColor = Color.yellow;
                foldoutLabel = $"계층 {depth + 1}  {layer.layerName} → {layer.stageName}";
            }
            else
            {
                foldoutLabel = $"계층 {depth + 1}  {layer.layerName}";
            }

            layer.foldout = EditorGUILayout.Foldout(layer.foldout, foldoutLabel);

            GUI.contentColor = m_OriginalContentColor;
        }

        private void DrawAddLayerButton(Layer layer, int depth)
        {
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.green;
            if(layer.foldout && !layer.linkToStage && layer.depth < 6)
            {
                if(GUILayout.Button($"새 계층 {depth + 2} 추가")) {
                    layer.subLayers.Add(new Layer(depth + 1));
                }
            }
            GUI.backgroundColor = m_OriginalBackgroundColor;
        }

        private void DrawRemoveLayerButton(Layer layer)
        {
            GUI.backgroundColor = Color.red;
            if(GUILayout.Button($"-"))
            {
                layer.isRemoved = true;
            }
            GUI.backgroundColor = m_OriginalBackgroundColor;
        }

        private void DrawFoldOutContents(Layer layer, int depth)
        {
            EditorGUI.indentLevel++;

            layer.layerName = EditorGUILayout.TextField("계층 이름", layer.layerName);
            layer.linkToStage = EditorGUILayout.Toggle("스테이지 연결", layer.linkToStage);

            if(layer.linkToStage)
            {
                layer.stageName = EditorGUILayout.TextField("스테이지 이름", layer.stageName);
            }
            else
            {
                List<Layer> subLayers = layer.subLayers;
                Layer removedLayer = null;
                foreach(var child in subLayers)
                {
                    if(child.data == null) continue;
                    DrawLayer(child, depth + 1);

                    if(child.isRemoved) {
                        removedLayer = child;
                    }
                }

                if(removedLayer != null)
                {
                    subLayers.Remove(removedLayer);
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}