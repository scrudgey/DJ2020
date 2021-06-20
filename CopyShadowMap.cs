using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CopyShadowMap : MonoBehaviour {
    CommandBuffer cb = null;

    void OnEnable() {
        var light = GetComponent<Light>();
        if (light) {
            cb = new CommandBuffer();
            cb.name = "CopyShadowMap";
            cb.SetGlobalTexture("_DirectionalShadowMask", new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
            light.AddCommandBuffer(UnityEngine.Rendering.LightEvent.AfterScreenspaceMask, cb);
        }
    }

    void OnDisable() {
        var light = GetComponent<Light>();
        if (light) {
            light.RemoveCommandBuffer(UnityEngine.Rendering.LightEvent.AfterScreenspaceMask, cb);
        }
    }
}