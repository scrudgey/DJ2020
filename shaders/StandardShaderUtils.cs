// using UnityEngine;

// public static class StandardShaderUtils {
//     public enum BlendMode {
//         Opaque,
//         Cutout,
//         Fade,
//         Transparent
//     }

//     public static void ChangeRenderMode(Renderer renderer, BlendMode blendMode) {
//         MaterialPropertyBlock _propBlock = new MaterialPropertyBlock();
//         switch (blendMode) {
//             case BlendMode.Opaque:
//                 renderer.GetPropertyBlock(_propBlock);

//                 _propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
//                 _propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
//                 _propBlock.SetInt("_ZWrite", 1);
//                 // _propBlock.S
//                 // _propBlock.DisableKeyword("_ALPHATEST_ON");
//                 // _propBlock.DisableKeyword("_ALPHABLEND_ON");
//                 // _propBlock.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//                 // _propBlock.renderQueue = -1;

//                 renderer.SetPropertyBlock(_propBlock);
//                 break;
//             case BlendMode.Cutout:
//                 renderer.GetPropertyBlock(_propBlock);

//                 _propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
//                 _propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
//                 _propBlock.SetInt("_ZWrite", 1);
//                 // _propBlock.EnableKeyword("_ALPHATEST_ON");
//                 // _propBlock.DisableKeyword("_ALPHABLEND_ON");
//                 // _propBlock.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//                 // _propBlock.renderQueue = 2450;
//                 _propBlock.SetColor("_Color", Color.white);

//                 renderer.SetPropertyBlock(_propBlock);
//                 break;
//             case BlendMode.Fade:
//                 renderer.GetPropertyBlock(_propBlock);

//                 _propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//                 _propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//                 _propBlock.SetInt("_ZWrite", 0);
//                 // _propBlock.DisableKeyword("_ALPHATEST_ON");
//                 // _propBlock.EnableKeyword("_ALPHABLEND_ON");
//                 // _propBlock.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//                 // _propBlock.renderQueue = 3000;
//                 renderer.SetPropertyBlock(_propBlock);
//                 break;
//             case BlendMode.Transparent:
//                 renderer.GetPropertyBlock(_propBlock);

//                 _propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
//                 _propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//                 _propBlock.SetInt("_ZWrite", 0);
//                 // _propBlock.DisableKeyword("_ALPHATEST_ON");
//                 // _propBlock.DisableKeyword("_ALPHABLEND_ON");
//                 // _propBlock.EnableKeyword("_ALPHAPREMULTIPLY_ON");
//                 Color newColor = Color.white;
//                 newColor.a = 0.3f;
//                 _propBlock.SetColor("_Color", newColor);
//                 // _propBlock.renderQueue = 3000;

//                 renderer.SetPropertyBlock(_propBlock);
//                 break;
//         }

//     }
// }