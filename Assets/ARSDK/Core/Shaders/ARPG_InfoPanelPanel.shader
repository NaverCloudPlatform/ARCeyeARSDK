Shader "ARPG/InfoPanelPanel"
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
			float _Width;
			float _Height;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.uv);

                float2 uvInPixel = float2(i.uv.x, 1-i.uv.y) * float2(_Width, _Height);

                float cornerLeft   = 0.095; // NOTE: absolute size of left corner
                float cornerRight  = 0.095; // NOTE: absolute size of right corner
                float cornerTop    = 0.095; // NOTE: absolute size of left corner
                float cornerBottom = 0.328; // NOTE: absolute size of right corner
                
                float tccLeft   = 66.0/1024.0; // NOTE: textureCornerCoord, relative coords of left corner in texture
                float tccRight  = 66.0/1024.0; // NOTE: textureCornerCoord, relative coords of right corner in texture
                float tccTop    = 66.0/749.0; // NOTE: textureCornerCoord, relative coords of left corner in texture
                float tccBottom = 227.0/749.0; // NOTE: textureCornerCoord, relative coords of right corner in texture

                float2 computedUV;

                // y-axis (top and bottom)
                if(uvInPixel.y < cornerTop) 
                    computedUV.y = 1-(uvInPixel.y * tccTop/cornerTop);
                else if(uvInPixel.y>=_Height-cornerBottom) 
                   computedUV.y = 1-((uvInPixel.y - (_Height-cornerBottom)) * tccBottom/cornerBottom + (1.0-tccBottom));
                else
                    computedUV.y = 0.5; //(O)

                // x-axis (left and right)
                if(uvInPixel.x < cornerLeft)
                    computedUV.x = uvInPixel.x * tccLeft/cornerLeft;
                else if (uvInPixel.x > _Width - cornerRight)
                    computedUV.x = (uvInPixel.x - (_Width-cornerRight)) * tccRight/cornerRight + (1.0-tccRight);
                else
                    computedUV.x = 0.5;

                // (bottom-center)
                if(uvInPixel.x >= cornerLeft && uvInPixel.x <= _Width - cornerRight && uvInPixel.y > _Height - cornerBottom){
                    if(i.uv.x<=0.5){
                        if (i.uv.x>=(cornerLeft-0.018)) {
                            computedUV.x = 0.5-(((0.5-i.uv.x)/(_Height/_Width) * _Height * tccBottom / cornerBottom)*(749.0/1024.0));
                        }else{
                            computedUV.x = 0.5-(((0.5-i.uv.x)/(_Width/_Height) * _Height * tccBottom / cornerBottom)*(749.0/1024.0));
                        }
                    }
                    else{
                        if (i.uv.x<1-(cornerRight-0.018)) {
                            computedUV.x = 0.5+(((i.uv.x-0.5)/(_Height/_Width) * _Height * tccBottom / cornerBottom)*(749.0/1024.0));
                        }else{
                            computedUV.x = 0.5+(((i.uv.x-0.5)/(_Width/_Height) * _Height * tccBottom / cornerBottom)*(749.0/1024.0));
                        }
                    }
                }

                fixed4 col = tex2D(_MainTex, computedUV);
                //col = float4(computedUV.x, computedUV.y, 0, 1);

                return col;
            }
            ENDCG
        }
    }
}
