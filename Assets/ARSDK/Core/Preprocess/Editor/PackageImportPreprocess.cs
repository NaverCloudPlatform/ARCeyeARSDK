using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public class PackageImportPreprocess
{
    private static Dictionary<string, string> packagesToAdd = new Dictionary<string, string>()
    {
        { "com.unity.nuget.newtonsoft-json", "3.2.1" },
        { "com.unity.cloud.gltfast", "6.9.0" },
        { "com.unity.cloud.draco", "5.1.8" },
    };

    private static Dictionary<string, string> packagesToDefineSymbols = new Dictionary<string, string>()
    {
        { "com.unity.cloud.gltfast", "ARSDK_GLTFAST" },
        { "com.unity.inputsystem", "ARSDK_INPUT_SYSTEM" }
    };

    static PackageImportPreprocess()
    {
        AddPackagesToManifest();

        // Layer 체크.
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        string[] layerNames = { "Map", "MapPOI", "MapArrow", "AMProjViz", "ARItem" };

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

        SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        SerializedProperty alwaysIncludedShaders = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");

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


        AddDefineSymbols();
    }

    private static void AddPackagesToManifest()
    {
        // manifest.json 파일 경로
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

        if (!File.Exists(manifestPath))
        {
            Debug.LogError("Cannot find manifest.json file");
            return;
        }

        string manifestJson = File.ReadAllText(manifestPath);
        string dependenciesBlock = GetDependenciesBlock(manifestJson);

        Dictionary<string, string> dependencies = ParseDependencies(dependenciesBlock);

        bool manifestChanged = false;
        foreach (var package in packagesToAdd)
        {
            if (!dependencies.ContainsKey(package.Key))
            {
                dependencies[package.Key] = package.Value;
                manifestChanged = true;
                // Debug.Log($"{package} package is added");
            }
            else
            {
                // Debug.Log($"{package} package is already installed");
            }
        }

        if (manifestChanged)
        {
            string updatedManifest = UpdateManifestJson(manifestJson, dependencies);
            File.WriteAllText(manifestPath, updatedManifest);

            // 패키지 매니저 리프레시
            AssetDatabase.Refresh();
        }
    }

    private static string GetDependenciesBlock(string manifestJson)
    {
        int startIndex = manifestJson.IndexOf("\"dependencies\": {");
        if (startIndex == -1)
        {
            Debug.LogError("Failed to find dependencies block in manifest.json");
            return null;
        }

        int endIndex = manifestJson.IndexOf("}", startIndex);
        return manifestJson.Substring(startIndex, endIndex - startIndex + 1);
    }

    private static Dictionary<string, string> ParseDependencies(string dependenciesBlock)
    {
        var dependencies = new Dictionary<string, string>();

        // JSON 형식에서 key-value 추출
        string[] lines = dependenciesBlock.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.Contains(":"))
            {
                string[] keyValue = line.Split(new[] { ':' }, 2);
                string key = keyValue[0].Trim().Replace("\"", "");
                string value = keyValue[1].Trim().Replace("\"", "").Replace(",", "");

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value) && key != "dependencies")
                {
                    dependencies[key] = value;
                }
            }
        }

        return dependencies;
    }

    private static string UpdateManifestJson(string manifestJson, Dictionary<string, string> dependencies)
    {
        // 새로운 dependencies 블록 만들기
        string newDependenciesBlock = "\"dependencies\": {\n";
        foreach (var kvp in dependencies)
        {
            newDependenciesBlock += $"    \"{kvp.Key}\": \"{kvp.Value}\",\n";
        }
        newDependenciesBlock = newDependenciesBlock.TrimEnd(',', '\n') + "\n  }";

        // 기존 dependencies 블록 대체
        string oldDependenciesBlock = GetDependenciesBlock(manifestJson);
        string updatedManifest = manifestJson.Replace(oldDependenciesBlock, newDependenciesBlock);
        return updatedManifest;
    }

    private static void AddDefineSymbols()
    {
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        string manifestJson = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";

        foreach (var package in packagesToDefineSymbols)
        {
            if (!manifestJson.Contains(package.Key))
                continue;

            string defineSymbol = package.Value;
            AddDefineSymbol(defineSymbol, NamedBuildTarget.Standalone);
            AddDefineSymbol(defineSymbol, NamedBuildTarget.iOS);
            AddDefineSymbol(defineSymbol, NamedBuildTarget.Android);
        }
    }

    private static void AddDefineSymbol(string defineSymbol, NamedBuildTarget target)
    {
        PlayerSettings.GetScriptingDefineSymbols(target, out string[] symbolArray);
        var symbols = string.Join(";", symbolArray);

        // Define Symbol이 이미 포함되어 있는지 확인
        if (!symbols.Contains(defineSymbol))
        {
            symbols += ";" + defineSymbol;
            PlayerSettings.SetScriptingDefineSymbols(target, symbols);
        }
    }

    private static IEnumerable<BuildTargetGroup> GetBuildTargetGroups()
    {
        foreach (BuildTargetGroup group in (BuildTargetGroup[])System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            // Unity가 지원하는 BuildTargetGroup만 반환
            if (group != BuildTargetGroup.Unknown && !IsObsolete(group))
            {
                yield return group;
            }
        }
    }

    private static bool IsObsolete(BuildTargetGroup group)
    {
        var attributes = typeof(BuildTargetGroup).GetField(group.ToString())
            .GetCustomAttributes(typeof(System.ObsoleteAttribute), false);
        return attributes.Length > 0;
    }
}