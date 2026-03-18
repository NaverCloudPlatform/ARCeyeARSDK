using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    [CustomPropertyDrawer(typeof(POIIconInfoItem))]
    public class IntStringItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty dpCodeProperty = property.FindPropertyRelative("dpCode");
            SerializedProperty iconProperty = property.FindPropertyRelative("icon");

            float fieldWidth = position.width / 2 - 5;
            Rect idRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect nameRect = new Rect(position.x + fieldWidth + 10, position.y, fieldWidth, position.height);

            EditorGUI.PropertyField(idRect, dpCodeProperty, GUIContent.none);
            EditorGUI.PropertyField(nameRect, iconProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(POIIconInfo))]
    public class POIIconInfoEditor : Editor
    {
        private SerializedProperty m_POIAtlasProp;
        private SerializedProperty m_DefaultIconProp;
        private SerializedProperty m_POIInfoListProp;
        private SerializedProperty m_POIIconDirectoryProp;

        private void OnEnable()
        {
            m_POIAtlasProp = serializedObject.FindProperty("m_POIAtlas");
            m_DefaultIconProp = serializedObject.FindProperty("m_DefaultIcon");
            m_POIInfoListProp = serializedObject.FindProperty("m_POIInfoList");
            m_POIIconDirectoryProp = serializedObject.FindProperty("m_POIIconDirectory");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_POIAtlasProp);
            EditorGUILayout.PropertyField(m_DefaultIconProp);
            EditorGUILayout.PropertyField(m_POIInfoListProp);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Auto Generation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_POIIconDirectoryProp, new GUIContent("POI Icon Directory"));

            if (GUILayout.Button("Generate"))
            {
                GeneratePOIInfoList();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void GeneratePOIInfoList()
        {
            POIIconInfo poiIconInfo = (POIIconInfo)target;

            if (poiIconInfo.POIIconDirectory == null)
            {
                EditorUtility.DisplayDialog("Error", "POI Icon Directory is not assigned.", "OK");
                return;
            }

            string directoryPath = AssetDatabase.GetAssetPath(poiIconInfo.POIIconDirectory);

            if (string.IsNullOrEmpty(directoryPath) || !System.IO.Directory.Exists(directoryPath))
            {
                EditorUtility.DisplayDialog("Error", "Invalid directory path.", "OK");
                return;
            }

            poiIconInfo.POIInfoList.Clear();

            string[] pngFiles = System.IO.Directory.GetFiles(directoryPath, "*.png", System.IO.SearchOption.AllDirectories);

            foreach (string pngFile in pngFiles)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(pngFile);
                int dpCode = -1;

                // Try to extract dpcode from filename
                if (TryExtractDpCode(fileName, out dpCode))
                {
                    string assetPath = pngFile.Replace("\\", "/");
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                    if (sprite != null)
                    {
                        POIIconInfoItem item = new POIIconInfoItem
                        {
                            dpCode = dpCode,
                            icon = sprite
                        };
                        poiIconInfo.POIInfoList.Add(item);
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not extract dpcode from filename: {pngFile}");
                }
            }

            EditorUtility.SetDirty(poiIconInfo);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Initialized {poiIconInfo.POIInfoList.Count} POI icons.", "OK");
        }

        private bool TryExtractDpCode(string fileName, out int dpCode)
        {
            dpCode = -1;

            // Try pattern: "dpcode" (entire filename is dpcode)
            if (int.TryParse(fileName, out dpCode))
            {
                return true;
            }

            // Try pattern: "이미지이름_dpcode"
            int underscoreIndex = fileName.LastIndexOf('_');
            if (underscoreIndex >= 0 && underscoreIndex < fileName.Length - 1)
            {
                string potentialDpCode = fileName.Substring(underscoreIndex + 1);
                if (int.TryParse(potentialDpCode, out dpCode))
                {
                    return true;
                }
            }

            // Try pattern: "dpcode_이미지이름"
            underscoreIndex = fileName.IndexOf('_');
            if (underscoreIndex >= 0)
            {
                string potentialDpCode = fileName.Substring(0, underscoreIndex);
                if (int.TryParse(potentialDpCode, out dpCode))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class POIIconInfoGenerator
    {
        [MenuItem("Assets/Create/ARC eye/POIIconInfo", priority = 10)]
        public static void CreateLayerInfoSetting()
        {
            POIIconInfo asset = ScriptableObject.CreateInstance<POIIconInfo>();

            AssetDatabase.CreateAsset(asset, "Assets/POIIconInfo.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}
