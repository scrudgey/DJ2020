// Shader "Custom/InterloperUnlit"
// {
//     Properties
//     {
//         // _Color ("Color", Color) = (1,1,1,1)
//         _MainTex ("Texture", 2D) = "white" {}
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         LOD 100

//         Pass
//         {
//             CGPROGRAM
// // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members screenPos)
// #pragma exclude_renderers d3d11
//             #pragma vertex vert
//             #pragma fragment frag
//             // make fog work
//             #pragma multi_compile_fog

//             #include "UnityCG.cginc"

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 UNITY_FOG_COORDS(1)
//                 float4 vertex : SV_POSITION;
//                 float4 pos:TEXCOORD3;
//                 float4 screenPos:TEXCOORD2;
//             };

//             sampler2D _MainTex;
//             float4 _MainTex_ST;

//             v2f vert (appdata v)
//             {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                 UNITY_TRANSFER_FOG(o,o.vertex);

//                 o.pos = UnityObjectToClipPos(v.vertex.xyz);
//                 o.screenPos = ComputeScreenPos(o.pos); // using the UnityCG.cginc version unmodified
//                 return o;
//             }

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 // sample the texture
//                 fixed4 col = tex2D(_MainTex, i.uv);

//                 // col.a = _Transparency;
//                 col.a *= 0.5;

//                 // apply fog
//                 UNITY_APPLY_FOG(i.fogCoord, col);
//                 return col;
//             }
//             // fixed4 frag (v2f i) : COLOR
//             // {
//             //     // float2 screenUV = i.screenPos.xy / i.screenPos.w;
//             //     // o.pos;
//             //     fixed4 col = tex2D(_MainTex, i.texcoord);
//             //     col.a = _Transparency;
//             //     return col + _Emission;
//             // }
//             ENDCG
//         }
//     }
// }



Shader "Custom/InterloperUnlit" {
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

CGINCLUDE
#include "UnityStandardCore.cginc"
ENDCG

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // struct v2f {
            //     float4 vertex : SV_POSITION;
            //     float2 texcoord : TEXCOORD0;
            //     UNITY_FOG_COORDS(1)
            //     UNITY_VERTEX_OUTPUT_STEREO
            // };

             struct v2f
            {
                // float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
                float4 pos:TEXCOORD3;
                float4 screenPos:TEXCOORD2;
            };

            // sampler2D _MainTex;
            // float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.pos = UnityObjectToClipPos(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.pos); // using the UnityCG.cginc version unmodified
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                float a = clamp( (i.screenPos.a - 0.0) * 0.5 , 0, 1);
                col.a = a;
                UNITY_APPLY_FOG(i.fogCoord, col);

                // float attenuation = UNITY_LIGHT_ATTENUATION(i);
                UNITY_LIGHT_ATTENUATION(attenuation, i, i.pos.xyz);
                float _ShadowIntensity = 0.5f;
                return fixed4(0,0,0,(1-attenuation)*_ShadowIntensity);


                return col;
            }
        ENDCG
    }
}

}