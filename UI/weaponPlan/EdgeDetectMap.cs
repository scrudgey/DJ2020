using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class EdgeDetectMap : MonoBehaviour {
    public Material material;
    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }
}