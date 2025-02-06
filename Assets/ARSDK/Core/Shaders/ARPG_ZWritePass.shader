Shader "ARPG/ZWrite" {
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
