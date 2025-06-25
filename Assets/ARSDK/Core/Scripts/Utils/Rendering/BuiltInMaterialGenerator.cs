using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GLTFast.Materials;

namespace ARCeye
{
    /// <summary>
    ///   GLTFast의 쉐이더 설정을 수정.
    /// </summary>
    public class BuiltInMaterialGenerator
    {
        const string RenderTypeTag = "RenderType";
        const string FadeRenderType = "Fade";

        const string k_AlphaTestOnKeyword = "_ALPHATEST_ON";
        const string k_AlphaBlendOnKeyword = "_ALPHABLEND_ON";
        const string k_AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";


        /// <summary>
        /// Fade 효과를 위한 shader 속성 설정.
        /// 기존의 'BuiltInMaterialGenerator.SetAlphaModeBlend'는 ZWrite가 비활성화 되어 있고 cull mode를 설정하지 않는다.
        /// 자연스러운 Fade 효과를 위해서 ZWrite와 back face culling을 활성화 해야 한다.
        /// </summary>
        public static void SetFadeModeBlend(Material material)
        {
            material.SetFloat(MaterialProperty.Mode, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(RenderTypeTag, FadeRenderType);

            material.EnableKeyword(k_AlphaBlendOnKeyword);

            material.SetInt(MaterialProperty.SrcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(MaterialProperty.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt(MaterialProperty.ZWrite, 1);   // On
            material.SetInt(MaterialProperty.CullMode, 2); // Back

            material.DisableKeyword(k_AlphaPremultiplyOnKeyword);
            material.DisableKeyword(k_AlphaTestOnKeyword);

            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}