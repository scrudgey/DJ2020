using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
public class MaterialController {
    enum State { opaque, transparent, fadeOut, fadeIn }
    State state;
    public List<Renderer> childRenderers;
    public GameObject gameObject;
    public Collider collider;
    public CharacterCamera camera;
    public TagSystemData tagSystemData;
    public float timer;
    public bool disableBecauseInterloper;
    public bool disableBecauseAbove;
    public float ceilingHeight = 1.5f;
    public float targetAlpha;
    public bool updatedThisLoop;
    Dictionary<Renderer, Material> normalMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, Material> interloperMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
    public MaterialController(Collider collider, CharacterCamera camera) {
        this.camera = camera;
        this.gameObject = collider.gameObject;
        this.tagSystemData = Toolbox.GetTagData(collider.gameObject);
        this.childRenderers = new List<Renderer>();
        this.childRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>())
                                    .Where(x => x != null &&
                                                !(x is ParticleSystemRenderer) &&
                                                !(x is LineRenderer)
                                                // !(x is SpriteRenderer) &&
                                                )
                                    .ToList();
        this.collider = collider;
        this.state = State.opaque;
        this.disableBecauseAbove = false;
        this.disableBecauseInterloper = false;
        this.timer = 0f;
        this.targetAlpha = 1f;
        this.updatedThisLoop = false;
        foreach (Renderer renderer in childRenderers) {
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            normalMaterials[renderer] = renderer.material;
            if (renderer.material != null) {
                Texture albedo = renderer.material.mainTexture;
                Material interloperMaterial = new Material(renderer.material);
                // interloperMaterial.shader = Resources.Load("Scripts/shaders/Interloper") as Shader;
                interloperMaterial.shader = Resources.Load("Scripts/shaders/InterloperShadow") as Shader;
                interloperMaterial.SetTexture("_Texture", albedo);
                // interloperMaterial.SetFloat("_Smoothness", 0);
                // interloperMaterial.SetFloat("_Glossiness", 0);
                // interloperMaterial
                // Debug.Log("loaded interloper material " + interloperMaterial);
                interloperMaterials[renderer] = interloperMaterial;
            }
        }

    }
    public void InterloperStart() {
        timer = 0.1f;
    }
    public void CeilingCheck(Vector3 playerPosition, Plane cullingPlane) {
        if (collider.bounds.center.y < playerPosition.y + 0.05f) {
            disableBecauseAbove = false;
            return;
        }

        float otherFloorY = collider.bounds.center.y - collider.bounds.extents.y;
        float directionY = otherFloorY - playerPosition.y;
        // Debug.Log($"[MaterialController] {gameObject} {direction} {direction.y > ceilingHeight} {collider.bounds.center} {collider.bounds.extents.y} {collider.bounds.center.y < playerPosition.y} ");
        if (gameObject.name.ToLower().Contains("cube")) {
            Debug.Log($"[MaterialController] {gameObject} {directionY} {directionY} > {ceilingHeight} = {directionY > ceilingHeight}, {collider.bounds.center} {collider.bounds.extents.y} {collider.bounds.center.y < playerPosition.y} ");
        }
        if (cullingPlane.GetSide(collider.bounds.center) && (collider.bounds.center.y - playerPosition.y > ceilingHeight * 1.5)) {
            disableBecauseAbove = true;
        } else if (directionY > ceilingHeight) {
            disableBecauseAbove = true;
        } else {
            disableBecauseAbove = false;
        }
    }
    public void MakeFadeOut() {
        if (state == State.transparent || state == State.fadeOut)
            return;
        state = State.fadeOut;
        // Debug.Log($"fadeout: {gameObject}");
        // TODO: not working?
        foreach (Renderer renderer in childRenderers) {
            if (renderer == null || interloperMaterials[renderer] == null || renderer.CompareTag("donthide"))
                continue;
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
        disableBecauseAbove = false;
        foreach (Renderer renderer in childRenderers) {
            if (renderer == null || normalMaterials[renderer] == null)
                return;
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
        float minimumAlpha = disableBecauseAbove ? 0f : (1f * (offAxisLength / 2f));
        if (state == State.fadeIn) {
            if (targetAlpha < 1) {
                targetAlpha += Time.unscaledDeltaTime * 3f;
            } else {
                MakeOpaque();
            }
        } else if (state == State.fadeOut) {
            if (targetAlpha > minimumAlpha) {
                targetAlpha -= Time.unscaledDeltaTime * 3f;
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
        if (state == State.fadeIn || state == State.fadeOut) {
            foreach (Renderer renderer in childRenderers) {
                if (renderer == null || !renderer.enabled || renderer.CompareTag("donthide"))
                    continue;
                renderer.material.SetFloat("_TargetAlpha", targetAlpha);
                // Debug.Log($"{gameObject} disableBecauseInterloper: {disableBecauseInterloper} disableBecauseAbove: {disableBecauseAbove} targetAlpha: {targetAlpha}");
                renderer.shadowCastingMode = (targetAlpha <= 0.01) || disableBecauseAbove ? ShadowCastingMode.ShadowsOnly : initialShadowCastingMode[renderer];
            }
        }
    }

    public bool active() => (disableBecauseInterloper && !tagSystemData.bulletPassthrough && !tagSystemData.dontHideInterloper) ||
                (disableBecauseAbove && !tagSystemData.dontHideAbove);
}
