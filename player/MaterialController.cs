using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
public class MaterialController {
    public enum State { opaque, transparent, fadeOut, fadeIn }
    public State state;
    public List<Renderer> childRenderers;
    public GameObject gameObject;
    public Collider collider;
    public CharacterCamera camera;
    public TagSystemData tagSystemData;
    public float timer;
    public bool disableBecauseInterloper;
    public bool disableRender;
    public float ceilingHeight = 1.75f;
    public float targetAlpha;
    public bool updatedThisLoop;
    Vector3 anchor;
    Dictionary<Renderer, Material> normalMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, Material> interloperMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
    Dictionary<Renderer, string> initialSortingLayers = new Dictionary<Renderer, string>();
    public MaterialController(Collider collider, CharacterCamera camera) {
        this.camera = camera;
        this.gameObject = collider.transform.root.gameObject;
        this.tagSystemData = Toolbox.GetTagData(collider.gameObject);
        this.childRenderers = new List<Renderer>();
        this.childRenderers = new List<Renderer>(gameObject.transform.root.GetComponentsInChildren<Renderer>())
                                    .Where(x => x != null &&
                                                !(x is ParticleSystemRenderer) &&
                                                !(x is LineRenderer)
                                                // !(x is SpriteRenderer) &&
                                                )
                                    .ToList();
        this.collider = collider;

        Transform findAnchor = gameObject.transform.Find("clearSighterAnchor");
        anchor = findAnchor != null ? findAnchor.position : collider.bounds.center;

        this.state = State.opaque;
        this.disableRender = false;
        this.disableBecauseInterloper = false;
        this.timer = 0f;
        this.targetAlpha = 1f;
        this.updatedThisLoop = false;
        foreach (Renderer renderer in childRenderers) {
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            normalMaterials[renderer] = renderer.material;
            initialSortingLayers[renderer] = LayerMask.LayerToName(renderer.gameObject.layer);
            if (renderer.material != null) {
                Texture albedo = renderer.material.mainTexture;
                Material interloperMaterial = new Material(renderer.material);
                interloperMaterial.shader = Resources.Load("Scripts/shaders/InterloperShadow") as Shader;
                interloperMaterial.SetTexture("_Texture", albedo);
                // interloperMaterial.SetFloat("_Smoothness", 0);
                // interloperMaterial.SetFloat("_Glossiness", 0);
                interloperMaterials[renderer] = interloperMaterial;
            }
        }

    }
    public void InterloperStart() {
        timer = 0.1f;
    }
    public bool CeilingCheck(Vector3 playerPosition, float floorHeight, bool debug = false) {

        float otherFloorY = anchor.y - collider.bounds.extents.y;
        float directionY = otherFloorY - playerPosition.y;
        if (debug)
            Debug.Log($"[MaterialController] {gameObject} {directionY} {anchor.y} < {playerPosition.y} = {anchor.y < playerPosition.y} ");



        if (anchor.y < playerPosition.y + 0.05f || anchor.y < floorHeight) {
            // disableRender = false;
            return false;
        }

        // between camera and player and above player
        // if (cullingPlane.GetSide(collider.bounds.center) && (collider.bounds.center.y - playerPosition.y > ceilingHeight * 1.5)) {
        //     // disableRender = true;
        //     return true;
        // } else 
        if (directionY > ceilingHeight) {
            // above player
            return true;

        } else {
            return false;
        }
    }
    public void MakeFadeOut() {
        if (state == State.transparent || state == State.fadeOut)
            return;
        state = State.fadeOut;
        foreach (Renderer renderer in childRenderers) {
            if (renderer == null || interloperMaterials[renderer] == null || renderer.CompareTag("donthide"))
                continue;
            if (renderer is SpriteRenderer) {
                renderer.gameObject.layer = LayerUtil.GetLayer(Layer.clearsighterHide);
                continue;
            }
            renderer.material = interloperMaterials[renderer];
            renderer.material.SetFloat("_TargetAlpha", 1);
            targetAlpha = 1;
        }
    }
    public void MakeFadeIn() {
        if (state == State.opaque || state == State.fadeIn)
            return;
        state = State.fadeIn;
    }
    public void MakeOpaque() {
        if (state == State.opaque)
            return;
        state = State.opaque;
        timer = 0f;
        disableBecauseInterloper = false;
        disableRender = false;
        foreach (Renderer renderer in childRenderers) {
            if (renderer == null || normalMaterials[renderer] == null)
                return;
            if (renderer is SpriteRenderer) {
                renderer.gameObject.layer = LayerMask.NameToLayer(initialSortingLayers[renderer]);
                continue;
            }
            renderer.material = normalMaterials[renderer];
        }
    }
    public void UpdateTargetAlpha(float offAxisLength = 0f) {
        if (childRenderers.Count == 0)
            return;
        if (timer > 0)
            timer -= Time.deltaTime;
        if (timer <= 0) {
            disableBecauseInterloper = false;
        } else {
            disableBecauseInterloper = true;
        }
        float minimumAlpha = disableRender ? 0f : (1f * (offAxisLength / 2f));
        if (state == State.fadeIn) {
            if (targetAlpha < 1) {
                targetAlpha += Time.unscaledDeltaTime * 10f;
            } else {
                MakeOpaque();
            }
        } else if (state == State.fadeOut) {
            if (targetAlpha > minimumAlpha) {
                targetAlpha -= Time.unscaledDeltaTime * 10f;
                targetAlpha = Mathf.Max(targetAlpha, minimumAlpha);
            } else {
                targetAlpha = minimumAlpha;
            }
        }

        targetAlpha = Mathf.Max(0f, targetAlpha);
        targetAlpha = Mathf.Min(1f, targetAlpha);
        // Debug.Log($"update target alpha: {state} {minimumAlpha} {offAxisLength} = {targetAlpha}");

    }
    public void Update() {
        if (childRenderers.Count == 0)
            return;
        if (active() && (camera.state == CameraState.normal || camera.state == CameraState.attractor)) {
            MakeFadeOut();
        } else {
            MakeFadeIn();
        }
        //     Debug.Log($"[update 2] {gameObject} {gameObject.transform.position} {childRenderers.Count} {active()} {state} {targetAlpha}");
        if (state == State.fadeIn || state == State.fadeOut) {
            foreach (Renderer renderer in childRenderers) {
                if (renderer == null || !renderer.enabled || renderer.CompareTag("donthide"))
                    continue;
                if (renderer is SpriteRenderer) {
                    renderer.gameObject.layer = LayerUtil.GetLayer(Layer.clearsighterHide);
                    continue;
                }
                // renderer.material.SetFloat("_TargetAlpha", targetAlpha);
                if (targetAlpha <= 0.1) {
                    renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    renderer.material = normalMaterials[renderer];
                } else {
                    renderer.shadowCastingMode = initialShadowCastingMode[renderer];
                    renderer.material = interloperMaterials[renderer];
                    renderer.material.SetFloat("_TargetAlpha", targetAlpha);
                }
            }
        }
    }

    public bool active() => (disableBecauseInterloper && !tagSystemData.bulletPassthrough && !tagSystemData.dontHideInterloper) ||
                (disableRender && !tagSystemData.dontHideAbove);
}
