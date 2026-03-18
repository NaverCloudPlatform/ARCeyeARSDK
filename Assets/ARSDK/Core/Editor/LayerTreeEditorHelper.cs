using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    /// <summary>
    /// SerializedLayerTree의 Inspector 렌더링 공용 유틸.
    /// LayerInfoSettingEditor, StageConfigEditor 등에서 사용.
    /// </summary>
    public static class LayerTreeEditorHelper
    {
        private const int kMAX_DEPTH = 7;

        private static Color s_OriginalContentColor;
        private static Color s_OriginalBackgroundColor;

        /// <summary>
        /// SerializedLayerTree를 Inspector에 렌더링하고, 변경 사항을 직렬화.
        /// </summary>
        public static void DrawLayerTree(SerializedLayerTree layerTree, SerializedProperty layerTreeProp)
        {
            Layer layer = layerTree.Deserialize();

            DrawLayer(layer, 0);

            layerTree.SerializeFromLayer(layer);

            EditorUtility.SetDirty(layerTreeProp.serializedObject.targetObject);
            layerTreeProp.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// LayerInfo → StageName 매핑 테이블을 표시.
        /// </summary>
        public static void DrawPatterns(SerializedLayerTree layerTree)
        {
            Layer rootLayer = layerTree.Deserialize();

            var linkedLayers = new List<(string layerPath, string stageName)>();
            CollectLinkedLayers(rootLayer, "", linkedLayers);

            if (linkedLayers.Count == 0) return;

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

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

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();
        }

        private static void DrawLayer(Layer layer, int depth)
        {
            if (layer == null) return;
            if (depth == kMAX_DEPTH) return;

            s_OriginalContentColor = GUI.contentColor;
            s_OriginalBackgroundColor = GUI.backgroundColor;

            EditorGUI.indentLevel = depth;

            EditorGUILayout.BeginHorizontal();

            DrawFoldOutLabel(layer, depth);

            GUILayout.FlexibleSpace();

            DrawRemoveLayerButton(layer);

            EditorGUILayout.EndHorizontal();

            if (layer.foldout)
            {
                DrawFoldOutContents(layer, depth);
            }
        }

        private static void DrawFoldOutLabel(Layer layer, int depth)
        {
            string layerPrefix = $"Layer {depth + 1}";

            if (layer.linkToStage)
            {
                GUI.contentColor = Color.yellow;
            }

            layer.foldout = EditorGUILayout.Foldout(layer.foldout, layerPrefix, true);

            GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);
            textFieldStyle.margin = new RectOffset(0, 0, 0, 0);
            layer.layerName = EditorGUILayout.TextField(layer.layerName, textFieldStyle, GUILayout.Width(150));

            GUI.contentColor = s_OriginalContentColor;
        }

        private static void DrawAddLayerButton(Layer layer, int depth)
        {
            if (layer.linkToStage || layer.depth >= 6) return;

            float indentWidth = (depth + 1) * 15f;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indentWidth);
            if (GUILayout.Button($"Add Layer {depth + 2}"))
            {
                layer.subLayers.Add(new Layer(depth + 1));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawRemoveLayerButton(Layer layer)
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button($"-"))
            {
                layer.isRemoved = true;
            }
            GUI.backgroundColor = s_OriginalBackgroundColor;
        }

        private static void DrawFoldOutContents(Layer layer, int depth)
        {
            EditorGUI.indentLevel++;

            bool hasSubLayers = layer.subLayers != null && layer.subLayers.Count > 0;

            if (!hasSubLayers)
            {
                layer.linkToStage = EditorGUILayout.Toggle("Link to Stage", layer.linkToStage);
            }

            if (layer.linkToStage && !hasSubLayers)
            {
                GUI.contentColor = Color.yellow;
                layer.stageName = EditorGUILayout.TextField("Stage Name", layer.stageName);
                GUI.contentColor = s_OriginalContentColor;
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

                DrawAddLayerButton(layer, depth);
            }

            EditorGUI.indentLevel--;
        }

        private static void CollectLinkedLayers(Layer layer, string parentPath, List<(string layerPath, string stageName)> result)
        {
            if (layer == null) return;

            string currentPath = string.IsNullOrEmpty(parentPath)
                ? layer.layerName
                : parentPath + "_" + layer.layerName;

            if (layer.linkToStage && !string.IsNullOrEmpty(layer.stageName))
            {
                result.Add((currentPath, layer.stageName));
            }

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
