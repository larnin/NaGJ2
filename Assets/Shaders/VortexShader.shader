Shader "Unlit/VortexShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _LowColor ("LowColor", Color) = (1, 1, 1, 1)
        _HoleColor ("HoleColor", Color) = (0, 0, 0, 1)
        _Speed1 ("Speed1", float) = 1
        _Speed2 ("Speed2", float) = 1
        _Cutout ("Cutout", float) = 0.5
        _SpiraleScale("SpiraleScale", float) = 1
        _SpiraleRepeat("SpiraleRepeat", float) = 1
        _HoleSize("HoleSize", Range(-1, 1)) = 0
        _HoleHardBorder("HoleHardBorder", Range(0, 0.1)) = 0.02
        _HoleBorder("HoleBorder", Range(0, 1)) = 0.5
        _LightPercent("LightPercent", Range(0, 1)) = 0.5
        _OutlineScale("OutlineScale", float) = 1
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD2; //Needed for sampling the depth texture
                float depth : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _LowColor;
            float4 _HoleColor;
            float _Speed1;
            float _Speed2;
            float _Cutout;
            float _SpiraleScale;
            float _SpiraleRepeat;
            float _HoleSize;
            float _HoleHardBorder;
            float _HoleBorder;
            float _LightPercent;
            float _OutlineScale;

            sampler2D _CameraDepthNormalsTexture; //Automatically filled by Unity

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w; //ProjectionParams.w is the far plane distance
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (setHole(i.uv, _HoleSize) < 0.5)
                {
                    float v = intersection(_CameraDepthNormalsTexture, i.screenPos, i.depth, _OutlineScale);

                    fixed4 c = cutout(v, _Cutout, _Color, _LowColor);
                    if (c.a < 0.5)
                        return _HoleColor;

                    return c;
                }

                float2 u = polar(i.uv, _Time.y * _Speed1);
                fixed4 c = tex2D(_MainTex, u);
                float v = (c.r + c.g + c.b) / 3;

                float a = archimedeanSpiral(i.uv, _SpiraleScale, _Time.y * _Speed2, _SpiraleRepeat);
                v *= a;

                if (setHoleBorder(i.uv, _HoleSize, _HoleHardBorder))
                    v = 1;

                v = saturate(i.uv, v, _HoleSize, _HoleBorder, _LightPercent, _Cutout);

                c = cutout(v, _Cutout, _Color, _LowColor);

                return c;
            }
            ENDCG

            CGINCLUDE

            #include "UnityCG.cginc"

            float setHole(float2 uv, float hole)
            {
                float x = uv.x - 0.5;
                float y = uv.y - 0.5;

                float d = sqrt((x * x) + (y * y));

                return d > hole;
            }

            float setHoleBorder(float2 uv, float hole, float border)
            {
                float x = uv.x - 0.5;
                float y = uv.y - 0.5;

                float d = sqrt((x * x) + (y * y));

                return d > hole && d < hole + border;
            }

            float2 pixelise(float2 uv, float pixels)
            {
                return floor(uv * pixels) / pixels;
            }

            float4 cutout(float v, float ceil, float4 hightColor, float4 lowColor)
            {
                if (v < ceil)
                    return float4(0, 0, 0, 0);

                float newValue = (v - ceil) / (1 - ceil);

                return hightColor * newValue + lowColor * (1 - newValue);
            }

            float saturate(float2 uv, float value, float hole, float border, float percent, float add)
            {
                float x = uv.x - 0.5;
                float y = uv.y - 0.5;

                float d = sqrt((x * x) + (y * y));

                if (d < hole)
                    return 0;

                if (d > 1)
                    return 0;

                float realBorder = hole + border;
                if (realBorder > .5)
                    realBorder = .5;

                if (d > realBorder)
                    return 0;

                realBorder -= hole;

                float borderPercent = 1 - ((d - hole) / realBorder);

                if (borderPercent < percent)
                    return value * borderPercent / percent;

                float scale = (borderPercent - percent) / (1 - percent);
                float newValue = (scale * 2 + 1) * value + scale * add * 2;
                if (newValue > 1)
                    return 1;
                return newValue;
            }

            float archimedeanSpiral(float2 uv, float scale, float angle, float repeat)
            {
                float x = uv.x - 0.5;
                float y = uv.y - 0.5;

                float a = atan2(y, x) + angle + 3.141593;
                float d = scale * sqrt((x * x) + (y * y));
                d += a / (2 * 3.141593) * repeat;

                float f = floor(d);

                float v = d - f;

                if (v > 0.5)
                    return (1 - v) * 2;

                return v * 2;
            }

            float2 polar(float2 uv, float t)
            {
                float x = uv.x - 0.5;
                float y = uv.y - 0.5;

                float a = atan2(y, x);
                float d = sqrt((x * x) + (y * y));

                return float2(a / (2 * 3.141593) + d, d + t);
            }

            float intersection(sampler2D cameraDepth, float4 screenPos, float depth, float outlineScale)
            {
                float diff = DecodeFloatRG(tex2D(cameraDepth, screenPos.xy / screenPos.w).zw) - depth;
                float intersectGradient = 1 - min(diff * outlineScale / _ProjectionParams.w, 1.0f);
                return intersectGradient;
            }

            ENDCG
        }
    }
}
