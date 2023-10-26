Shader "ARPG/InfoPanelImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius px", Float) = 7
		_Width ("Width px", Float) = 1500
		_Height ("Height px", Float) = 1200
        _UseRoundedCorner ("UseRoundedCorner", Float) = 1

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent+1"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Lighting Off 
        Cull [_CullMode]
        ZTest [_ZTest]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
			float _Width;
			float _Height;
            float _UseRoundedCorner;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float2 GetRadiusToPointVector(float2 pixel, float2 halfRes, float radius) {
				float2 firstQuadrant = abs(pixel);
				float2 radiusToPoint = firstQuadrant - (halfRes - radius);
				radiusToPoint = max(radiusToPoint, 0.0);
				return radiusToPoint;
			}
	
			float HardRounded(float2 pixel, float2 halfRes, float radius) {
				float2 v = GetRadiusToPointVector(pixel, halfRes, radius);
				float alpha = 1.0 - floor(length(v) / radius);
				return alpha;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvInPixel = (i.uv - 0.5) * float2(_Width, _Height);
                float2 halfRes = float2(_Width, _Height) * 0.5;
                float alpha = HardRounded( uvInPixel, halfRes, _Radius );

                fixed4 col = tex2D(_MainTex, i.uv);

                if(_UseRoundedCorner){
                    if(alpha<=0.0)
                        discard;
                }
                return col;
            }
            ENDCG
        }
    }
}
