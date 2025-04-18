using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

namespace ARCeye
{
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;

            EditorGUI.BeginProperty(position, label, property);

            var layerNameProp = property.FindPropertyRelative("layerName");
            var linkToStageProp = property.FindPropertyRelative("linkToStage");                                                       
            var stageNameProp = property.FindPropertyRelative("stageName");
            var subLayerProp  = property.FindPropertyRelative("subLayers");

            string layerName = layerNameProp.stringValue;
            bool linkToStage = linkToStageProp.boolValue;

            EditorGUI.PrefixLabel(position, new GUIContent($"Layer - {layerName}"));
            position.y += lineHeight;

            {
                EditorGUI.indentLevel++;

                float fullWidth = EditorGUIUtility.currentViewWidth - position.x - 10;

                Rect layerNameRect = new Rect(position.x, position.y, fullWidth, EditorGUIUtility.singleLineHeight);
                position.y += lineHeight;

                Rect linkToStageRect = new Rect(position.x, position.y, fullWidth, EditorGUIUtility.singleLineHeight);
                position.y += lineHeight;

                EditorGUI.PropertyField(layerNameRect, layerNameProp, new GUIContent("Layer Name"));
                EditorGUI.PropertyField(linkToStageRect, linkToStageProp, new GUIContent("Link to Stage"));

                if(linkToStage)
                {
                    Rect stageNameRect = new Rect(position.x, position.y, fullWidth, EditorGUIUtility.singleLineHeight);
                    position.y += lineHeight;

                    EditorGUI.PropertyField(stageNameRect, stageNameProp, new GUIContent("Stage Name"));
                }
                else
                {
                    Rect subLayerRect  = new Rect(position.x, position.y, fullWidth, EditorGUIUtility.singleLineHeight);
                    position.y += lineHeight;

                    EditorGUI.PropertyField(subLayerRect, subLayerProp, new GUIContent("Sub Layer"));
                    EditorUtility.SetDirty(subLayerProp.serializedObject.targetObject);
                }
                
                EditorGUI.indentLevel--;
            }

            subLayerProp.serializedObject.ApplyModifiedProperties();
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }
}