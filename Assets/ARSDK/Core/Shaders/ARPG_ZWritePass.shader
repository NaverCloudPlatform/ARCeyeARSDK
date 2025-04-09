Shader "ARPG/ZWrite" {
    Properties
    {
        _Color ("Color", Vector) = (0, 0, 0)
    }
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue"="Transparent" }

        Pass
        {
            Name  "FrontPass"
            // Tags {"LightMode" = "SRPDefaultUnlit"}
            ZWrite On
            ColorMask 0
        }
    }
}
