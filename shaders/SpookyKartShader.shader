Shader "Unlit/SpookyKartShader"
{
	Properties
	{
		[PerInstanceData]_MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_DecalOffset("DecalOffset", Float) = -1
		_Cutoff("Cutoff", Range(0,1)) = 0.5
		_FogStart("Fog Start", Float) = 10
		_FogRange("Fog Range", Float) = 5
		_FogColor("Fog Color", Color) = (1,1,1,1)
		_ScaleX ("Billboard Scale X", Float) = 1
		_ScaleY ("Billboard Scale Y", Float) = 1

		[KeywordEnum(None, Full, Y)] _Billboard ("Billboard mode", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1 //"One"
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 0 //"Zero"
		[Enum(Off,0,On,1)] _ZWrite("ZWrite", Float) = 1.0 //"On"
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
		LOD 100
		Offset [_DecalOffset], [_DecalOffset]
		ZTest LEqual
		Cull Off
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile _BILLBOARD_NONE _BILLBOARD_FULL _BILLBOARD_Y
			
			#include "UnityCG.cginc"

			struct appdata
			{
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float fogCoord : TEXCOORD1;
			};

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
			UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			float _Cutoff;
			
			float _FogStart;
			float _FogRange;
			float4 _FogColor;
			float4 _Color;

			float _ScaleX, _ScaleY;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

#if _BILLBOARD_NONE
				o.vertex = UnityObjectToClipPos(v.vertex);
#elif _BILLBOARD_FULL
				float4 vertOffset = float4(v.vertex.x * _ScaleX, v.vertex.y * _ScaleY, 0, 0.0);
				float4 camDir = UnityViewToClipPos(UnityObjectToViewPos(float3(0, 0, 0)) + vertOffset);
				o.vertex = camDir;
#elif _BILLBOARD_Y
				float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				float3 dist = _WorldSpaceCameraPos - worldPos;
				float angle = atan2(dist.x, dist.z);	// angle we need to rotate by to face the camera

				float3x3 rotMatrix;
				float cosinus = cos(angle);
				float sinus = sin(angle);

				// Construct rotation matrix
				rotMatrix[0].xyz = float3(cosinus, 0, sinus);
				rotMatrix[1].xyz = float3(0, 1, 0);
				rotMatrix[2].xyz = float3(-sinus, 0, cosinus);


				float4 newPos = float4(mul(rotMatrix, v.vertex * float4(_ScaleX, _ScaleY, 1, 0)), 1); // The position of the vertex after the rotation & squash z
				o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, newPos));
#endif

				float4 instancedOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST);
				o.uv = v.uv * instancedOffset.xy + instancedOffset.zw;
				o.fogCoord = -UnityObjectToViewPos(v.vertex).z;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				clip(col.a - _Cutoff);

				float fogFac = clamp((i.fogCoord - _FogStart) / _FogRange, 0, 1);
				fogFac = fogFac - fmod(fogFac, 0.1);
				col.rgb = lerp(col.rgb, _FogColor.rgb, fogFac);

				return col;
			}
			ENDCG
		}
	}
}