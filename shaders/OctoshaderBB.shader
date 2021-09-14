// billboarded sprite with shadows

Shader "Sprites/Custom/SpriteShadowBillboard"
{
    Properties{
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
    [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _ScaleX ("Scale X", Float) = 1.0
        _ScaleY ("Scale Y", Float) = 1.0
    }

    SubShader{
        Tags {
        "Queue" = "Transparent"
        "IgnoreProjector" = "True"
        "RenderType" = "Transparent"
        "PreviewType" = "Plane"
        "CanUseSpriteAtlas" = "True"
        }
        
        Pass {
            ZWrite On            // write depth data to z-buffer
            ColorMask 0            // but won't write color to frame buffer
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        CGPROGRAM
#pragma surface surf Lambert vertex:vert alphatest:_Cutoff addshadow nofog nolightmap nodynlightmap keepalpha noinstancing
#pragma multi_compile_local _ PIXELSNAP_ON
#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
#include "UnitySprites.cginc"

            uniform float _ScaleX;
            uniform float _ScaleY;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color    : COLOR;
        };

        void vert(inout appdata_full v, out Input o) 
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
 
            // apply object scale
            v.vertex.xy *= float2(length(unity_ObjectToWorld._m00_m10_m20), length(unity_ObjectToWorld._m01_m11_m21));
 
            // get the camera basis vectors
            float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
            float3 up = normalize(UNITY_MATRIX_V._m10_m11_m12);
            float3 right = normalize(UNITY_MATRIX_V._m00_m01_m02);
 
            // rotate to face camera
            // float4x4 rotationMatrix = float4x4( right,      0,
            //                                     up,         0,
            //                                     forward,    0,
            //                                     0, 0, 0,    1);

            // rotate to face camera but leave height dimension alone.
            float4x4 rotationMatrix = float4x4( right,      0,
                                                up,         0,
                                                float3(1, 1, 1),    0,
                                                0, 0, 0,    1);

            v.vertex = mul(v.vertex, rotationMatrix);
            // v.normal = mul(v.normal, rotationMatrix);
 
            // undo object to world transform surface shader will apply
            v.vertex.xyz = mul((float3x3)unity_WorldToObject, v.vertex.xyz);
            v.normal = mul(v.normal, (float3x3)unity_ObjectToWorld);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = SampleSpriteTexture(IN.uv_MainTex) * IN.color;
            o.Albedo = c.rgb * c.a;
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Transparent/Cutout/VertexLit"
}