using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public static class MaterialProperty
    {
        public static readonly int Mode = Shader.PropertyToID("_Mode");
        public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        public static readonly int CullMode = Shader.PropertyToID("_CullMode");
    }
}