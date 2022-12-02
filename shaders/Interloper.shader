Shader "Custom/Interloper"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _TargetAlpha ("TargetAlpha", Range(0,1)) = 1.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 0.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0 

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _TargetAlpha;

        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            // c.a = clamp( (0.5 - IN.screenPos.a) * -0.5, 0, 1);
            // float camDistance = distance(IN.worldPos, _WorldSpaceCameraPos);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            if (c.a > 0.5)
                o.Alpha = _TargetAlpha;
            else
                o.Alpha = 0;
        }


        half4 LightingStandard (SurfaceOutputStandard s, half3 lightDir, half atten) {
                half NdotL = dot (s.Normal, lightDir);
                half4 c; c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
                c.a = s.Alpha;
                return c;
        }

        inline fixed4 LightingStandard_SingleLightmap (SurfaceOutputStandard s, fixed4 color) {
            half3 lm = DecodeLightmap (color);
            return fixed4(lm, 0);
        }

        inline fixed4 LightingStandard_DualLightmap (SurfaceOutputStandard s, fixed4 totalColor, fixed4 indirectOnlyColor, half indirectFade) {
            half3 lm = lerp (DecodeLightmap (indirectOnlyColor), DecodeLightmap (totalColor), indirectFade);
            return fixed4(lm, 0);
        }

        inline fixed4 LightingStandard_StandardLightmap (SurfaceOutputStandard s, fixed4 color, fixed4 scale, bool surfFuncWritesNormal) {
            UNITY_DIRBASIS

            half3 lm = DecodeLightmap (color);
            half3 scalePerBasisVector = DecodeLightmap (scale);

            if (surfFuncWritesNormal)
            {
                half3 normalInRnmBasis = saturate (mul (unity_DirBasis, s.Normal));
                lm *= dot (normalInRnmBasis, scalePerBasisVector);
            }

            return fixed4(lm, 0);
        }


        ENDCG
    }
    FallBack "Diffuse"
}