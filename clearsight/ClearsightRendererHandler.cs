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
    public enum CullingState { normal, interloper, above }
    public enum RendererState { opaque, transparent, invisible }
    public Bounds bounds;
    public string name;
    NeoClearsighterV3 clearsighter;

    CullingState state;
    RendererState currentRendererState;
    RendererState desiredRendererState;

    Dictionary<Renderer, SubRenderHandler> handlers;

    Vector3 adjustedPosition;
    int stayTransparentFrames;
    int transparentRequests;
    bool isDynamic;
    float alpha;
    bool hasCutaway;

    bool isSubscribedToTimeUpdate;


    public ClearsightRendererHandler(NeoClearsighterV3 clearsighter, Transform root, Vector3 position, bool isDynamic = false) {
        name = root.name;
        // if (name.Contains("datastore")) {
        //     Debug.Log($"datastore clearsighter created: {root}");
        // }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        this.isDynamic = isDynamic;
        this.clearsighter = clearsighter;
        alpha = 1;

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
        Transform findAnchor = root.Find("clearSighterAnchor");
        if (findAnchor != null) {
            // Debug.Log($"found clear sighter anchor: {root.gameObject} {findAnchor}");
            this.adjustedPosition = findAnchor.position;
        } else {
            this.adjustedPosition = tmpBounds.center;
            this.adjustedPosition.y -= tmpBounds.extents.y;
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

    public void ChangeState(CullingState toState) {
        if (name.Contains("datastore")) {
            Debug.Log($"clearsighter {name}: changestate {state} -> {toState}");
        }
        switch (toState) {
            case CullingState.normal:
                if (desiredRendererState != RendererState.opaque) {
                    FadeIn();
                }
                transparentRequests = 0;
                desiredRendererState = RendererState.opaque;
                break;
            case CullingState.interloper:
                transparentRequests += 1;
                if ((transparentRequests >= 3 || stayTransparentFrames > 0)
                    && desiredRendererState != RendererState.transparent
                    && currentRendererState != RendererState.transparent) {
                    stayTransparentFrames = 3;
                    FadeOut();
                    desiredRendererState = RendererState.transparent;
                }

                // how to handle easing between states?
                // we tally transparent requests- if we just went transparent we don't go back to opaque right away
                // we also need to integrate a few transparent requests before we start fading.

                // since we need to change the requests and stayframes per frame, we need to subscribe to time updates.
                // just make sure we unsubscribe correctly!
                // what is the condition there?
                // it would mean currentRendererState == desiredRendererState and also the integrations are 0.

                // allow us to change desired state and subscribe- easy.
                // allow frames / requests to determine when we start to fade.
                //  put all that logic in time update
                // 
                // overall this means we need more state.
                // it means we can have a desired renderer state that is different from the current renderer state
                //  but not updating alpha yet.

                // the main problem with ubsubscribe when current == desired and frames are 0:
                // we will continue to request transparent constantly as object is in view.
                // therefore frames will never be 0 and we will never unsubscribe.

                // desired behavior:
                // state: normal    render: opaque
                // transparent request issued : do nothing
                // transparent request issued : do nothing
                // transparent request issued : do nothing
                // transparent request issued : desired state: transparent, subscrube
                // update: alpha
                // update: alpha
                // update: alpha
                // update: alpha
                // current state == transparent, ubsubscribe


                break;
            case CullingState.above:
                if (desiredRendererState != RendererState.invisible) {
                    FadeOut();
                }
                desiredRendererState = RendererState.invisible;
                transparentRequests = 0;
                break;
        }
        if (currentRendererState != desiredRendererState) {
            // switch (desiredRendererState) {
            //     case RendererState.opaque:
            //         break;
            //     case RendererState.invisible:
            //         break;
            //     case RendererState.transparent:
            //         FadeOut();
            //         break;
            // }
            SubscribeToTimeUpdate();
        }
        state = toState;
    }

    void SubscribeToTimeUpdate() {
        if (!isSubscribedToTimeUpdate) {
            isSubscribedToTimeUpdate = true;
            clearsighter.OnTime += HandleTimeTick;
        }
    }

    void MakeInvisible() {
        foreach (SubRenderHandler handler in handlers.Values) {
            handler.TotallyInvisible();
        }
    }
    void FadeOut() {
        foreach (SubRenderHandler handler in handlers.Values) {
            if (!(state == CullingState.interloper && handler.data.dontHideInterloper))
                handler.FadeTransparent();
        }
    }
    void FadeIn() {
        foreach (SubRenderHandler handler in handlers.Values) {
            handler.FadeOpaque();
        }
    }
    public bool IsAbove(Vector3 playerPosition) {
        return adjustedPosition.y > playerPosition.y;
    }

    void HandleTimeTick(float deltaTime) {
        if (transparentRequests > 7) {
            transparentRequests = 7;
        }
        if (transparentRequests > 0) {
            transparentRequests--;
        }
        if (stayTransparentFrames > 0) {
            stayTransparentFrames--;
        } else if (stayTransparentFrames < 0) {
            stayTransparentFrames = 0;
        }

        if (desiredRendererState == RendererState.opaque && stayTransparentFrames > 0) {
            return;
        }

        if (currentRendererState != desiredRendererState) {
            // we are in transition

            // update alpha
            switch (desiredRendererState) {
                case RendererState.opaque:
                    alpha += deltaTime * 4;
                    break;
                case RendererState.invisible:
                case RendererState.transparent:
                    alpha -= deltaTime * 2;
                    break;
            }
            if (alpha > 1) alpha = 1;
            if (alpha < 0) alpha = 0;

            // apply alpha to all sub renderers
            foreach (SubRenderHandler handler in handlers.Values) {
                handler.HandleTimeTick(alpha, hasCutaway);
            }

            // conclude transition conditionally
            switch (desiredRendererState) {
                case RendererState.transparent:
                    if (alpha <= 0.1) {
                        foreach (SubRenderHandler handler in handlers.Values) {
                            if (!(state == CullingState.interloper && handler.data.dontHideInterloper))
                                handler.CompleteFadeOut(hasCutaway);
                        }
                        currentRendererState = RendererState.transparent;
                    }
                    break;
                case RendererState.opaque:
                    if (alpha >= 1) {
                        foreach (SubRenderHandler handler in handlers.Values) {
                            handler.CompleteFadeIn();
                        }
                        currentRendererState = RendererState.opaque;
                    }
                    break;
                case RendererState.invisible:
                    if (alpha <= 0) {
                        MakeInvisible();
                        foreach (SubRenderHandler handler in handlers.Values) {
                            if (!(state == CullingState.interloper && handler.data.dontHideInterloper))
                                handler.CompleteFadeOut(hasCutaway);
                        }
                        currentRendererState = RendererState.invisible;
                    }
                    break;
            }
        } else if (currentRendererState == desiredRendererState) {
            isSubscribedToTimeUpdate = false;
            clearsighter.OnTime -= HandleTimeTick;
        }
    }

    public bool IsVisible() {
        return currentRendererState == RendererState.opaque;
    }
}





// if (state == CullingState.interloper) {
//     if (stayTransparentFrames <= 0 && transparentRequests < 5) {
//         ChangeState(CullingState.opaque);
//         return true;
//     }
// } else if (state == CullingState.opaque || state == CullingState.forceOpaque) {
//     return true;
// }