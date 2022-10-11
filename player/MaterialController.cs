using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Dictionary<Renderer, Material> normalMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, Material> interloperMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
    public MaterialController(Collider collider, CharacterCamera camera) {
        this.camera = camera;
        this.gameObject = collider.gameObject;
        this.tagSystemData = Toolbox.GetTagData(collider.gameObject);
        // this.childRenderers = new List<Renderer>(gameObject.transform.root.GetComponentsInChildren<Renderer>(true)).Where(x => !(x is SpriteRenderer)).ToList();
        this.childRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>()).ToList();
        this.collider = collider;

        this.state = State.opaque;
        this.disableBecauseAbove = false;
        this.disableBecauseInterloper = false;
        this.timer = 0f;
        this.targetAlpha = 1f;
        foreach (Renderer renderer in childRenderers) {
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            normalMaterials[renderer] = renderer.material;
            if (renderer.material != null) {
                Material interloperMaterial = new Material(renderer.material);
                interloperMaterial.shader = Resources.Load("Scripts/shaders/Interloper") as Shader;
                interloperMaterials[renderer] = interloperMaterial;
            }
        }

    }
    public void InterloperStart() {
        timer = 0.1f;
    }
    public void CeilingCheck(Vector3 playerPosition) {
        if (collider.bounds.center.y < playerPosition.y + 0.05f) {
            disableBecauseAbove = false;
            return;
        }

        Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
        Vector3 direction = otherFloor - playerPosition;
        // Debug.Log($"[MaterialController] {gameObject} {direction} {direction.y > ceilingHeight} {collider.bounds.center} {collider.bounds.extents.y} {collider.bounds.center.y < playerPosition.y} ");
        if (direction.y > ceilingHeight) {
            disableBecauseAbove = true;
        } else {
            disableBecauseAbove = false;
        }
    }
    public void MakeFadeOut() {
        if (state == State.transparent || state == State.fadeOut)
            return;
        state = State.fadeOut;
        foreach (Renderer renderer in childRenderers) {
            // TODO: check on renderer tag
            if (renderer == null || interloperMaterials[renderer] == null)
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

        if (timer > 0)
            timer -= Time.deltaTime;
        if (timer <= 0) {
            disableBecauseInterloper = false;
        } else {
            disableBecauseInterloper = true;
        }
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
            foreach (Renderer renderer in childRenderers.Where(renderer =>
                                                                renderer != null &&
                                                                renderer.enabled)) {
                renderer.material.SetFloat("_TargetAlpha", targetAlpha);
                renderer.shadowCastingMode = targetAlpha == 0 ? ShadowCastingMode.ShadowsOnly : initialShadowCastingMode[renderer];
            }
        }

    }

    public bool active() => (disableBecauseInterloper && !tagSystemData.bulletPassthrough && !tagSystemData.dontHideInterloper) ||
                (disableBecauseAbove && !tagSystemData.dontHideAbove);
}
