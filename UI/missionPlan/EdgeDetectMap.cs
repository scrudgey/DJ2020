using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
public class EdgeDetectMap : MonoBehaviour {
    public Material material;
    float storedShadowDistance;
    void OnPreRender() {
        storedShadowDistance = QualitySettings.shadowDistance;
        QualitySettings.shadowDistance = 0;
    }
    void OnPostRender() {
        QualitySettings.shadowDistance = storedShadowDistance;
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }
}