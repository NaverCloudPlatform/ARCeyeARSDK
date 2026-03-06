using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.U2D;
#endif

namespace ARCeye
{
    /// <summary>
    /// ARSDK Project Validation Rule
    /// </summary>
    public class ARSDKValidationRule
    {
        public string message;
        public string category;
        public Func<bool> checkPredicate;
        public Action fixIt;
        public string fixItMessage;
        public bool fixItAutomatic;
        public BuildTargetGroup[] buildTargetGroup;
        public bool error;
        public string settingsPath; // Settings page path for Edit button

        public ARSDKValidationRule()
        {
            error = true;
            fixItAutomatic = false;
            buildTargetGroup = null;
            settingsPath = "Project/Player";
        }

        public bool CanFix => fixIt != null;
    }

    /// <summary>
    /// ARSDK Project Validation System
    /// </summary>
    public static class ARSDKProjectValidation
    {
        private static List<ARSDKValidationRule> s_ValidationRules;

        public static List<ARSDKValidationRule> GetRules()
        {
            if (s_ValidationRules == null)
            {
                s_ValidationRules = new List<ARSDKValidationRule>();
                BuildValidationRules();
            }
            return s_ValidationRules;
        }

        private static void BuildValidationRules()
        {
            s_ValidationRules.Clear();

            // Define Symbol: ARSDK_GLTFAST
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "ARSDK_GLTFAST must be defined in Scripting Define Symbols",
                category = "Scripting Define Symbols",
                checkPredicate = () => IsDefineSymbolSet("ARSDK_GLTFAST"),
                fixIt = () => AddDefineSymbol("ARSDK_GLTFAST"),
                fixItMessage = "Add ARSDK_GLTFAST to Scripting Define Symbols",
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            // Scene Components Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "ARPlayGround must be added to the scene",
                category = "Scene Setup",
                checkPredicate = () => IsComponentInScene<ARPlayGround>(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "ItemGenerator must be added to the scene",
                category = "Scene Setup",
                checkPredicate = () => IsComponentInScene<ItemGenerator>(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "MapCameraController must be added to the scene",
                category = "Scene Setup",
                checkPredicate = () => IsComponentInScene<MapCameraController>(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "NextStep must be added to the scene",
                category = "Scene Setup",
                checkPredicate = () => IsComponentInScene<NextStep>(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            // Contents Path Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "Contents Path must be set in ARPlayGround",
                category = "ARSDK Configuration",
                checkPredicate = () => IsContentsPathSet(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            // StreamingAssets Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "StreamingAssets folder must exist when StreamingAssets is enabled",
                category = "ARSDK Configuration",
                checkPredicate = () => IsStreamingAssetsValid(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });

            // Input System Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "Active Input Handling should be set to 'Both' for better compatibility",
                category = "Input System",
                checkPredicate = () => IsInputSystemSetToBoth(),
                fixIt = () => SetInputSystemToBoth(),
                fixItMessage = "Set Active Input Handling to 'Both'",
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = false
            });

            // Sprite Atlas Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "Sprite Atlas should be set to 'Sprite Atlas V2 - Enabled'",
                category = "Graphics",
                checkPredicate = () => IsSpriteAtlasV2Enabled(),
                fixIt = () => SetSpriteAtlasV2(),
                fixItMessage = "Set Sprite Atlas to 'Sprite Atlas V2 - Enabled'",
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = false
            });

            // TextMeshPro Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "TextMeshPro Essential Assets should be imported",
                category = "TextMeshPro",
                checkPredicate = () => IsTextMeshProEssentialsImported(),
                fixIt = () => ImportTextMeshProEssentials(),
                fixItMessage = "Import TextMeshPro Essential Resources",
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = false
            });

            // URP MapCamera Validation
            s_ValidationRules.Add(new ARSDKValidationRule
            {
                message = "When using URP, MapCamera must have URP rendering components",
                category = "Rendering",
                checkPredicate = () => IsMapCameraValidForURP(),
                fixIt = null,
                fixItMessage = null,
                fixItAutomatic = false,
                buildTargetGroup = null,
                error = true
            });
        }

        private static bool IsDefineSymbolSet(string symbol)
        {
            NamedBuildTarget[] namedTargets = new[] { NamedBuildTarget.Android, NamedBuildTarget.iOS };

            foreach (var namedTarget in namedTargets)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
                var symbolList = defines.Split(';').Select(s => s.Trim()).ToList();

                if (!symbolList.Contains(symbol))
                {
                    return false;
                }
            }

            return true;
        }

        private static void AddDefineSymbol(string symbol)
        {
            NamedBuildTarget[] namedTargets = new[] { NamedBuildTarget.Android, NamedBuildTarget.iOS };

            foreach (var namedTarget in namedTargets)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
                var symbolList = defines.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

                if (!symbolList.Contains(symbol))
                {
                    symbolList.Add(symbol);
                    string newDefines = string.Join(";", symbolList);
                    PlayerSettings.SetScriptingDefineSymbols(namedTarget, newDefines);
                    Debug.Log($"{symbol} has been added to Scripting Define Symbols for {namedTarget}.");
                }
            }
        }

        private static bool IsComponentInScene<T>() where T : UnityEngine.Object
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
            {
                return true; // Skip validation if no scene is loaded
            }

            var component = UnityEngine.Object.FindObjectOfType<T>();
            return component != null;
        }

        private static bool IsContentsPathSet()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
            {
                return true; // Skip validation if no scene is loaded
            }

            var arPlayGround = UnityEngine.Object.FindObjectOfType<ARPlayGround>();
            if (arPlayGround == null)
            {
                return true; // Skip if ARPlayGround is not in scene
            }

            return !string.IsNullOrEmpty(arPlayGround.ContentsPath);
        }

        private static bool IsStreamingAssetsValid()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
            {
                return true; // Skip validation if no scene is loaded
            }

            var arPlayGround = UnityEngine.Object.FindObjectOfType<ARPlayGround>();
            if (arPlayGround == null)
            {
                return true; // Skip if ARPlayGround is not in scene
            }

            // Check only if StreamingAssets is enabled
            if (arPlayGround.StreamingAssets)
            {
                string streamingAssetsPath = Application.streamingAssetsPath;
                return Directory.Exists(streamingAssetsPath);
            }

            return true;
        }

        private static bool IsInputSystemSetToBoth()
        {
            try
            {
                // Access PlayerSettings via SerializedObject
                var playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
                if (playerSettings == null || playerSettings.Length == 0)
                {
                    Debug.LogWarning("[ARSDK Validation] Could not load ProjectSettings.asset");
                    return true;
                }

                var serializedObject = new SerializedObject(playerSettings[0]);
                var activeInputHandlerProperty = serializedObject.FindProperty("activeInputHandler");

                if (activeInputHandlerProperty == null)
                {
                    Debug.LogWarning("[ARSDK Validation] Could not find activeInputHandler property in ProjectSettings.");
                    // Try to list all properties for debugging
                    SerializedProperty iterator = serializedObject.GetIterator();
                    Debug.Log("[ARSDK Validation] Available properties in ProjectSettings:");
                    int count = 0;
                    while (iterator.NextVisible(true) && count < 20)
                    {
                        if (iterator.name.ToLower().Contains("input"))
                        {
                            Debug.Log($"  - {iterator.name} = {iterator.intValue}");
                        }
                        count++;
                    }
                    return true;
                }

                // 0 = Legacy (Old), 1 = InputSystemPackage (New), 2 = Both
                int currentValue = activeInputHandlerProperty.intValue;

                // Only "Both" (value = 2) should pass validation
                bool isValid = (currentValue == 2);

                if (!isValid)
                {
                    string[] inputHandlerNames = { "Legacy", "InputSystemPackage (New)", "Both" };
                    string currentName = currentValue >= 0 && currentValue < inputHandlerNames.Length
                        ? inputHandlerNames[currentValue]
                        : "Unknown";
                    Debug.Log($"[ARSDK Validation] Current Active Input Handling: {currentName} (value: {currentValue}). Expected: Both (2)");
                }

                return isValid;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ARSDK Validation] Failed to check Active Input Handling: {e.Message}\n{e.StackTrace}");
                return true; // Skip validation if check fails
            }
        }

        private static void SetInputSystemToBoth()
        {
            try
            {
                // Access PlayerSettings via SerializedObject
                var playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
                if (playerSettings == null || playerSettings.Length == 0)
                {
                    Debug.LogWarning("[ARSDK Validation] Could not load ProjectSettings.asset");
                    return;
                }

                var serializedObject = new SerializedObject(playerSettings[0]);
                var activeInputHandlerProperty = serializedObject.FindProperty("activeInputHandler");

                if (activeInputHandlerProperty == null)
                {
                    Debug.LogWarning("[ARSDK Validation] Could not find activeInputHandler property. This Unity version may not support this setting.");
                    return;
                }

                // Store original value in case user cancels
                int originalValue = activeInputHandlerProperty.intValue;

                // Set to "Both" (value = 2)
                activeInputHandlerProperty.intValue = 2;
                serializedObject.ApplyModifiedProperties();

                // Show restart dialog
                bool restart = EditorUtility.DisplayDialog(
                    "Unity editor restart required",
                    "The Unity editor must be restarted for this change to take effect.\nCancel to revert changes.",
                    "Apply",
                    "Cancel"
                );

                if (restart)
                {
                    // Restart Unity Editor
                    EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
                }
                else
                {
                    // Revert changes
                    activeInputHandlerProperty.intValue = originalValue;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log("[ARSDK Validation] Active Input Handling change reverted.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ARSDK Validation] Failed to set Active Input Handling: {e.Message}");
            }
        }

        private static bool IsSpriteAtlasV2Enabled()
        {
#if UNITY_2020_2_OR_NEWER
            var spritePackerMode = EditorSettings.spritePackerMode;
            return spritePackerMode == SpritePackerMode.SpriteAtlasV2;
#else
            return true; // Skip for older Unity versions
#endif
        }

        private static void SetSpriteAtlasV2()
        {
#if UNITY_2020_2_OR_NEWER
            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;
            Debug.Log("Sprite Atlas has been set to 'Sprite Atlas V2 - Enabled'.");
#endif
        }

        private static bool IsTextMeshProEssentialsImported()
        {
            // Check if TMP_Settings exists in the project
            string[] guids = AssetDatabase.FindAssets("t:TMP_Settings");
            if (guids.Length == 0)
            {
                return false;
            }

            // Check if Essential resources exist
            string[] essentialFolders = AssetDatabase.FindAssets("TextMesh Pro", new[] { "Assets" });
            return essentialFolders.Length > 0;
        }

        private static void ImportTextMeshProEssentials()
        {
            try
            {
                // Try to find TMP_PackageResourceImporter using reflection
                var tmpPackageResourceImporterType = System.Type.GetType("TMPro.EditorUtilities.TMP_PackageResourceImporter, Unity.TextMeshPro.Editor");

                if (tmpPackageResourceImporterType == null)
                {
                    // Fallback: Try to find the menu item method
                    var tmpMenuItemsType = System.Type.GetType("TMPro.EditorUtilities.TMPro_ResourceImporterWindow, Unity.TextMeshPro.Editor");
                    if (tmpMenuItemsType != null)
                    {
                        var method = tmpMenuItemsType.GetMethod("ShowPackageResourceImporterWindow",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (method != null)
                        {
                            method.Invoke(null, null);
                            Debug.Log("[ARSDK Validation] TextMeshPro Resource Importer window opened. Please import Essential Resources.");
                            return;
                        }
                    }

                    Debug.LogWarning("[ARSDK Validation] Could not find TextMeshPro Resource Importer. Please import Essential Resources manually from Window > TextMeshPro > Import TMP Essential Resources.");
                    return;
                }

                // Try to import essentials automatically
                var importEssentialsMethod = tmpPackageResourceImporterType.GetMethod("ImportProjectResourcesEssentials",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (importEssentialsMethod != null)
                {
                    importEssentialsMethod.Invoke(null, null);
                    Debug.Log("[ARSDK Validation] TextMeshPro Essential Resources have been imported.");
                    AssetDatabase.Refresh();
                }
                else
                {
                    // Try to open the resource importer window
                    var showWindowMethod = tmpPackageResourceImporterType.GetMethod("ShowPackageResourceImporterWindow",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    if (showWindowMethod != null)
                    {
                        showWindowMethod.Invoke(null, null);
                        Debug.Log("[ARSDK Validation] TextMeshPro Resource Importer window opened. Please import Essential Resources.");
                    }
                    else
                    {
                        Debug.LogWarning("[ARSDK Validation] Could not find import method. Please import Essential Resources manually from Window > TextMeshPro > Import TMP Essential Resources.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ARSDK Validation] Failed to import TextMeshPro Essential Resources: {e.Message}\nPlease import manually from Window > TextMeshPro > Import TMP Essential Resources.");
            }
        }

        private static bool IsMapCameraValidForURP()
        {
            // Check if URP is being used
            var renderPipeline = GraphicsSettings.currentRenderPipeline;
            if (renderPipeline == null)
            {
                return true; // Not using URP, skip validation
            }

            string pipelineName = renderPipeline.GetType().Name;
            if (!pipelineName.Contains("Universal"))
            {
                return true; // Not using URP, skip validation
            }

            // If using URP, check if MapCamera has proper URP setup
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
            {
                return true; // Skip validation if no scene is loaded
            }

            var mapCameraController = UnityEngine.Object.FindObjectOfType<MapCameraController>();
            if (mapCameraController == null)
            {
                return true; // Skip if MapCameraController is not in scene
            }

            // Check if MapCamera has URP-compatible settings
            var mapCamera = mapCameraController.GetComponentInChildren<MapCamera>();
            if (mapCamera == null)
            {
                return false;
            }

            var camera = mapCamera.GetComponent<Camera>();
            if (camera == null)
            {
                return false;
            }

            // Check if camera has URP additional camera data component
#if UNITY_2019_3_OR_NEWER
            var additionalCameraData = camera.GetComponent("UniversalAdditionalCameraData");
            return additionalCameraData != null;
#else
            return true;
#endif
        }

        public static void RefreshRules()
        {
            s_ValidationRules = null;
            GetRules();
        }
    }
}
