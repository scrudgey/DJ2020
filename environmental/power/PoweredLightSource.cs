using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightSource : MonoBehaviour {
    public MeshRenderer meshRenderer;
    public PoweredComponent poweredComponent;
    public Light[] lights;
    float timer;
    void Start() {
        poweredComponent.OnStateChange += OnPowerChange;
    }
    void OnDestroy() {
        poweredComponent.OnStateChange -= OnPowerChange;
    }
    public void OnPowerChange(PoweredComponent node) {
        if (meshRenderer != null) {
            if (node.power) {
                meshRenderer.material.EnableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", Color.white);
            } else {
                meshRenderer.material.DisableKeyword("_EMISSION");
                meshRenderer.material.SetColor("_EmissionColor", Color.black);
            }
        }

        foreach (Light light in lights) {
            light.enabled = node.power;
        }
    }
}
