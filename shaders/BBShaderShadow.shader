Shader "Sprites/Custom/BillboardShadow" {
    Properties{
        _ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1)
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _ScaleX ("Scale X", Float) = 1.0
        _ScaleY ("Scale Y", Float) = 1.0
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "DisableBatching" = "True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert alphatest:_Cutoff addshadow
            #pragma fragment frag


            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #include "UnityCG.cginc"
            
            uniform float _ScaleX;
            uniform float _ScaleY;
            sampler2D _MainTex;

            struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
            
            v2f vert(appdata_t IN)
			{
                v2f OUT;

                float4 view = mul(
                    UNITY_MATRIX_MV, 
                    float4(0, IN.vertex.y * _ScaleY, 0.0, 1.0)
                ) + float4(IN.vertex.x * _ScaleX, 0.0, 0.0, 0.0) ;

                float4 proj = mul(
                    UNITY_MATRIX_P, 
                    view 
                );

                OUT.vertex = proj;
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : COLOR
            {
                fixed4 color = tex2D (_MainTex, IN.texcoord);
                color.rgb *= color.a;
                color.rgb *= IN.color;

                return color;
            }
            ENDCG
        }
    }
        Fallback "Transparent/VertexLit"

}