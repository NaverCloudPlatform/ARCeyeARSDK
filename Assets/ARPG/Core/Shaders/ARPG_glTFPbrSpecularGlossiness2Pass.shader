// Based on glTFast PbrSpecularGlossiness shader source. Copyright (c) 2020 Andreas Atteneder. Apache License

Shader "glTF/PbrSpecularGlossiness (2Pass)"
{
    Properties
    {
        [HideInInspector] _MainTex ("My Texture", 2D) = "white" { }
        [MainColor] baseColorFactor("Diffuse", Color) = (1,1,1,1)
        [MainTexture] baseColorTexture("Diffuse Tex", 2D) = "white" {}
        baseColorTexture_Rotation ("Diffuse Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] baseColorTexture_texCoord ("Diffuse Tex UV", Float) = 0

        alphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        glossinessFactor("Glossiness", Range(0.0, 1.0)) = 1

        specularFactor("Specular", Color) = (1,1,1)
        specularGlossinessTexture("Specular-Glossiness Tex", 2D) = "white" {}
        specularGlossinessTexture_Rotation ("Specular-Glossiness Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] specularGlossinessTexture_texCoord ("Specular-Glossiness Tex UV", Float) = 0

        normalTexture_scale("Normal Scale", Float) = 1.0
        [Normal] normalTexture("Normal Tex", 2D) = "bump" {}
        normalTexture_Rotation ("Normal Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] normalTexture_texCoord ("Normal Tex UV Set", Float) = 0

        occlusionTexture_strength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        occlusionTexture("Occlusion Tex", 2D) = "white" {}
        occlusionTexture_Rotation ("Occlusion Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] occlusionTexture_texCoord ("Occlusion Tex UV Set", Float) = 0

        [HDR] emissiveFactor("Emissive", Color) = (0,0,0)
        emissiveTexture("Emissive Tex", 2D) = "white" {}
        emissiveTexture_Rotation ("Emissive Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] emissiveTexture_texCoord ("Emissive Tex UV", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT SpecularSetup
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
        UsePass "glTF/PbrSpecularGlossiness/FORWARD"
        UsePass "glTF/PbrSpecularGlossiness/FORWARD_DELTA"
        UsePass "glTF/PbrSpecularGlossiness/ShadowCaster"
        UsePass "glTF/PbrSpecularGlossiness/DEFERRED"
        UsePass "glTF/PbrSpecularGlossiness/META"
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
        UsePass "glTF/PbrSpecularGlossiness/FORWARD"
        UsePass "glTF/PbrSpecularGlossiness/FORWARD_DELTA"
        UsePass "glTF/PbrSpecularGlossiness/ShadowCaster"
        UsePass "glTF/PbrSpecularGlossiness/META"
    }

    FallBack "VertexLit"
    CustomEditor "GLTFast.Editor.BuiltInShaderGUI"
}
