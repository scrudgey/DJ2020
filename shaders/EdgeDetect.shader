Shader "Custom/EdgeDetect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
         _Threshold("Threshold", int) = 0
    }

    SubShader
    {
        Tags 
		{ 
			"RenderType" = "Opaque"
		}

        Pass
        {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float2 _MainTex_TexelSize;
            float _Threshold;
            sampler2D _CameraDepthTexture;

			float3 sobel(float2 uv)
			{
				float x = 0;
				float y = 0;

				float2 texelSize = _MainTex_TexelSize;
                x += tex2D(_CameraDepthTexture, uv + float2(-texelSize.x, -texelSize.y)) * -1.0;
                x += tex2D(_CameraDepthTexture, uv + float2(-texelSize.x,            0)) * -2.0;
                x += tex2D(_CameraDepthTexture, uv + float2(-texelSize.x,  texelSize.y)) * -1.0;

                x += tex2D(_CameraDepthTexture, uv + float2( texelSize.x, -texelSize.y)) *  1.0;
                x += tex2D(_CameraDepthTexture, uv + float2( texelSize.x,            0)) *  2.0;
                x += tex2D(_CameraDepthTexture, uv + float2( texelSize.x,  texelSize.y)) *  1.0;

                y += tex2D(_CameraDepthTexture, uv + float2(-texelSize.x, -texelSize.y)) * -1.0;
                y += tex2D(_CameraDepthTexture, uv + float2(           0, -texelSize.y)) * -2.0;
                y += tex2D(_CameraDepthTexture, uv + float2( texelSize.x, -texelSize.y)) * -1.0;

                y += tex2D(_CameraDepthTexture, uv + float2(-texelSize.x,  texelSize.y)) *  1.0;
                y += tex2D(_CameraDepthTexture, uv + float2(           0,  texelSize.y)) *  2.0;
                y += tex2D(_CameraDepthTexture, uv + float2( texelSize.x,  texelSize.y)) *  1.0;
				return sqrt(x * x + y * y) > _Threshold;
			}

			// Horizontal blurring pass.
			fixed4 frag(v2f_img i) : SV_Target
			{
				return fixed4(sobel(i.uv), 1.0);
			}
			ENDCG
        }
    }
}