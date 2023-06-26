using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class EdgeDetectMap : MonoBehaviour {
    public Camera myCamera;
    public Material material;
    public bool applyFilter;
    float storedShadowDistance;
    void Start() {
        myCamera.depthTextureMode = DepthTextureMode.Depth;
    }
    void OnPreRender() {
        storedShadowDistance = QualitySettings.shadowDistance;
        QualitySettings.shadowDistance = 0;
    }
    void OnPostRender() {
        QualitySettings.shadowDistance = 1000;
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (applyFilter) {
            Graphics.Blit(source, destination, material);
        } else {
            Graphics.Blit(source, destination);
        }
    }
}