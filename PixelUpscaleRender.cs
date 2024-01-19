using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class PixelUpscaleRender : MonoBehaviour {
    public Camera myCamera;
    public Material material;
    public bool applyFilter;
    public RenderTexture half;
    public RenderTexture intermediate;
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        half.filterMode = FilterMode.Point;
        source.filterMode = FilterMode.Point;
        destination.filterMode = FilterMode.Point;
        if (applyFilter) {
            Graphics.Blit(source, intermediate, material);
            Graphics.Blit(intermediate, half);
        } else {
            Graphics.Blit(source, half);
        }
    }
}