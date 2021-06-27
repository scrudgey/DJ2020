// Sprite Shadow Shader - AllenDevs

Shader "Sprites/Custom/SpriteShadow"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
    [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _ScaleY ("Scale Y", Float) = 1.0
        _ScaleX ("Scale X", Float) = 1.0
    }

        SubShader
    {
        Tags
    {
        "Queue" = "Transparent"
        "IgnoreProjector" = "True"
        "RenderType" = "Transparent"
        "PreviewType" = "Plane"
        "CanUseSpriteAtlas" = "True"
    }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        CGPROGRAM
            #pragma target 3.0
#pragma surface surf Lambert vertex:vert alphatest:_Cutoff addshadow nofog nolightmap nodynlightmap keepalpha noinstancing
#pragma multi_compile_local _ PIXELSNAP_ON
#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
#include "UnitySprites.cginc"
        uniform float _ScaleY;
        uniform float _ScaleX;

        struct Input
    {
        float2 uv_MainTex;
        fixed4 color;
        float4 vertex   : POSITION;
        float2 texcoord : TEXCOORD0;
    };

    void vert(inout appdata_full v, out Input o)
    {
        UNITY_INITIALIZE_OUTPUT(Input, o);

        float4 view = mul(
            UNITY_MATRIX_MV, 
            float4(0, v.vertex.y * _ScaleY, 0.0, 1.0)
        ) + float4(v.vertex.x * _ScaleX, 0.0, 0.0, 0.0) ;

        float4 proj = mul(
            UNITY_MATRIX_P, 
            view 
        );

        v.vertex = proj;

        o.color = v.color * _Color * _RendererColor;
        o.vertex = proj;
        o.texcoord = v.texcoord;
    }

    void surf(Input IN, inout SurfaceOutput o)
    {
        fixed4 c = SampleSpriteTexture(IN.uv_MainTex) * IN.color;
        o.Albedo = c.rgb * c.a;
        o.Alpha = c.a;
    }
    ENDCG
    }

        Fallback "Transparent/VertexLit"
}