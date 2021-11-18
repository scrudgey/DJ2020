// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

#ifndef SPRITE_SHADOWS_INCLUDED
#define SPRITE_SHADOWS_INCLUDED

#include "UnityCG.cginc"
#include "ShaderShared.cginc"

// #ifdef UNITY_INSTANCING_ENABLED

//     UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
//         // this could be smaller but that's how bit each entry is regardless of type
//         UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
//     UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

//     #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

// #endif // instancing

////////////////////////////////////////
// Vertex structs
//

struct vertexInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
};

struct vertexOutput
{ 
	V2F_SHADOW_CASTER;
	float2 texcoord : TEXCOORD1;
};

////////////////////////////////////////
// Vertex program
//

vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	// TRANSFER_SHADOW_CASTER(o)
    o.pos = calculateLocalPos(v.vertex);

    o.pos = UnityApplyLinearShadowBias(o.pos);

	o.texcoord = calculateTextureCoord(v.texcoord);
	return o;
}

////////////////////////////////////////
// Fragment program
//


uniform fixed _ShadowAlphaCutoff;

fixed4 frag(vertexOutput IN) : COLOR 
{
	fixed4 texureColor = calculateTexturePixel(IN.texcoord);
	clip(texureColor.a - _ShadowAlphaCutoff);
	
	SHADOW_CASTER_FRAGMENT(IN)
}

#endif // SPRITE_SHADOWS_INCLUDED