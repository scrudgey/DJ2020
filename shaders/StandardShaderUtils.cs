using UnityEngine;

public static class StandardShaderUtils {
    public enum BlendMode {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    public static void ChangeRenderMode(Renderer renderer, BlendMode blendMode) {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propBlock);
        switch (blendMode) {
            case BlendMode.Opaque:
                propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                propBlock.SetInt("_ZWrite", 1);
                foreach (Material m in renderer.materials) {
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = -1;
                }
                break;
            case BlendMode.Cutout:
                propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                propBlock.SetInt("_ZWrite", 1);
                // propBlock.SetColor("_Color", Color.white);
                foreach (Material m in renderer.materials) {
                    m.EnableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 2450;
                }
                break;
            case BlendMode.Fade:
                propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                propBlock.SetInt("_ZWrite", 0);
                foreach (Material m in renderer.materials) {
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                }
                break;
            case BlendMode.Transparent:
                propBlock.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                propBlock.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                propBlock.SetInt("_ZWrite", 0);
                // Color newColor = Color.white;
                // newColor.a = 0.3f;
                // propBlock.SetColor("_Color", newColor);
                foreach (Material m in renderer.materials) {
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                }
                break;
        }
        renderer.SetPropertyBlock(propBlock);
    }
}