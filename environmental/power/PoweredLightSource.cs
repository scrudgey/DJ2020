using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightSource : PoweredComponent {
    public MeshRenderer meshRenderer;
    public Light[] lights;
    float timer;

    override protected void OnPowerOn() {
        meshRenderer.material.EnableKeyword("_EMISSION");
        // meshRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        meshRenderer.material.SetColor("_EmissionColor", Color.white);
        foreach (Light light in lights) {
            light.enabled = true;
        }
    }
    override protected void OnPowerOff() {
        meshRenderer.material.DisableKeyword("_EMISSION");
        meshRenderer.material.SetColor("_EmissionColor", Color.black);
        foreach (Light light in lights) {
            light.enabled = false;
        }
    }
}
