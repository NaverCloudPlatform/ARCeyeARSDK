using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Newtonsoft.Json.Linq;

namespace ARCeye
{
    [CustomEditor(typeof(StageConfig))]
    public class StageConfigEditor : Editor
    {
        private SerializedProperty m_ScriptProp;
        private SerializedProperty m_StagesProp;

        private ReorderableList m_ReorderableList;

        // Stage Name 편집 모드 상태 (인덱스 기반)
        private HashSet<int> m_EditingStageNameIndices = new HashSet<int>();

        // 스테이지별 foldout 상태 (인덱스 기반)
        private HashSet<int> m_ExpandedIndices = new HashSet<int>();

        void OnEnable()
        {
            m_ScriptProp = serializedObject.FindProperty("m_Script");
            m_StagesProp = serializedObject.FindProperty("stages");

            m_ReorderableList = new ReorderableList(serializedObject, m_StagesProp, true, true, true, true);
            m_ReorderableList.drawHeaderCallback = DrawHeader;
            m_ReorderableList.drawElementCallback = DrawElement;
            m_ReorderableList.elementHeightCallback = GetElementHeight;
            m_ReorderableList.onRemoveCallback = OnRemove;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ScriptProp);
            }

            EditorGUILayout.Space();

            // + 버튼으로 amproj 파일 선택
            if (GUILayout.Button("Load amproj file", GUILayout.Height(24)))
            {
                SelectAMProjFile();
            }

            EditorGUILayout.Space();

            // 스테이지 리소스 리스트
            m_ReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        #region ReorderableList Callbacks

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Stage Resources");
        }

        private float GetElementHeight(int index)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (m_ExpandedIndices.Contains(index))
            {
                // foldout + 5 fields (Stage Name, Layer Info, IBL, Map Model, Map Height Field) + 하단 여백
                return lineHeight * 6 + EditorGUIUtility.standardVerticalSpacing;
            }

            return lineHeight;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_StagesProp.GetArrayElementAtIndex(index);
            var stageProp = element.FindPropertyRelative("stage");
            string stageName = stageProp.stringValue;

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float y = rect.y + 1f;

            // Foldout 헤더 (drag handle 영역만큼 오른쪽으로 이동)
            float dragHandleWidth = 14f;
            bool expanded = m_ExpandedIndices.Contains(index);
            Rect foldoutRect = new Rect(rect.x + dragHandleWidth, y, rect.width - dragHandleWidth, lineHeight);
            bool newExpanded = EditorGUI.Foldout(foldoutRect, expanded, stageName, true, EditorStyles.foldout);
            if (newExpanded != expanded)
            {
                if (newExpanded)
                    m_ExpandedIndices.Add(index);
                else
                    m_ExpandedIndices.Remove(index);
            }

            if (!newExpanded) return;

            y += lineHeight + spacing;

            float indent = 15f;

            var layerInfoProp = element.FindPropertyRelative("layerInfo");
            var mapModelProp = element.FindPropertyRelative("mapModel");
            var iblProp = element.FindPropertyRelative("ibl");
            var mapHeightFieldProp = element.FindPropertyRelative("mapHeightField");

            // Stage Name
            float buttonWidth = 25f;
            float buttonSpacing = 2f;
            Rect stageNameRect = new Rect(rect.x + indent, y, rect.width - indent - buttonWidth - buttonSpacing, lineHeight);
            Rect stageNameValueRect = EditorGUI.PrefixLabel(stageNameRect, new GUIContent("Stage Name"));
            Rect editButtonRect = new Rect(rect.xMax - buttonWidth, y, buttonWidth, lineHeight);

            bool isEditing = m_EditingStageNameIndices.Contains(index);
            string stageNameControlName = $"StageNameField_{index}";

            using (new EditorGUI.DisabledScope(!isEditing))
            {
                GUI.SetNextControlName(stageNameControlName);
                string newStageName = EditorGUI.TextField(stageNameValueRect, stageName);
                if (newStageName != stageName)
                {
                    stageProp.stringValue = newStageName;
                }
            }

            // 포커스가 벗어나면 편집 모드 해제
            if (isEditing)
            {
                if (GUI.GetNameOfFocusedControl() != stageNameControlName)
                {
                    m_EditingStageNameIndices.Remove(index);
                }
                else
                {
                    // 편집 중에는 포커스 변경을 즉시 감지하기 위해 Repaint 요청
                    Repaint();
                }
            }

            Texture2D editIcon = EditorGUIUtility.FindTexture("editicon.sml");
            if (GUI.Button(editButtonRect, new GUIContent(editIcon)))
            {
                m_EditingStageNameIndices.Add(index);
                EditorGUI.FocusTextInControl(stageNameControlName);
            }
            y += lineHeight + spacing;
            float fieldX = rect.x + indent;
            float fieldWidth = rect.width - indent;

            // Layer Info
            Rect layerInfoRect = new Rect(fieldX, y, fieldWidth, lineHeight);
            EditorGUI.PropertyField(layerInfoRect, layerInfoProp, new GUIContent("Layer Info"));
            y += lineHeight + spacing;

            // Map Model
            Rect mapModelRect = new Rect(fieldX, y, fieldWidth, lineHeight);
            DrawModelPathFieldInRect(mapModelRect, mapModelProp, "Map Model");
            y += lineHeight + spacing;

            // Map Height Field
            Rect mapHeightFieldRect = new Rect(fieldX, y, fieldWidth, lineHeight);
            DrawModelPathFieldInRect(mapHeightFieldRect, mapHeightFieldProp, "Map Height Field");
            y += lineHeight + spacing;

            // IBL
            Rect iblRect = new Rect(fieldX, y, fieldWidth, lineHeight);
            DrawModelPathFieldInRect(iblRect, iblProp, "IBL");
        }

        private void OnRemove(ReorderableList list)
        {
            m_ExpandedIndices.Remove(list.index);
            m_EditingStageNameIndices.Remove(list.index);

            // 삭제된 인덱스 이후의 인덱스를 하나씩 내림
            m_ExpandedIndices = ShiftIndicesAfterRemoval(m_ExpandedIndices, list.index);
            m_EditingStageNameIndices = ShiftIndicesAfterRemoval(m_EditingStageNameIndices, list.index);

            m_StagesProp.DeleteArrayElementAtIndex(list.index);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        private static HashSet<int> ShiftIndicesAfterRemoval(HashSet<int> indices, int removedIndex)
        {
            var result = new HashSet<int>();
            foreach (int i in indices)
            {
                if (i > removedIndex)
                    result.Add(i - 1);
                else if (i < removedIndex)
                    result.Add(i);
            }
            return result;
        }

        #region Model Path Field (Rect 기반)

        private void DrawModelPathFieldInRect(Rect rect, SerializedProperty prop, string label)
        {
            float buttonWidth = 25f;
            float spacing = 2f;

            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - buttonWidth - spacing, rect.height);
            Rect buttonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);

            Rect valueRect = EditorGUI.PrefixLabel(fieldRect, new GUIContent(label));
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.TextField(valueRect, prop.stringValue);
            }

            Texture2D folderIcon = EditorGUIUtility.FindTexture("FolderOpened Icon");
            if (GUI.Button(buttonRect, new GUIContent(folderIcon)))
            {
                ModelPathBrowseHelper.DrawModelPathField(serializedObject, prop, label);
            }
        }

        #endregion

        private void SelectAMProjFile()
        {
            ARPlayGround arPlayGround = FindFirstObjectByType<ARPlayGround>();
            string initialDir = Application.streamingAssetsPath;
            if (arPlayGround != null)
            {
                string rootPath = arPlayGround.StreamingAssets
                    ? Application.streamingAssetsPath
                    : Application.persistentDataPath;
                string contentsFolder = arPlayGround.ContentsFolder;
                if (!string.IsNullOrEmpty(contentsFolder))
                {
                    string dir = $"{rootPath}/{contentsFolder}";
                    if (Directory.Exists(dir))
                        initialDir = dir;
                }
            }

            string selectedPath = EditorUtility.OpenFilePanel("Select amproj file", initialDir, "amproj");
            if (string.IsNullOrEmpty(selectedPath))
                return;

            if (m_StagesProp.arraySize > 0 && !EditorUtility.DisplayDialog(
                "Load amproj file",
                "Loading an amproj file will overwrite all existing stage configurations. Do you want to continue?",
                "Continue",
                "Cancel"))
            {
                return;
            }

            ParseAMProjAndPopulate(selectedPath);
        }

        private void ParseAMProjAndPopulate(string amprojPath)
        {
            if (!File.Exists(amprojPath))
            {
                Debug.LogError("[StageConfig] amproj file not found: " + amprojPath);
                return;
            }

            string json = File.ReadAllText(amprojPath);
            JObject root = JObject.Parse(json);
            int version = root["version"]?.Value<int>() ?? 1;

            if (version < 3)
            {
                Debug.LogWarning("[StageConfig] v1/v2 amproj is not supported. Please select a v3 amproj file.");
                return;
            }

            // v3: root.children에서 Stage 추출
            var rootChildren = root["root"]?["children"] as JArray;
            if (rootChildren == null)
            {
                Debug.LogWarning("[StageConfig] root.children not found in v3 amproj.");
                return;
            }

            var stageNames = new List<string>();
            foreach (var child in rootChildren)
            {
                if (child["type"]?.Value<string>() == "Stage" && child["usage"]?.Value<int>() != 2)
                {
                    string name = child["name"]?.Value<string>();
                    if (!string.IsNullOrEmpty(name))
                    {
                        stageNames.Add(name);
                    }
                }
            }

            if (stageNames.Count == 0)
            {
                Debug.LogWarning("[StageConfig] No stages found in v3 amproj.");
                return;
            }

            // 기존 목록 초기화 후 스테이지별 요소 추가
            Undo.RecordObject(target, "Populate StageConfig from amproj");

            m_StagesProp.arraySize = stageNames.Count;
            m_ExpandedIndices.Clear();
            for (int i = 0; i < stageNames.Count; i++)
            {
                var element = m_StagesProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("stage").stringValue = stageNames[i];
                element.FindPropertyRelative("layerInfo").stringValue = "";
                element.FindPropertyRelative("ibl").stringValue = "";
                element.FindPropertyRelative("mapModel").stringValue = "";
                element.FindPropertyRelative("mapHeightField").stringValue = "";

                m_ExpandedIndices.Add(i);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);

            string fileName = Path.GetFileName(amprojPath);
            Debug.Log($"[StageConfig] Loaded {stageNames.Count} stage(s) from {fileName}.");
        }
    }
}
