using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CullingComponent : MonoBehaviour {

    public enum CullingState { normal, interloper, above }
    public enum RendererState { opaque, transparent, invisible }
    public string idn;
    public int floor;
    public Bounds bounds;
    public string myName;
    public string rooftopZoneIdn;
    CullingState state;
    Dictionary<Renderer, SubRenderHandler> handlers;
    public Vector3 adjustedPosition;
    bool isDynamic;
    bool hasCutaway;


    public void Initialize(bool isDynamic = false) {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        this.isDynamic = isDynamic;
        if (renderers.Length > 0) {
            Bounds tmpBounds = renderers[0].bounds;

            this.handlers = new Dictionary<Renderer, SubRenderHandler>();
            foreach (Renderer renderer in renderers) {
                if (renderer is ParticleSystemRenderer) continue;
                if (renderer is TrailRenderer) continue;
                if (renderer.name == "cutaway") continue;
                AddSubRenderHandler(renderer);
                tmpBounds.Encapsulate(renderer.bounds);
            }
            this.bounds = tmpBounds;
            Transform findAnchor = transform.Find("clearSighterAnchor");
            if (findAnchor != null) {
                // Debug.Log($"found clear sighter anchor: {root.gameObject} {findAnchor}");
                this.adjustedPosition = findAnchor.position;
            } else {
                this.adjustedPosition = tmpBounds.center;
                this.adjustedPosition.y -= tmpBounds.extents.y;
            }
        } else {
            this.handlers = new Dictionary<Renderer, SubRenderHandler>();
        }
    }

    public void AddSubRenderHandler(Renderer renderer) {
        SubRenderHandler handler = new SubRenderHandler(renderer);
        handlers[renderer] = handler;
        hasCutaway |= handler.isCutaway;
    }
    public void RemoveSubRenderHandler(Renderer renderer) {
        handlers.Remove(renderer);
    }
    public void ApplyCulling(Vector3 playerPosition) {
        // if (IsAbove(playerPosition)) {
        //     ChangeState(CullingComponent.CullingState.above);
        // } else {
        ChangeState(CullingComponent.CullingState.interloper);
        // }
    }
    public void StopCulling() {
        ChangeState(CullingComponent.CullingState.normal);
    }
    public void ChangeState(CullingState toState) {
        switch (toState) {
            case CullingState.normal:
                FadeIn();
                break;
            case CullingState.interloper:
                FadeOut();
                break;
            case CullingState.above:
                MakeInvisible();
                break;
        }
        state = toState;
    }
    public bool IsAbove(Vector3 playerPosition) {
        return adjustedPosition.y > playerPosition.y;
    }
    public bool IsAbove(int playerFloor) {
        return floor > playerFloor;
    }
    void MakeInvisible() {
        foreach (SubRenderHandler handler in handlers.Values) {
            handler.TotallyInvisible();
        }
    }
    void FadeOut() {
        foreach (SubRenderHandler handler in handlers.Values) {
            if (!(state == CullingState.interloper && handler.data.dontHideInterloper))
                handler.CompleteFadeOut(hasCutaway);
        }
    }
    void FadeIn() {
        foreach (SubRenderHandler handler in handlers.Values) {
            handler.CompleteFadeIn();
        }
    }

}