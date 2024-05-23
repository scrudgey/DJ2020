using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightSource : MonoBehaviour, INodeBinder<PowerNode> {
    public PowerNode node { get; set; }
    public MeshRenderer meshRenderer;
    public Light[] lights;
    float timer;
    public void HandleNodeChange() {
        if (meshRenderer != null) {
            if (node.powered) {
                meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", Color.white);
            } else {
                meshRenderer.material.DisableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", Color.black);
            }
        }

        foreach (Light light in lights) {
            if (light == null) continue;
            light.enabled = node.powered;
        }
    }
}
