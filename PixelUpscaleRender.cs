using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class PixelUpscaleRender : MonoBehaviour {
    public Camera myCamera;
    public Material material;
    public bool applyFilter;
    public RenderTexture small;
    public RenderTexture large;
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        small.filterMode = FilterMode.Point;
        source.filterMode = FilterMode.Point;
        destination.filterMode = FilterMode.Point;
        if (applyFilter) {
            Graphics.Blit(source, large, material);
            Graphics.Blit(large, small);
        } else {
            Graphics.Blit(source, small);
        }
    }
}