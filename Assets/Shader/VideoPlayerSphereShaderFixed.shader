Shader "Custom/InsideSphereFixed360"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendSize ("Seam Blend Size", Range(0, 0.05)) = 0.005
        _PoleFade ("Pole Fade Strength", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Front
        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlendSize;
            float _PoleFade;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Corrigir costura horizontal com blend
                if (uv.x < _BlendSize)
                {
                    float4 col1 = tex2D(_MainTex, float2(uv.x, uv.y));
                    float4 col2 = tex2D(_MainTex, float2(uv.x + 1.0 - _BlendSize, uv.y));
                    float t = uv.x / _BlendSize;
                    return lerp(col2, col1, t);
                }
                else if (uv.x > 1.0 - _BlendSize)
                {
                    float4 col1 = tex2D(_MainTex, float2(uv.x, uv.y));
                    float4 col2 = tex2D(_MainTex, float2(uv.x - 1.0 + _BlendSize, uv.y));
                    float t = (uv.x - (1.0 - _BlendSize)) / _BlendSize;
                    return lerp(col1, col2, t);
                }

                // Suavizar polos (topo e fundo da esfera)
                float fadeTop = smoothstep(1.0, 1.0 - _PoleFade, uv.y);
                float fadeBottom = smoothstep(0.0, _PoleFade, uv.y);
                float fade = min(fadeTop, fadeBottom);

                return tex2D(_MainTex, uv) * fade;
            }
            ENDCG
        }
    }
}