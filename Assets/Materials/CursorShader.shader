Shader "Unlit/CursorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
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
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pulse = (sin(_Time.y * 4) + 1) / 4 + 0.5;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col = amplifyColor(col);
                col *= pulse;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
                
            ENDCG

            CGINCLUDE

            float4 amplifyColor(float4 col)
            {
                if (col.a < 0.5)
                    return col;

                col.r += 2 * (col.a - 0.5f);
                col.g += 2 * (col.a - 0.5f);
                col.b += 2 * (col.a - 0.5f);

                return col;
            }

            ENDCG
        }
    }
}
