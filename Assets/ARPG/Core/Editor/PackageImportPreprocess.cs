using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public class PackageImportPreprocess
{
    static PackageImportPreprocess()
    {
        // Layer 체크.
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        string[] layerNames = {"Map", "MapPOI", "MapArrow", "AMProjViz"};
        
        foreach (string layerName in layerNames)
        {
            bool layerExist = false;
            for (int i = 5; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                if (layerSP.stringValue == layerName)
                {
                    layerExist = true;
                    break;
                }
            }

            if (!layerExist)
            {
                for (int j = 5; j < layersProp.arraySize; j++)
                {
                    SerializedProperty newLayer = layersProp.GetArrayElementAtIndex(j);
                    if (newLayer.stringValue == "")
                    {
                        newLayer.stringValue = layerName;
                        tagManager.ApplyModifiedProperties();
                        break;
                    }
                }
            }
        }



        // Always included shader 체크.
        string[] guids = AssetDatabase.FindAssets("ARPG t:shader");

        if (guids.Length == 0)
        {
            UnityEngine.Debug.LogWarning("ARPG 쉐이더 경로를 찾을 수 없습니다. Always Included Shaders에 직접 추가해주세요");
            return;
        }

        SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        SerializedProperty alwaysIncludedShaders = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");

        int shadersCount = alwaysIncludedShaders.arraySize;

        List<string> shaderNames = new List<string>();

        for(int i=0 ; i<shadersCount ; i++)
        {
            Object obj = alwaysIncludedShaders.GetArrayElementAtIndex(i).objectReferenceValue;
            shaderNames.Add(obj.name);
        }

        foreach (string guid in guids)
        {
            string shaderPath = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            string shaderName = shader.name;

            if(shaderNames.Contains(shaderName))
            {
                continue;
            }

            alwaysIncludedShaders.InsertArrayElementAtIndex(alwaysIncludedShaders.arraySize);
            alwaysIncludedShaders.GetArrayElementAtIndex(alwaysIncludedShaders.arraySize - 1).objectReferenceValue = shader;
        }

        // Unity 기본 쉐이더를 찾아서 추가.
        var unlitColorShader = Shader.Find("Unlit/Color");
        if (unlitColorShader == null)
        {
            Debug.LogError("Shader 'Unlit/Color' not found");
            return;
        }

        bool shaderAlreadyIncluded = false;
        for (int i = 0; i < alwaysIncludedShaders.arraySize; i++)
        {
            var shaderElement = alwaysIncludedShaders.GetArrayElementAtIndex(i);
            if (shaderElement.objectReferenceValue == unlitColorShader)
            {
                shaderAlreadyIncluded = true;
                break;
            }
        }

        if (!shaderAlreadyIncluded)
        {
            alwaysIncludedShaders.arraySize++;
            alwaysIncludedShaders.GetArrayElementAtIndex(alwaysIncludedShaders.arraySize - 1).objectReferenceValue = unlitColorShader;
        }

        graphicsSettings.ApplyModifiedProperties();
    }
}