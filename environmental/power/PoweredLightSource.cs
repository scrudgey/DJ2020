using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredLightSource : MonoBehaviour {
    public MeshRenderer meshRenderer;
    public PoweredComponent poweredComponent;
    public Light[] lights;
    float timer;
    void Start() {
        // PoweredComponent poweredComponent = GetComponent<PoweredComponent>();
        poweredComponent.OnStateChange += OnPowerChange;
    }
    void OnDestroy() {
        poweredComponent.OnStateChange -= OnPowerChange;
    }
    public void OnPowerChange(PoweredComponent node) {
        if (node.power) {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.white);
        } else {
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
        foreach (Light light in lights) {
            light.enabled = node.power;
        }
    }
    // override protected void OnPowerOn() {
    //     meshRenderer.material.EnableKeyword("_EMISSION");
    //     // meshRenderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
    //     meshRenderer.materxial.SetColor("_EmissionColor", Color.white);
    //     // foreach (Light light in lights) {
    //     //     light.enabled = true;
    //     // }
    // }
    // override protected void OnPowerOff() {
    //     meshRenderer.material.DisableKeyword("_EMISSION");
    //     meshRenderer.material.SetColor("_EmissionColor", Color.black);
    //     // foreach (Light light in lights) {
    //     //     light.enabled = false;
    //     // }
    // }
}
