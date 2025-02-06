using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class URPMaterialGenerator
    {
        private const string k_PBRMetallicRoughness = "Shader Graphs/glTF-pbrMetallicRoughness-alpha";
        private const string k_PBRSpecularGlossiness = "Shader Graphs/glTF-pbrSpecularGlossiness-alpha";
        private const string k_PBRUnlit = "Shader Graphs/glTF-unlit-alpha";

        public static void SetFadeModeBlend(Material material)
        {
            Shader alphaShader;
            
            string shaderName = material.shader.name;
            if(shaderName.Contains("pbrMetallicRoughness"))
            {
                alphaShader = Shader.Find(k_PBRMetallicRoughness);
            }
            else if(shaderName.Contains("pbrSpecularGlossiness"))
            {
                alphaShader = Shader.Find(k_PBRSpecularGlossiness);
            }
            else if(shaderName.Contains("unlit"))
            {
                alphaShader = Shader.Find(k_PBRUnlit);
            }
            else
            {
                Debug.LogError($"Alpha shader for {shaderName} isn't implemented!");
                return;
            }

            material.shader = alphaShader;
        }
    }
}