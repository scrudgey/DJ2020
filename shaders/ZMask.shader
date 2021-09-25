Shader "Custom/ZMask" {
 
    Properties {
    }

    SubShader {
        LOD 100
   
        // Tags { "Queue"="Geometry-50" "IgnoreProjector"="True" "RenderType"="Opaque" }
        // Tags { "Queue"="Geometry+10"}

        // Tags {"RenderType" = "Opaque" "Queue"="Geometry-50"}
        // Tags {"RenderType" = "Opaque" "Queue"="AlphaTest+10"}
        Tags {"RenderType" = "Opaque" "Queue"="Geometry+10"}
        // Tags {"Queue"="2010"}

        ZWrite On   // for more on Z-buffer stuff and Offset, see here
        // ZTest LEqual   // GEqual = mask stuff in front of the mask geo
        Lighting Off
        // Color [_Color]   // change alpha in material to tweak mask strength
       
        Pass {
            ColorMask 0
        }
    }
}