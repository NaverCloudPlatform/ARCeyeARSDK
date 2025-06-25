using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARCeye
{
    public class MaterialGenerator
    {
        const string RenderPipelineTag = "RenderPipeline";
        const string UniversalPipelineType = "UniversalPipeline";

        const string k_ZWrite = "ARPG/ZWrite";

        public static Material GetZWriteMaterial()
        {
            Shader zwriteShader = Shader.Find(k_ZWrite);
            Material zwriteMaterial = new Material(zwriteShader);

            SetRenderPipeline(zwriteMaterial);

            return zwriteMaterial;
        }

        /// <summary>
        ///   Render pipeline 설정에 따라 알맞은 태그를 설정.
        /// </summary>
        public static void SetRenderPipeline(Material material)
        {
            // 현재는 Built-in과 URP만 지원.
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                material.SetOverrideTag(RenderPipelineTag, UniversalPipelineType);
            }
        }

        /// <summary>
        ///   PBR 오브젝트의 material을 fade 효과가 적용될 수 있는 material로 변경한다.
        /// </summary>
        public static void SetFadeModeBlend(Material material)
        {
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                URPMaterialGenerator.SetFadeModeBlend(material);
            }
            else
            {
                BuiltInMaterialGenerator.SetFadeModeBlend(material);
            }
        }
    }
}
