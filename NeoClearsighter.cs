using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class NeoClearsighter : MonoBehaviour {
    enum State { normal, showAll }
    State state;
    PointOctree<Renderer> rendererTree;
    public Transform followTransform;
    Transform myTransform;
    WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();
    HashSet<Renderer> previousStaticRendererBatch;
    HashSet<Renderer> previousDynamicRendererBatch;
    Dictionary<Renderer, Vector3> rendererPositions;
    Dictionary<Renderer, Transform> rendererTransforms;
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode;

    Dictionary<Collider, Renderer[]> colliderToRenderer;
    Dictionary<Collider, Transform> dynamicColliderRoot;
    Collider[] colliderHits;
    CharacterCamera myCamera;
    bool initialized;
    // void Awake() {
    //     GameManager.OnInputModeChange += HandleInputModeChange;
    // }
    // void OnDestroy() {
    //     GameManager.OnInputModeChange -= HandleInputModeChange;
    // }

    public void Initialize(Transform followTransform, CharacterCamera camera) {
        this.followTransform = followTransform;
        this.myCamera = camera;

        myTransform = transform;
        colliderHits = new Collider[5000];

        InitializeTree();
        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));
        initialized = true;
    }

    void InitializeTree() {
        rendererPositions = new Dictionary<Renderer, Vector3>();
        colliderToRenderer = new Dictionary<Collider, Renderer[]>();
        dynamicColliderRoot = new Dictionary<Collider, Transform>();
        rendererTransforms = new Dictionary<Renderer, Transform>();
        initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
        previousStaticRendererBatch = new HashSet<Renderer>();
        previousDynamicRendererBatch = new HashSet<Renderer>();
        List<Renderer> staticRenderers = GameObject.FindObjectsOfType<Renderer>().Where(renderer => renderer.isPartOfStaticBatch).ToList();
        rendererTree = new PointOctree<Renderer>(100, Vector3.zero, 1);
        foreach (Renderer renderer in staticRenderers) {
            Vector3 position = renderer.bounds.center - new Vector3(0f, renderer.bounds.extents.y, 0f);
            // TODO: handle anchor
            rendererTree.Add(renderer, position);
            rendererPositions[renderer] = position;
            // if (renderer.transform.root.name.ToLower().Contains("rail")) {
            //     Debug.Log($"[NeoClearSighter] initial shadow casting mode: {renderer.shadowCastingMode}");
            // }
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
        }
    }

    IEnumerator HandleGeometry() {
        if (initialized) {
            if ((myCamera.state == CameraState.normal || myCamera.state == CameraState.attractor)) {
                state = State.normal;
            } else {
                state = State.showAll;
            }

            switch (state) {
                case State.normal:
                    yield return HandleGeometryNormal();
                    break;
                case State.showAll:
                    yield return ShowAllGeometry();
                    break;
            }
        } else {
            yield return null;
        }

    }

    IEnumerator ShowAllGeometry() {
        int j = 0;
        // reset previous batch
        foreach (Renderer renderer in previousStaticRendererBatch.Concat(previousDynamicRendererBatch)) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        }
        previousStaticRendererBatch = new HashSet<Renderer>();
        previousDynamicRendererBatch = new HashSet<Renderer>();
        yield return waitForFrame;
    }

    IEnumerator HandleGeometryNormal() {
        HashSet<Renderer> nextStaticRenderBatch = new HashSet<Renderer>();
        HashSet<Renderer> nextDynamicRenderBatch = new HashSet<Renderer>();
        Vector3 origin = followTransform.position + new Vector3(0f, 1.5f, 0f);
        Ray upRay = new Ray(origin, Vector3.up);
        Renderer[] above = rendererTree.GetNearby(upRay, 20f);
        int j = 0;
        for (int i = 0; i < above.Length; i++) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer renderer = above[i];
            // if (renderer.transform.root.name.ToLower().Contains("rail")) {
            //     Debug.Log($"[NeoClearSighter] static {rendererPositions[renderer].y} > {origin.y}");
            // }
            if (rendererPositions[renderer].y > origin.y) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                nextStaticRenderBatch.Add(renderer);
                if (previousStaticRendererBatch.Contains(renderer)) {
                    previousStaticRendererBatch.Remove(renderer);
                }
            }
        }


        // non-static colliders above me
        int numberHits = Physics.OverlapSphereNonAlloc(origin, 20f, colliderHits, LayerUtil.GetLayerMask(Layer.obj, Layer.bulletPassThrough, Layer.shell, Layer.bulletOnly, Layer.interactive), QueryTriggerInteraction.Ignore);
        for (int k = 0; k < numberHits; k++) {
            Collider collider = colliderHits[k];
            if (collider == null || collider.gameObject == null || collider.transform.IsChildOf(myTransform) || collider.transform.IsChildOf(followTransform))
                continue;
            j += 1;
            if (j > 500) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer[] renderers = GetDynamicRenderers(collider);
            Transform root = dynamicColliderRoot[collider];
            // if (root.name.ToLower().Contains("rail")) {
            //     Debug.Log($"[NeoClearSighter] dynamic: {root.position.y} > {origin.y}");
            // }
            if (root.position.y > origin.y) {
                foreach (Renderer renderer in renderers) {
                    if (nextStaticRenderBatch.Contains(renderer)) continue;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    nextDynamicRenderBatch.Add(renderer);
                    if (previousDynamicRendererBatch.Contains(renderer)) {
                        previousDynamicRendererBatch.Remove(renderer);
                    }
                }
            }
        }

        // reset previous batch
        foreach (Renderer renderer in previousStaticRendererBatch.Concat(previousDynamicRendererBatch)) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            // if (renderer.transform.root.name.ToLower().Contains("rail")) {
            //     Debug.Log($"[NeoClearSighter] previous batch: {renderer} {initialShadowCastingMode[renderer]}");
            // }
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        }

        previousStaticRendererBatch = nextStaticRenderBatch;
        previousDynamicRendererBatch = nextDynamicRenderBatch;
        yield return waitForFrame;
    }

    public Renderer[] GetDynamicRenderers(Collider key) {
        if (colliderToRenderer.ContainsKey(key)) {
            return colliderToRenderer[key];
        } else {
            Renderer[] renderers = key.transform.root.GetComponentsInChildren<Renderer>().Where(x => x != null &&
                                                !(x is ParticleSystemRenderer) &&
                                                !(x is LineRenderer)
                                                ).ToArray();
            colliderToRenderer[key] = renderers;
            dynamicColliderRoot[key] = key.transform.root;
            foreach (Renderer renderer in renderers) {
                rendererTransforms[renderer] = renderer.transform;
                if (!initialShadowCastingMode.ContainsKey(renderer))
                    initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            }
            return renderers;
        }
    }

    public void HandleInputModeChange(InputMode oldInputMode, InputMode newInputMode) {

    }
}
