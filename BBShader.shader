Shader "Billboard" {Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ScaleX ("Scale X", Float) = 1.0
        _ScaleY ("Scale Y", Float) = 1.0
    }
    

    SubShader {
        Tags {
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

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            uniform float _ScaleX;
            uniform float _ScaleY;
            
            appdata_img vert(appdata_img IN)
            {
                appdata_img OUT;

                float4 view = mul(
                    UNITY_MATRIX_MV, 
                    float4(0.0, 0.0, 0.0, 1.0)
                ) + float4(IN.vertex.x * _ScaleX, IN.vertex.y * _ScaleY, 0.0, 0.0);

                float4 proj = mul(
                    UNITY_MATRIX_P,
                    view
                );
                
                OUT.vertex = proj;
                OUT.texcoord = IN.texcoord;
                
                return OUT;
            }
            
            sampler2D _MainTex;
            fixed4 frag(appdata_img IN) : COLOR
            {
                fixed4 color = tex2D (_MainTex, IN.texcoord);
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}