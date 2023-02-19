Shader "Unlit/GridShader"
{
    Properties
    {

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100

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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 1);
                float d = dotting(i.uv);
                d = fadeWidth(d, i.normal.x, 1.5);

                float w = wave(i.uv);
                w = fadeWidth(w, i.normal.x, 1);
                
                float globalFade = fadeAtEnd(i.normal.y);

                float finalValue = (w * 2 + d) * globalFade;

                float b = finalValue / 2;
                float ceil = 0.5;
                float other = finalValue < ceil ? 0 : (finalValue - ceil) / (2 - ceil);

                col.r = other;
                col.g = other;
                col.b = b;
                col.a = b * 0.6;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG

            CGINCLUDE

            float dotting(float2 uv) 
            {
                return 1;
                /*float param = uv.y * 10;
                float value = sin(param);
                return value * value * value * value;*/
            }

            float fadeAtEnd(float d)
            {
                // d in range [0, 1]
                float v = d * 2 - 1;
                v *= v * v * v;
                return 1 - v;
            }

            float fadeWidth(float value, float d, float amplifier)
            {
                float v = d * 2 - 1;
                v *= v * amplifier;
                
                v = value - v;
                if (v < 0)
                    v = 0;
                return v;
            }

            float wave(float2 uv)
            {
                float param = uv.y + _Time.y * 5;
                param /= 4;
                param = fmod(param, 10) / 5;
                if (param > 1)
                    param = 2 - param;

                float ceil = 0.99f;
                if (param < ceil)
                    return 0;
                return (param - ceil) / (1 - ceil);
            }

            ENDCG
        }
    }
}
