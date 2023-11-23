using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class ClearsightRendererHandler {
    public enum State { opaque, transparent, above }
    List<SubRenderHandler> handlers;
    public Bounds bounds;
    NeoClearsighterV3 clearsighter;
    Transform myTransform;
    Vector3 adjustedPosition;
    int frames;
    int transparentRequests;
    bool isDynamic;

    float alpha;
    bool fadeAlpha;

    public ClearsightRendererHandler(NeoClearsighterV3 clearsighter, Transform root, Vector3 position, Bounds bounds, bool isDynamic = false) {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        this.isDynamic = isDynamic;
        this.myTransform = root;
        this.clearsighter = clearsighter;
        alpha = 1;

        this.bounds = renderers[0].bounds;
        this.handlers = new List<SubRenderHandler>();
        foreach (Renderer renderer in renderers) {
            if (renderer.name == "cutaway") continue;
            this.bounds.Encapsulate(renderer.bounds);
            handlers.Add(new SubRenderHandler(renderer));
        }

        Transform findAnchor = root.Find("clearSighterAnchor");
        if (findAnchor != null) {
            Debug.Log($"found clear sighter anchor: {root.gameObject} {findAnchor}");
            this.adjustedPosition = findAnchor.position;
        } else {
            this.adjustedPosition = position;
        }

        this.adjustedPosition.y -= bounds.extents.y;
    }

    State state;
    public void ChangeState(State toState) {
        switch (toState) {
            case State.opaque:
                if (state == toState) return;
                frames = 0;
                transparentRequests = 0;
                MakeOpaque();
                break;
            case State.transparent:
                frames = 3;
                transparentRequests += 2;
                if (transparentRequests >= 5) {
                    MakeTransparent();
                }
                break;
            case State.above:
                if (state == toState) return;
                frames = 0;
                transparentRequests = 0;
                MakeInvisible();
                break;
        }
        state = toState;
    }

    void MakeTransparent() {
        foreach (SubRenderHandler handler in handlers) {
            handler.MakeTransparent();
        }
        FadeOut();
    }

    void FadeOut() {
        fadeAlpha = true;
        foreach (SubRenderHandler handler in handlers) {
            handler.Fade();
        }
        clearsighter.OnTime += HandleTimeTick;
    }
    void FadeIn() {
        fadeAlpha = false;
        foreach (SubRenderHandler handler in handlers) {
            handler.Fade();
        }
        clearsighter.OnTime += HandleTimeTick;
    }

    void MakeInvisible() {
        foreach (SubRenderHandler handler in handlers) {
            handler.MakeInvisible();
        }
        clearsighter.OnTime -= HandleTimeTick;
        alpha = 0;
    }

    void MakeOpaque() {
        FadeIn();
        foreach (SubRenderHandler handler in handlers) {
            handler.MakeOpaque();
        }
    }


    public bool Update(Vector3 playerPosition) {
        // Update is called on handlers that were part of a previous batch not touched this frame.
        if (isDynamic) {
            adjustedPosition = myTransform.position;
            adjustedPosition.y -= bounds.extents.y;
        }
        // we disable geometry above if the floor of the renderer bounds is above the lifted origin point
        // which is player position + 1.5
        if (adjustedPosition.y > playerPosition.y) {
            ChangeState(State.above);
            return false;
        }

        if (transparentRequests > 7) {
            transparentRequests = 7;
        }
        if (transparentRequests > 0) {
            transparentRequests--;
        }

        if (frames > 0) {
            frames--;
        } else if (frames < 0) {
            frames = 0;
        }

        if (state == State.transparent) {
            if (frames <= 0 && transparentRequests < 2) {
                ChangeState(State.opaque);
                return true;
            }

        } else if (state == State.opaque) {
            return true;
        }
        return false;
    }

    void HandleTimeTick(float deltaTime) {
        if (fadeAlpha) {
            alpha -= deltaTime * 5;
        } else {
            alpha += deltaTime * 5;
        }
        if (alpha > 1) alpha = 1;
        if (alpha < 0) alpha = 0;

        foreach (SubRenderHandler handler in handlers) {
            handler.HandleTimeTick(alpha);
        }
        if (fadeAlpha && alpha <= 0) {
            clearsighter.OnTime -= HandleTimeTick;
            foreach (SubRenderHandler handler in handlers) {
                handler.CompleteFadeOut();
            }
        }
        if (!fadeAlpha && alpha >= 1) {
            clearsighter.OnTime -= HandleTimeTick;
            foreach (SubRenderHandler handler in handlers) {
                handler.CompleteFadeIn();
            }
        }
    }
}