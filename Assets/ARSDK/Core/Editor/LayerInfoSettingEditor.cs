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
            m_LayerInfoSetting = (LayerInfoSetting)target;
            m_LayerTreeProp = serializedObject.FindProperty("m_LayerTree");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLogo();
            DrawAllLayers();
            DrawPatterns();
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
            // Location 헤더
            EditorGUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            headerStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Configuration", headerStyle);

            SerializedLayerTree layerTree = m_LayerInfoSetting.layerTree;
            Layer layer = layerTree.Deserialize();

            DrawLayer(layer, 0);

            layerTree.SerializeFromLayer(layer);

            EditorUtility.SetDirty(m_LayerTreeProp.serializedObject.targetObject);
            m_LayerTreeProp.serializedObject.ApplyModifiedProperties();
        }

        private void DrawLayer(Layer layer, int depth)
        {
            if (layer == null) return;
            if (depth == kMAX_DEPTH) return;

            m_OriginalContentColor = GUI.contentColor;
            m_OriginalBackgroundColor = GUI.backgroundColor;


            EditorGUI.indentLevel = depth;

            EditorGUILayout.BeginHorizontal();

            // FoldOut 타이틀 바.
            DrawFoldOutLabel(layer, depth);

            GUILayout.FlexibleSpace();

            // 계층 삭제 버튼
            DrawRemoveLayerButton(layer);

            EditorGUILayout.EndHorizontal();


            // Foldout 컨텐츠.
            if (layer.foldout)
            {
                DrawFoldOutContents(layer, depth);
            }
        }

        private void DrawFoldOutLabel(Layer layer, int depth)
        {
            string layerPrefix = $"Layer {depth + 1}";

            if (layer.linkToStage)
            {
                GUI.contentColor = Color.yellow;
            }

            // Foldout with minimal label
            layer.foldout = EditorGUILayout.Foldout(layer.foldout, layerPrefix, true);

            // Layer Name TextField inline with vertical alignment
            GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);
            textFieldStyle.margin = new RectOffset(0, 0, 0, 0);
            layer.layerName = EditorGUILayout.TextField(layer.layerName, textFieldStyle, GUILayout.Width(150));

            GUI.contentColor = m_OriginalContentColor;
        }

        private void DrawAddLayerButton(Layer layer, int depth)
        {
            if (layer.linkToStage || layer.depth >= 6) return;

            // depth에 따라 들여쓰기 계산 (Unity 기본 indent는 약 15픽셀)
            float indentWidth = (depth + 1) * 15f;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indentWidth);
            if (GUILayout.Button($"Add Layer {depth + 2}"))
            {
                layer.subLayers.Add(new Layer(depth + 1));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRemoveLayerButton(Layer layer)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button($"-"))
            {
                layer.isRemoved = true;
            }
            GUI.backgroundColor = m_OriginalBackgroundColor;
        }

        private void DrawFoldOutContents(Layer layer, int depth)
        {
            EditorGUI.indentLevel++;

            bool hasSubLayers = layer.subLayers != null && layer.subLayers.Count > 0;

            // 하위 레이어가 없을 때만 Link to Stage 표시
            if (!hasSubLayers)
            {
                layer.linkToStage = EditorGUILayout.Toggle("Link to Stage", layer.linkToStage);
            }

            if (layer.linkToStage && !hasSubLayers)
            {
                GUI.contentColor = Color.yellow;
                layer.stageName = EditorGUILayout.TextField("Stage Name", layer.stageName);
                GUI.contentColor = m_OriginalContentColor;
            }
            else
            {
                List<Layer> subLayers = layer.subLayers;
                Layer removedLayer = null;
                foreach (var child in subLayers)
                {
                    if (child.data == null) continue;
                    DrawLayer(child, depth + 1);

                    if (child.isRemoved)
                    {
                        removedLayer = child;
                    }
                }

                if (removedLayer != null)
                {
                    subLayers.Remove(removedLayer);
                }

                // 하위 레이어 목록 아래에 Add New Layer 버튼 표시
                DrawAddLayerButton(layer, depth);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawPatterns()
        {
            SerializedLayerTree layerTree = m_LayerInfoSetting.layerTree;
            Layer rootLayer = layerTree.Deserialize();

            // LinkToStage가 true인 모든 레이어 수집
            List<(string layerPath, string stageName)> linkedLayers = new List<(string, string)>();
            CollectLinkedLayers(rootLayer, "", linkedLayers);

            if (linkedLayers.Count == 0) return;

            EditorGUILayout.Space(10);

            // 사각형 영역 시작
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 내부 마진 추가
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();

            // 테이블 헤더
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layer Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Stage Name", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // 테이블 내용
            foreach (var item in linkedLayers)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.layerPath);
                EditorGUILayout.LabelField(item.stageName);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();

            // 하단 마진 추가
            GUILayout.Space(5);

            // 사각형 영역 끝
            EditorGUILayout.EndVertical();
        }

        private void CollectLinkedLayers(Layer layer, string parentPath, List<(string layerPath, string stageName)> result)
        {
            if (layer == null) return;

            // 현재 레이어의 경로 생성
            string currentPath = string.IsNullOrEmpty(parentPath)
                ? layer.layerName
                : parentPath + "_" + layer.layerName;

            // LinkToStage가 true이면 결과에 추가
            if (layer.linkToStage && !string.IsNullOrEmpty(layer.stageName))
            {
                result.Add((currentPath, layer.stageName));
            }

            // 하위 레이어 순회
            if (layer.subLayers != null)
            {
                foreach (var subLayer in layer.subLayers)
                {
                    if (subLayer.data != null)
                    {
                        CollectLinkedLayers(subLayer, currentPath, result);
                    }
                }
            }
        }
    }
}