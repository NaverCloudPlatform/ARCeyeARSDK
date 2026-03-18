using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomEditor(typeof(ARPlayGround))]
    public class ARPlayGroundEditor : Editor
    {
        private SerializedProperty m_ScriptProp;
        private SerializedProperty m_ContentsFolderProp;
        private SerializedProperty m_StageConfigProp;
        private SerializedProperty m_LoadOnAwakeProp;
        private SerializedProperty m_StreamingAssetsProp;
        private SerializedProperty m_LocaleProp;
        private SerializedProperty m_LogLevelProp;
        private SerializedProperty m_VisualizeAMProjProp;
        private SerializedProperty m_OnSceneLoadedProp;
        private SerializedProperty m_OnSceneUnloadedProp;
        private SerializedProperty m_OnStageChangedProp;
        private SerializedProperty m_OnPOIListLoadedProp;
        private SerializedProperty m_OnCustomRangeEnteredProp;
        private SerializedProperty m_OnCustomRangeExitedProp;
        private SerializedProperty m_OnNavigationStartedProp;
        private SerializedProperty m_OnNavigationEndedProp;
        private SerializedProperty m_OnNavigationFailedProp;
        private SerializedProperty m_OnNavigationReroutedProp;
        private SerializedProperty m_OnDistanceUpdatedProp;
        private SerializedProperty m_OnDestinationArrivedProp;
        private SerializedProperty m_OnTransitMovingStartedProp;
        private SerializedProperty m_OnTransitMovingEndedProp;
        private SerializedProperty m_OnTransitMovingFailedProp;

        void OnEnable()
        {
            m_ScriptProp = serializedObject.FindProperty("m_Script");
            m_ContentsFolderProp = serializedObject.FindProperty("m_ContentsFolder");
            m_StageConfigProp = serializedObject.FindProperty("m_StageConfig");
            m_LoadOnAwakeProp = serializedObject.FindProperty("<LoadOnAwake>k__BackingField");
            m_StreamingAssetsProp = serializedObject.FindProperty("<StreamingAssets>k__BackingField");
            m_LocaleProp = serializedObject.FindProperty("m_Locale");
            m_LogLevelProp = serializedObject.FindProperty("m_LogLevel");
            m_VisualizeAMProjProp = serializedObject.FindProperty("<VisualizeAMProj>k__BackingField");
            m_OnSceneLoadedProp = serializedObject.FindProperty("m_OnSceneLoaded");
            m_OnSceneUnloadedProp = serializedObject.FindProperty("m_OnSceneUnloaded");
            m_OnStageChangedProp = serializedObject.FindProperty("m_OnStageChanged");
            m_OnPOIListLoadedProp = serializedObject.FindProperty("m_OnPOIListLoaded");
            m_OnCustomRangeEnteredProp = serializedObject.FindProperty("m_OnCustomRangeEntered");
            m_OnCustomRangeExitedProp = serializedObject.FindProperty("m_OnCustomRangeExited");
            m_OnNavigationStartedProp = serializedObject.FindProperty("m_OnNavigationStarted");
            m_OnNavigationEndedProp = serializedObject.FindProperty("m_OnNavigationEnded");
            m_OnNavigationFailedProp = serializedObject.FindProperty("m_OnNavigationFailed");
            m_OnNavigationReroutedProp = serializedObject.FindProperty("m_OnNavigationRerouted");
            m_OnDistanceUpdatedProp = serializedObject.FindProperty("m_OnDistanceUpdated");
            m_OnDestinationArrivedProp = serializedObject.FindProperty("m_OnDestinationArrived");
            m_OnTransitMovingStartedProp = serializedObject.FindProperty("m_OnTransitMovingStarted");
            m_OnTransitMovingEndedProp = serializedObject.FindProperty("m_OnTransitMovingEnded");
            m_OnTransitMovingFailedProp = serializedObject.FindProperty("m_OnTransitMovingFailed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ScriptProp);
            }

            // Contents Folder with Browse button
            DrawContentsPathField();

            if (ModelPathBrowseHelper.GetAMProjVersion() >= 3)
            {
                EditorGUILayout.PropertyField(m_StageConfigProp);
            }

            // Core settings
            EditorGUILayout.PropertyField(m_StreamingAssetsProp);
            EditorGUILayout.PropertyField(m_LoadOnAwakeProp);
            EditorGUILayout.PropertyField(m_LocaleProp);

            // Debug
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_LogLevelProp);
            EditorGUILayout.PropertyField(m_VisualizeAMProjProp);

            // Events
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnSceneLoadedProp);
            EditorGUILayout.PropertyField(m_OnSceneUnloadedProp);
            EditorGUILayout.PropertyField(m_OnStageChangedProp);
            EditorGUILayout.PropertyField(m_OnPOIListLoadedProp);
            EditorGUILayout.PropertyField(m_OnCustomRangeEnteredProp);
            EditorGUILayout.PropertyField(m_OnCustomRangeExitedProp);

            // Navigation
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Navigation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnNavigationStartedProp);
            EditorGUILayout.PropertyField(m_OnNavigationEndedProp);
            EditorGUILayout.PropertyField(m_OnNavigationFailedProp);
            EditorGUILayout.PropertyField(m_OnNavigationReroutedProp);
            EditorGUILayout.PropertyField(m_OnDistanceUpdatedProp);
            EditorGUILayout.PropertyField(m_OnDestinationArrivedProp);
            EditorGUILayout.PropertyField(m_OnTransitMovingStartedProp);
            EditorGUILayout.PropertyField(m_OnTransitMovingEndedProp);
            EditorGUILayout.PropertyField(m_OnTransitMovingFailedProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawContentsPathField()
        {
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            float buttonWidth = 25f;
            float spacing = 2f;
            float totalButtonsWidth = buttonWidth * 2 + spacing * 2;

            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - totalButtonsWidth, rect.height);
            Rect refreshButtonRect = new Rect(rect.xMax - buttonWidth * 2 - spacing, rect.y, buttonWidth, rect.height);
            Rect browseButtonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);

            EditorGUI.LabelField(fieldRect, "Contents Folder");
            Rect valueRect = EditorGUI.PrefixLabel(fieldRect, new GUIContent("Contents Folder"));
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.TextField(valueRect, m_ContentsFolderProp.stringValue);
            }

            Texture2D refreshIcon = EditorGUIUtility.FindTexture("Refresh");
            if (GUI.Button(refreshButtonRect, new GUIContent(refreshIcon, "Refresh contents folder")))
            {
                RefreshContentsFolder();
            }

            Texture2D folderIcon = EditorGUIUtility.FindTexture("FolderOpened Icon");
            if (GUI.Button(browseButtonRect, new GUIContent(folderIcon)))
            {
                string rootPath = GetRootPath();

                // Determine initial directory: use current value's directory if valid, otherwise root
                string initialDir = rootPath;
                string currentValue = m_ContentsFolderProp.stringValue;
                if (!string.IsNullOrEmpty(currentValue))
                {
                    string currentDir = $"{rootPath}/{currentValue}".Replace("\\", "/");
                    if (System.IO.Directory.Exists(currentDir))
                    {
                        initialDir = currentDir;
                    }
                }

                if (!System.IO.Directory.Exists(initialDir))
                {
                    System.IO.Directory.CreateDirectory(rootPath);
                    initialDir = rootPath;
                }

                string selectedPath = EditorUtility.OpenFolderPanel("Select Contents Folder", initialDir, "");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Normalize path separators
                    selectedPath = selectedPath.Replace("\\", "/");
                    rootPath = rootPath.Replace("\\", "/");

                    // Ensure rootPath ends with /
                    if (!rootPath.EndsWith("/"))
                    {
                        rootPath += "/";
                    }

                    if (selectedPath == rootPath.TrimEnd('/') || selectedPath.StartsWith(rootPath))
                    {
                        string relativePath = selectedPath.Length >= rootPath.Length
                            ? selectedPath.Substring(rootPath.Length)
                            : "";

                        if (ValidateContentsFolder(rootPath, relativePath))
                        {
                            m_ContentsFolderProp.stringValue = relativePath;
                            serializedObject.ApplyModifiedProperties();
                            Debug.Log($"[ARPlayGround] Contents folder selected: {relativePath}");
                            NotifySceneOverlay(rootPath, relativePath);
                        }
                    }
                    else
                    {
                        string pathType = m_StreamingAssetsProp.boolValue ? "StreamingAssets" : "PersistentData";
                        EditorUtility.DisplayDialog(
                            "Error",
                            $"The selected folder is outside the {pathType} path.\n\n{rootPath}",
                            "OK"
                        );
                    }
                }
            }

        }

        private void RefreshContentsFolder()
        {
            string currentValue = m_ContentsFolderProp.stringValue;
            if (string.IsNullOrEmpty(currentValue))
            {
                Debug.LogWarning("[ARPlayGround] Contents folder is not set.");
                return;
            }

            string rootPath = GetRootPath();
            if (ValidateContentsFolder(rootPath, currentValue))
            {
                // amproj 버전 캐시 초기화
                ModelPathBrowseHelper.InvalidateCache();
                Debug.Log($"[ARPlayGround] Contents folder refreshed: {currentValue}");
                NotifySceneOverlay(rootPath, currentValue);
            }
        }

        private bool ValidateContentsFolder(string rootPath, string contentsPath)
        {
            if (string.IsNullOrEmpty(contentsPath))
            {
                Debug.LogError($"[ARPlayGround] No contents folder selected. Please select a subfolder inside: {rootPath}");
                return false;
            }

            string folderName = System.IO.Path.GetFileName(contentsPath);
            string amprojPath = $"{rootPath}/{contentsPath}/{folderName}.amproj";

            if (System.IO.File.Exists(amprojPath))
            {
                return true;
            }

            // Check if any .amproj file exists in the directory
            string contentsDir = $"{rootPath}/{contentsPath}";
            string[] amprojFiles = System.IO.Directory.Exists(contentsDir)
                ? System.IO.Directory.GetFiles(contentsDir, "*.amproj")
                : System.Array.Empty<string>();

            if (amprojFiles.Length > 0)
            {
                string foundFile = System.IO.Path.GetFileName(amprojFiles[0]);
                Debug.LogError($"[ARPlayGround] amproj file name does not match the contents folder. Expected: {folderName}.amproj, Found: {foundFile}");
            }
            else
            {
                Debug.LogError($"[ARPlayGround] No amproj file found in: {contentsDir}");
            }

            return false;
        }

        private void NotifySceneOverlay(string rootPath, string contentsPath)
        {
            string folderName = System.IO.Path.GetFileName(contentsPath);
            string amprojPath = $"{rootPath}/{contentsPath}/{folderName}.amproj";
            AMProjSceneOverlay.RequestLoadAMProj(amprojPath);
        }

        private string GetRootPath()
        {
            bool useStreamingAssets = m_StreamingAssetsProp.boolValue;

            if (useStreamingAssets)
            {
                return Application.streamingAssetsPath;
            }
            else
            {
                return Application.persistentDataPath;
            }
        }
    }
}
