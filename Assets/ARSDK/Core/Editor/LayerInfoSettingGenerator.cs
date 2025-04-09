using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye
{
    public class LayerInfoSettingGenerator
    {
        [MenuItem("Assets/Create/ARCeye/LayerInfoSetting")]
        public static void CreateLayerInfoSetting()
        {
            LayerInfoSetting asset = ScriptableObject.CreateInstance<LayerInfoSetting>();

            string path = GetSelectedDirectoryPath();
            string assetPathAndName = GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        private static string GetSelectedDirectoryPath()
        {
            string path = "Assets";
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (Selection.activeObject != null)
            {
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (File.Exists(selectedPath))
                    {
                        selectedPath = Path.GetDirectoryName(selectedPath);
                    }

                    path = selectedPath;
                }
            }

            return path;
        }

        private static string GenerateUniqueAssetPath(string path)
        {
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(path, "New LayerInfoSetting.asset")
            );

            return assetPathAndName;
        }
    }
}