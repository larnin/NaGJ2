Shader "Unlit/CutMeshShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling("Tiling", float) = 1
        _Color("Color", Color) = (1, 1, 1, 1)
        _LowColor("LowColor", Color) = (1, 1, 1, 1)
        _Speed1("Speed1", float) = 1
        _Speed2("Speed2", float) = 1
        _Speed3("Speed3", float) = 1
        _Cutout("Cutout", float) = 0.5
        _Ceil("Ceil", float) = 0.5
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend One Zero
        AlphaToMask On
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _LowColor;
            float _Speed1;
            float _Speed2;
            float _Speed3;
            float _Cutout;
            float _Ceil;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 offset1 = fixed2(_Time.y, _Time.y / 4) * _Speed1;
                fixed2 offset2 = fixed2(-_Time.y / 2, _Time.y * 1.5) * _Speed2;
                fixed2 offset3 = fixed2(-_Time.y / 3, -_Time.y * 1.2) * _Speed3;

                fixed4 col1 = tex2D(_MainTex, i.uv + offset1);
                fixed4 col2 = tex2D(_MainTex, i.uv + offset2);
                fixed4 col3 = tex2D(_MainTex, i.uv + offset3);

                fixed4 col = (col1 + col2 + col3) / 3;
                float v = (col.r + col.g + col.b) / 3;


                v *= i.normal.x;

                if (v < _Cutout)
                    return float4(0, 0, 0, 0);

                float newValue = (v - _Cutout) / (1 - _Cutout);

                return _Color * newValue + _LowColor * (1 - newValue);
            }
            ENDCG
        }
    }
}
