using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class SubRenderHandler {
    public TagSystemData data;
    MaterialPropertyBlock propBlock;
    GameObject cutawayRenderer;
    Material interloperMaterial;
    public Material initialMaterial;
    public ShadowCastingMode initialShadowCastingMode;
    Bounds bounds;
    public bool isCutaway;

    public Renderer renderer;

    public SubRenderHandler(Renderer renderer) {
        this.renderer = renderer;
        propBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propBlock);
        initialMaterial = renderer.sharedMaterial;
        interloperMaterial = NeoClearsighterV3.NewInterloperMaterial(renderer);
        initialShadowCastingMode = renderer.shadowCastingMode;
        if (renderer.CompareTag("cutaway")) {
            isCutaway = true;
            Transform cutaway = renderer.transform.parent.Find("cutaway");
            cutaway.gameObject.SetActive(false);
            cutawayRenderer = cutaway.gameObject;
        }
        TagSystem system = renderer.GetComponentInParent<TagSystem>();
        if (system != null) {
            this.data = system.data;
        } else {
            this.data = new TagSystemData();
        }
        // this.data = Toolbox.GetTagData(renderer.gameObject);
    }

    public void MakeTransparent() {
        if (data.dontHideInterloper) return;
        if (isCutaway)
            cutawayRenderer.SetActive(true);
        Fade();
    }
    public void Fade() {
        if (data.dontHideInterloper) return;
        if (data.transparentIsInvisible) {
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        } else {
            renderer.material = interloperMaterial;
            renderer.shadowCastingMode = initialShadowCastingMode;
        }
    }

    public void MakeInvisible() {
        if (isCutaway)
            cutawayRenderer.SetActive(false);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    public void MakeOpaque() {
        if (renderer == null) return;
        if (isCutaway) {
            cutawayRenderer.SetActive(false);
        }
        if (renderer.CompareTag("occlusionSpecial")) return;
        Fade();
    }

    public void HandleTimeTick(float alpha, bool parentHasCutaway) {
        float myAlpha = alpha;
        if (!isCutaway && !parentHasCutaway) myAlpha = Mathf.Max(myAlpha, 0.7f);

        propBlock.SetFloat("_TargetAlpha", myAlpha);
        renderer.SetPropertyBlock(propBlock);
    }

    public void CompleteFadeOut() {
        if (isCutaway)
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }
    public void CompleteFadeIn() {
        renderer.shadowCastingMode = initialShadowCastingMode;
        renderer.material = initialMaterial;
    }

}