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
    float myAlpha;
    CullingComponent parent;

    public SubRenderHandler(Renderer renderer, CullingComponent parent) {
        this.renderer = renderer;
        this.parent = parent;
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
    }

    public void FadeTransparent() {
        if (renderer == null) return;
        if (isCutaway)
            cutawayRenderer.SetActive(true);
        Fade();
    }

    public void FadeOpaque() {
        if (renderer == null) return;
        if (isCutaway)
            cutawayRenderer.SetActive(false);
        Fade();
    }

    public void Fade() {
        if (renderer == null) return;
        if (data.dontHideInterloper) return;
        if (data.partialTransparentIsInvisible) {
            renderer.material = initialMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        } else {
            renderer.material = interloperMaterial;
            renderer.shadowCastingMode = initialShadowCastingMode;
        }
    }

    public void HandleTimeTick(float alpha, bool parentHasCutaway) {
        if (renderer == null || data.partialTransparentIsInvisible) return;
        myAlpha = alpha;
        myAlpha = Math.Max(0.1f, myAlpha);
        if (!isCutaway && !parentHasCutaway) myAlpha = Mathf.Max(0.7f, myAlpha);
        propBlock.SetFloat("_TargetAlpha", myAlpha);
        renderer.SetPropertyBlock(propBlock);
    }
    public void TotallyInvisible(bool debug) {
        if (renderer == null) return;
        if (isCutaway)
            cutawayRenderer.SetActive(false);
        if (debug) {
            Debug.Log($"invisible: renderer: {renderer}\tmaterial:{renderer.material}->{initialMaterial}\tshadowmode:{renderer.shadowCastingMode}->{initialShadowCastingMode}");
        }
        renderer.material = initialMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }
    public void CutawayInvisible(bool parentHasCutaway) {
        if (renderer == null) return;
        if (isCutaway)
            cutawayRenderer.SetActive(true);
        if (!isCutaway && !parentHasCutaway && !data.totallTransparentIsInvisible) {
            myAlpha = 0.7f;
            propBlock.SetFloat("_TargetAlpha", myAlpha);
            renderer.SetPropertyBlock(propBlock);
        } else {
            renderer.material = initialMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }
    public void Visible(bool debug) {
        if (renderer == null) return;
        if (isCutaway)
            cutawayRenderer.SetActive(false);

        if (debug) {
            Debug.Log($"visibile: renderer: {renderer}\tmaterial:{renderer.material}->{initialMaterial}\tshadowmode:{renderer.shadowCastingMode}->{initialShadowCastingMode}");
        }
        renderer.material = initialMaterial;
        renderer.shadowCastingMode = initialShadowCastingMode;

    }

}