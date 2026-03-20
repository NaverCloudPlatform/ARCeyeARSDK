using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    public static class ModelPathBrowseHelper
    {
        /// <summary>
        /// Draws a model path field with Browse button.
        /// If amprojAssetPath is not null, the field is locked to the amproj value.
        /// </summary>
        public static void DrawModelPathField(SerializedObject serializedObject, SerializedProperty prop, string label, string amprojAssetPath = null)
        {
            bool lockedByAmproj = amprojAssetPath != null;

            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
            float buttonWidth = 25f;
            float spacing = 2f;

            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - buttonWidth - spacing, rect.height);
            Rect buttonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);

            EditorGUI.LabelField(fieldRect, label);
            Rect valueRect = EditorGUI.PrefixLabel(fieldRect, new GUIContent(label));
            using (new EditorGUI.DisabledScope(true))
            {
                string displayValue = lockedByAmproj ? amprojAssetPath : prop.stringValue;
                EditorGUI.TextField(valueRect, displayValue);
            }

            using (new EditorGUI.DisabledScope(lockedByAmproj))
            {
                Texture2D folderIcon = EditorGUIUtility.FindTexture("FolderOpened Icon");
                GUIContent buttonContent = lockedByAmproj
                    ? new GUIContent(folderIcon, "Asset path is defined in the amproj file.")
                    : new GUIContent(folderIcon);

                if (GUI.Button(buttonRect, buttonContent))
                {
                    BrowseAndAssign(serializedObject, prop, label);
                }
            }
        }

        public static void BrowseAndAssign(SerializedObject serializedObject, SerializedProperty prop, string label)
        {
            ARPlayGround arPlayGround = Object.FindFirstObjectByType<ARPlayGround>();
            if (arPlayGround == null)
            {
                Debug.LogError($"[{label}] ARPlayGround not found in the scene.");
                return;
            }

            string rootPath = GetRootPath(arPlayGround);
            string contentsFolder = arPlayGround.ContentsFolder;
            string contentsFolderPath = string.IsNullOrEmpty(contentsFolder)
                ? rootPath
                : $"{rootPath}/{contentsFolder}";

            contentsFolderPath = contentsFolderPath.Replace("\\", "/");
            rootPath = rootPath.Replace("\\", "/");

            // Determine initial directory
            string initialDir = System.IO.Directory.Exists(contentsFolderPath) ? contentsFolderPath : rootPath;
            string currentValue = prop.stringValue;
            if (!string.IsNullOrEmpty(currentValue))
            {
                string currentDir = System.IO.Path.GetDirectoryName($"{contentsFolderPath}{currentValue}");
                if (currentDir != null)
                {
                    currentDir = currentDir.Replace("\\", "/");
                    if (System.IO.Directory.Exists(currentDir))
                    {
                        initialDir = currentDir;
                    }
                }
            }

            string selectedPath = EditorUtility.OpenFilePanel($"Select {label}", initialDir, "");

            if (string.IsNullOrEmpty(selectedPath))
                return;

            selectedPath = selectedPath.Replace("\\", "/");

            if (!rootPath.EndsWith("/"))
            {
                rootPath += "/";
            }

            // Validate the selected path is inside the root path
            if (!selectedPath.StartsWith(rootPath.TrimEnd('/')) && !selectedPath.StartsWith(rootPath))
            {
                string pathType = arPlayGround.StreamingAssets ? "StreamingAssets" : "PersistentData";
                Debug.LogError($"[{label}] The selected file is outside the {pathType} path: {rootPath}");
                return;
            }

            // Calculate relative path from contents folder
            if (!contentsFolderPath.EndsWith("/"))
            {
                contentsFolderPath += "/";
            }

            string relativePath;
            if (selectedPath.StartsWith(contentsFolderPath))
            {
                relativePath = "/" + selectedPath.Substring(contentsFolderPath.Length);
            }
            else
            {
                string fromDir = contentsFolderPath.TrimEnd('/');
                relativePath = "/" + GetRelativePath(fromDir, selectedPath);
            }

            Undo.RecordObject(serializedObject.targetObject, $"Change {label}");
            prop.stringValue = relativePath;
            serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(serializedObject.targetObject);
            EditorUtility.SetDirty(serializedObject.targetObject);
            Debug.Log($"[{label}] selected: {relativePath}");
        }

        /// <summary>
        /// Parses the amproj file and returns indicator asset paths.
        /// Returns null if version >= 3, file not found, or parsing fails.
        /// </summary>
        public static AMProjIndicatorInfo ParseAMProjIndicators()
        {
            // v3 이상이면 indicator 경로가 amproj에 포함되지 않음
            int version = GetAMProjVersion();
            if (version < 1 || version >= 3)
                return null;

            ARPlayGround arPlayGround = Object.FindFirstObjectByType<ARPlayGround>();
            if (arPlayGround == null)
                return null;

            string amprojPath = arPlayGround.amprojFilePath;
            if (string.IsNullOrEmpty(amprojPath) || !System.IO.File.Exists(amprojPath))
                return null;

            try
            {
                string json = System.IO.File.ReadAllText(amprojPath);
                var root = Newtonsoft.Json.Linq.JObject.Parse(json);

                var appendices = root["appendices"] as Newtonsoft.Json.Linq.JArray;
                if (appendices == null || appendices.Count == 0)
                    return null;

                var indicatorConfigs = appendices[0]["indicatorConfigs"] as Newtonsoft.Json.Linq.JArray;
                if (indicatorConfigs == null || indicatorConfigs.Count == 0)
                    return null;

                var config = indicatorConfigs[0];
                var info = new AMProjIndicatorInfo();

                // Parse turnspot
                var turnspot = config["turnspot"];
                if (turnspot != null)
                {
                    var models = turnspot["models"] as Newtonsoft.Json.Linq.JArray;
                    if (models != null)
                    {
                        foreach (var model in models)
                        {
                            int type = (int?)model["type"] ?? -1;
                            string asset = (string)model["asset"];
                            if (!string.IsNullOrEmpty(asset))
                            {
                                info.TurnSpotAssets[type] = "/" + asset;
                            }
                        }
                    }
                }

                // Parse nextstep
                var nextstep = config["nextstep"];
                if (nextstep != null)
                {
                    var models = nextstep["models"] as Newtonsoft.Json.Linq.JArray;
                    if (models != null)
                    {
                        foreach (var model in models)
                        {
                            string name = (string)model["name"];
                            string asset = (string)model["asset"];
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(asset))
                            {
                                info.NextStepAssets[name] = "/" + asset;
                            }
                        }
                    }
                }

                return info;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static string GetRootPath(ARPlayGround arPlayGround)
        {
            if (arPlayGround.StreamingAssets)
            {
                return Application.streamingAssetsPath;
            }
            else
            {
                return Application.persistentDataPath;
            }
        }

        private static string GetRelativePath(string fromDir, string toFile)
        {
            string[] fromParts = fromDir.Split('/');
            string[] toParts = toFile.Split('/');

            int commonLength = 0;
            for (int i = 0; i < fromParts.Length && i < toParts.Length; i++)
            {
                if (fromParts[i] == toParts[i])
                    commonLength++;
                else
                    break;
            }

            var result = new System.Text.StringBuilder();
            for (int i = commonLength; i < fromParts.Length; i++)
            {
                result.Append("../");
            }
            for (int i = commonLength; i < toParts.Length; i++)
            {
                if (i > commonLength)
                    result.Append("/");
                result.Append(toParts[i]);
            }

            return result.ToString();
        }

        private static int s_CachedAMProjVersion = -1;
        private static string s_CachedAmprojPath;

        /// <summary>
        /// Invalidates the cached amproj version so it will be re-read on the next call to GetAMProjVersion().
        /// </summary>
        public static void InvalidateCache()
        {
            s_CachedAmprojPath = null;
            s_CachedAMProjVersion = -1;
        }

        /// <summary>
        /// Returns the amproj version for the current ARPlayGround configuration.
        /// Caches the result and only re-reads when the amproj path changes.
        /// Returns -1 if ARPlayGround is not found, path is invalid, or parsing fails.
        /// </summary>
        public static int GetAMProjVersion()
        {
            ARPlayGround arPlayGround = Object.FindFirstObjectByType<ARPlayGround>();
            if (arPlayGround == null)
            {
                s_CachedAmprojPath = null;
                s_CachedAMProjVersion = -1;
                return -1;
            }

            string amprojPath = arPlayGround.amprojFilePath;
            if (amprojPath == s_CachedAmprojPath)
                return s_CachedAMProjVersion;

            s_CachedAmprojPath = amprojPath;

            if (string.IsNullOrEmpty(amprojPath) || !System.IO.File.Exists(amprojPath))
            {
                s_CachedAMProjVersion = -1;
                return -1;
            }

            try
            {
                string json = System.IO.File.ReadAllText(amprojPath);
                var root = Newtonsoft.Json.Linq.JObject.Parse(json);
                s_CachedAMProjVersion = (int?)root["version"] ?? 1;
            }
            catch
            {
                s_CachedAMProjVersion = -1;
            }

            return s_CachedAMProjVersion;
        }
    }

    public class AMProjIndicatorInfo
    {
        // v1/v2 amproj always locks indicator paths
        public bool IsLocked = true;

        // key: type (0=left, 1=right, 2=straight, 3=up, 4=down, 5=destination)
        public Dictionary<int, string> TurnSpotAssets = new Dictionary<int, string>();
        // key: name ("arrow", "dot", "text")
        public Dictionary<string, string> NextStepAssets = new Dictionary<string, string>();

        /// <summary>
        /// Returns the asset path for the given turnspot type.
        /// When locked (v1/v2), returns empty string if not found (still locked).
        /// When not locked, returns null (Browse enabled).
        /// </summary>
        public string GetTurnSpotAsset(int type)
        {
            if (TurnSpotAssets.TryGetValue(type, out string path))
                return path;
            return IsLocked ? "" : null;
        }

        /// <summary>
        /// Returns the asset path for the given nextstep name.
        /// When locked (v1/v2), returns empty string if not found (still locked).
        /// When not locked, returns null (Browse enabled).
        /// </summary>
        public string GetNextStepAsset(string name)
        {
            if (NextStepAssets.TryGetValue(name, out string path))
                return path;
            return IsLocked ? "" : null;
        }
    }
}
