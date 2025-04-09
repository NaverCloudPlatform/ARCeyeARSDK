using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ARCeye
{
    [Serializable]
    public class POIIconInfoItem
    {
        public int dpCode;
        public Sprite icon;
    }

    [Serializable]
    public class POIIconInfo : ScriptableObject
    {
        [SerializeField]
        private SpriteAtlas m_POIAtlas;

        [SerializeField]
        private Sprite m_DefaultIcon;

        [SerializeField]
        private List<POIIconInfoItem> m_POIInfoList;

        
        public Sprite GetSprite(int dpCode)
        {
            POIIconInfoItem item = m_POIInfoList.Find(e => e.dpCode == dpCode);

            if(item != null)
            {
                string iconName = item.icon.name;
                return m_POIAtlas.GetSprite(iconName);
            }
            else
            {
                return m_DefaultIcon;
            }
        }
    }

#if UNITY_EDITOR
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

    public static class POIIconInfoGenerator
    {
        [MenuItem("Assets/Create/ARCeye/POIIconInfo")]
        public static void CreateLayerInfoSetting()
        {
            POIIconInfo asset = ScriptableObject.CreateInstance<POIIconInfo>();

            AssetDatabase.CreateAsset(asset, "Assets/POIIconInfo.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
#endif
}