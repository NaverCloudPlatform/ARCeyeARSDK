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

            AssetDatabase.CreateAsset(asset, "Assets/ARPG/Core/LayerInfoSetting.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}