// Based on glTFast glTFPbrMetallicRoughness shader source. Copyright (c) 2020 Andreas Atteneder. Apache License

Shader "glTF/PbrMetallicRoughness (2Pass)"
{
    Properties
    {
        [HideInInspector] _MainTex ("My Texture", 2D) = "white" { }
        [MainColor] baseColorFactor("Base Color", Color) = (1,1,1,1)
        [MainTexture] baseColorTexture("Base Color Tex", 2D) = "white" {}
        baseColorTexture_Rotation ("Base Color Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] baseColorTexture_texCoord ("Base Color Tex UV", Float) = 0
        
        alphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        roughnessFactor("Roughness", Range(0.0, 1.0)) = 1
        
        [Gamma] metallicFactor("Metallic", Range(0.0, 1.0)) = 1.0
        metallicRoughnessTexture("Metallic-Roughness Tex", 2D) = "white" {}
        metallicRoughnessTexture_Rotation ("Metallic-Roughness Map Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] metallicRoughnessTexture_texCoord ("Metallic-Roughness Tex UV", Float) = 0

        normalTexture_scale("Normal Scale", Float) = 1.0
        [Normal] normalTexture("Normal Tex", 2D) = "bump" {}
        normalTexture_Rotation ("Normal Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] normalTexture_texCoord ("Normal Tex UV", Float) = 0

        occlusionTexture_strength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        occlusionTexture("Occlusion Tex", 2D) = "white" {}
        occlusionTexture_Rotation ("Occlusion Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] occlusionTexture_texCoord ("Occlusion Tex UV", Float) = 0
        
        [HDR] emissiveFactor("Emissive", Color) = (0,0,0)
        emissiveTexture("Emission Tex", 2D) = "white" {}
        emissiveTexture_Rotation ("Emission Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] emissiveTexture_texCoord ("Emission Tex UV", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        zwrite on
        Pass { 
            ColorMask 0 
        }

        zwrite off
        UsePass "glTF/PbrMetallicRoughness/FORWARD"
        UsePass "glTF/PbrMetallicRoughness/FORWARD_DELTA"
        UsePass "glTF/PbrMetallicRoughness/ShadowCaster"
        UsePass "glTF/PbrMetallicRoughness/DEFERRED"
        UsePass "glTF/PbrMetallicRoughness/META"
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        zwrite on
        Pass { 
            ColorMask 0 
        }

        zwrite off
        UsePass "glTF/PbrMetallicRoughness/FORWARD"
        UsePass "glTF/PbrMetallicRoughness/FORWARD_DELTA"
        UsePass "glTF/PbrMetallicRoughness/ShadowCaster"
        UsePass "glTF/PbrMetallicRoughness/META"
    }

    FallBack "VertexLit"
    CustomEditor "GLTFast.Editor.BuiltInShaderGUI"
}
