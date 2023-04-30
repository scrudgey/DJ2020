using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class NeoClearsighter : MonoBehaviour {
    enum State { normal, showAll, interloperOnly }
    State state;
    PointOctree<Renderer> rendererTree;
    BoundsOctree<Renderer> rendererBoundsTree;
    public Transform followTransform;
    Transform myTransform;
    WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();
    HashSet<Renderer> previousAboveRendererBatch;
    HashSet<Renderer> previousInterloperBatch;
    HashSet<Renderer> previousDynamicRendererBatch;
    Dictionary<Renderer, Vector3> rendererPositions;
    Dictionary<Renderer, Bounds> rendererBounds;
    Dictionary<Renderer, Transform> rendererTransforms;
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode;
    Dictionary<Renderer, TagSystemData> rendererTagData;

    Dictionary<Collider, Renderer[]> colliderToRenderer;
    Dictionary<Collider, Transform> dynamicColliderRoot;
    Collider[] colliderHits;
    CharacterCamera myCamera;
    Transform cameraTransform;
    bool initialized;
    Vector3 previousOrigin;
    private List<Collider> rooftopZones = new List<Collider>();
    // void Awake() {
    //     GameManager.OnInputModeChange += HandleInputModeChange;
    // }
    // void OnDestroy() {
    //     GameManager.OnInputModeChange -= HandleInputModeChange;
    // }

    public void Initialize(Transform followTransform, CharacterCamera camera) {
        this.followTransform = followTransform;
        this.myCamera = camera;

        cameraTransform = myCamera.transform;
        myTransform = transform;
        colliderHits = new Collider[5000];

        InitializeTree();
        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));

        rooftopZones = GameObject.FindObjectsOfType<RooftopZone>()
           .SelectMany(zone => zone.GetComponentsInChildren<Collider>())
           .ToList();
        initialized = true;
    }

    void InitializeTree() {
        rendererPositions = new Dictionary<Renderer, Vector3>();
        rendererBounds = new Dictionary<Renderer, Bounds>();
        colliderToRenderer = new Dictionary<Collider, Renderer[]>();
        dynamicColliderRoot = new Dictionary<Collider, Transform>();
        rendererTransforms = new Dictionary<Renderer, Transform>();
        initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
        rendererTagData = new Dictionary<Renderer, TagSystemData>();
        previousAboveRendererBatch = new HashSet<Renderer>();
        previousInterloperBatch = new HashSet<Renderer>();
        previousDynamicRendererBatch = new HashSet<Renderer>();
        List<Renderer> staticRenderers = GameObject.FindObjectsOfType<Renderer>()
            .Where(renderer => renderer.isPartOfStaticBatch)
            .Concat(
                GameObject.FindObjectsOfType<SpriteRenderer>()
                    .Where(obj => obj.CompareTag("decor"))
                    .Select(obj => obj.GetComponent<Renderer>())
            ).ToList();
        rendererTree = new PointOctree<Renderer>(100, Vector3.zero, 1);
        rendererBoundsTree = new BoundsOctree<Renderer>(100, Vector3.zero, 0.5f, 1);
        foreach (Renderer renderer in staticRenderers) {
            // Vector3 position = renderer.bounds.center - new Vector3(0f, renderer.bounds.extents.y, 0f);
            Vector3 position = renderer.bounds.center;
            // TODO: handle anchor
            rendererTree.Add(renderer, position);
            rendererBoundsTree.Add(renderer, renderer.bounds);
            rendererPositions[renderer] = position;
            rendererBounds[renderer] = renderer.bounds;
            rendererTagData[renderer] = Toolbox.GetTagData(renderer.gameObject);
            // if (renderer.transform.root.name.ToLower().Contains("rail")) {
            //     Debug.Log($"[NeoClearSighter] initial shadow casting mode: {renderer.shadowCastingMode}");
            // }
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            Transform findAnchor = renderer.gameObject.transform.root.Find("clearSighterAnchor");
            if (findAnchor != null) {
                rendererPositions[renderer] = findAnchor.position;
            }
        }
    }

    IEnumerator HandleGeometry() {
        if (initialized) {
            if ((myCamera.state == CameraState.normal || myCamera.state == CameraState.attractor)) {
                state = State.normal;
            } else {
                state = State.showAll;
            }

            // bool inRooftopZone = false;
            foreach (Collider zone in rooftopZones) {
                if (zone == null) continue;
                if (zone.bounds.Contains(followTransform.position)) {
                    state = State.interloperOnly;
                    break;
                }
            }

            switch (state) {
                case State.normal:
                    yield return HandleGeometryNormal();
                    break;
                case State.showAll:
                    yield return ShowAllGeometry();
                    break;
                case State.interloperOnly:
                    yield return InterloperOnly();
                    break;
            }
        } else {
            yield return null;
        }

    }

    IEnumerator ShowAllGeometry() {
        int j = 0;
        // reset previous batch
        foreach (Renderer renderer in previousAboveRendererBatch.Concat(previousDynamicRendererBatch).Concat(previousInterloperBatch)) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        }
        previousAboveRendererBatch = new HashSet<Renderer>();
        previousInterloperBatch = new HashSet<Renderer>();
        previousDynamicRendererBatch = new HashSet<Renderer>();
        yield return waitForFrame;
    }

    IEnumerator HandleGeometryNormal() {
        HashSet<Renderer> nextAboveRenderBatch = new HashSet<Renderer>();
        HashSet<Renderer> nextInterloperBatch = new HashSet<Renderer>();
        HashSet<Renderer> nextDynamicRenderBatch = new HashSet<Renderer>();
        Vector3 origin = followTransform.position;
        if (previousOrigin == null || previousOrigin == Vector3.zero) {
            previousOrigin = origin;
        }
        if (Mathf.Abs(origin.y - previousOrigin.y) > 0.5f) {
            previousOrigin = origin;
        } else {
            origin.y = previousOrigin.y;
        }

        Vector3 liftedOrigin = origin + new Vector3(0f, 1.5f, 0f);
        Ray upRay = new Ray(liftedOrigin, Vector3.up);

        // static geometry above me
        Renderer[] above = rendererTree.GetNearby(upRay, 20f);
        int j = 0;
        for (int i = 0; i < above.Length; i++) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer renderer = above[i];
            Vector3 position = rendererPositions[renderer] - new Vector3(0f, rendererBounds[renderer].extents.y, 0f);
            if (position.y > liftedOrigin.y) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                nextAboveRenderBatch.Add(renderer);
                if (previousAboveRendererBatch.Contains(renderer)) {
                    previousAboveRendererBatch.Remove(renderer);
                }
            }
        }

        // static geometry interlopers
        List<Renderer> interlopers = rendererBoundsTree.GetWithinFrustum(interloperFrustrum());
        Plane detectionPlane = new Plane(-1f * cameraTransform.forward, followTransform.position);

        Plane XPlane = new Plane(Vector3.right, followTransform.position);
        Plane ZPlane = new Plane(Vector3.forward, followTransform.position);

        Toolbox.DrawPlane(followTransform.position, XPlane);
        Toolbox.DrawPlane(followTransform.position, ZPlane);

        bool cameraXSide = XPlane.GetSide(cameraTransform.position);
        bool cameraZSide = ZPlane.GetSide(cameraTransform.position);

        j = 0;
        for (int i = 0; i < interlopers.Count; i++) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }

            Renderer renderer = interlopers[i];

            TagSystemData tagSystemData = rendererTagData[renderer];
            if (tagSystemData.dontHideInterloper) continue;

            Vector3 rendererPosition = rendererBounds[renderer].center;
            Vector3 directionToInterloper = rendererPosition - followTransform.position;

            float xParity = cameraXSide ? 1f : -1f;
            float zParity = cameraZSide ? 1f : -1f;
            bool rendererXSide = XPlane.GetSide(rendererPosition + new Vector3(xParity * rendererBounds[renderer].extents.x, 0f, 0f));
            bool rendererZSide = ZPlane.GetSide(rendererPosition + new Vector3(0f, 0f, zParity * rendererBounds[renderer].extents.z));
            // bool rendererDetectionSide = detectionPlane.GetSide(rendererPosition);
            // if (directionToInterloper.y > 0.2f) { //&& !detectionPlane.GetSide(rendererPosition)
            if (rendererXSide == cameraXSide && rendererZSide == cameraZSide && directionToInterloper.y > 0.2f) {
                if (nextAboveRenderBatch.Contains(renderer)) continue;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                nextInterloperBatch.Add(renderer);
                if (previousInterloperBatch.Contains(renderer)) {
                    previousInterloperBatch.Remove(renderer);
                }
            }
        }

        // non-static colliders above me
        int numberHits = Physics.OverlapSphereNonAlloc(liftedOrigin, 20f, colliderHits, LayerUtil.GetLayerMask(Layer.obj, Layer.bulletPassThrough, Layer.shell, Layer.bulletOnly, Layer.interactive), QueryTriggerInteraction.Ignore);
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
            if (root.position.y > liftedOrigin.y) {
                foreach (Renderer renderer in renderers) {
                    if (nextAboveRenderBatch.Contains(renderer) || nextInterloperBatch.Contains(renderer)) continue;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    nextDynamicRenderBatch.Add(renderer);
                    if (previousDynamicRendererBatch.Contains(renderer)) {
                        previousDynamicRendererBatch.Remove(renderer);
                    }
                }
            }
        }

        // reset previous batch
        foreach (Renderer renderer in previousAboveRendererBatch.Concat(previousDynamicRendererBatch).Concat(previousInterloperBatch)) {
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

        previousAboveRendererBatch = nextAboveRenderBatch;
        previousDynamicRendererBatch = nextDynamicRenderBatch;
        previousInterloperBatch = nextInterloperBatch;
        yield return waitForFrame;
    }
    Plane[] interloperFrustrum() {
        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        float size = 2f;
        Plane left = new Plane(cameraTransform.right, followTransform.position - size * cameraTransform.right);
        Plane right = new Plane(-1f * cameraTransform.right, followTransform.position + size * cameraTransform.right);
        Plane down = new Plane(cameraTransform.up, followTransform.position - size * cameraTransform.up);
        Plane up = new Plane(-1f * cameraTransform.up, followTransform.position + size * cameraTransform.up);
        Plane near = new Plane(cameraTransform.forward, cameraTransform.position);
        Plane far = new Plane(-1f * cameraTransform.forward, followTransform.position);
        return new Plane[] { left, right, down, up, near, far };
    }
    IEnumerator InterloperOnly() {
        HashSet<Renderer> nextInterloperBatch = new HashSet<Renderer>();
        Vector3 origin = followTransform.position;

        float interloperSpread = 2f;
        Ray towardCameraRay = new Ray(origin, cameraTransform.position - origin);
        Ray towardCameraRay2 = new Ray(origin, (interloperSpread * cameraTransform.up + cameraTransform.position) - origin);
        Ray towardCameraRay4 = new Ray(origin, (interloperSpread * cameraTransform.right + cameraTransform.position) - origin);
        Ray towardCameraRay5 = new Ray(origin, (-interloperSpread * cameraTransform.right + cameraTransform.position) - origin);

        int j = 0;

        List<Renderer> interlopers = rendererBoundsTree.GetWithinFrustum(interloperFrustrum());
        Plane detectionPlane = new Plane(cameraTransform.forward, followTransform.position);
        j = 0;
        for (int i = 0; i < interlopers.Count; i++) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer renderer = interlopers[i];

            TagSystemData tagSystemData = rendererTagData[renderer];
            if (tagSystemData.dontHideInterloper) continue;

            Vector3 rendererPosition = rendererPositions[renderer];
            Vector3 directionToInterloper = rendererPosition - followTransform.position;
            if (!detectionPlane.GetSide(rendererPosition) && directionToInterloper.y > 0.2f) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                nextInterloperBatch.Add(renderer);
                if (previousInterloperBatch.Contains(renderer)) {
                    previousInterloperBatch.Remove(renderer);
                }
            }
        }

        // reset previous batch
        foreach (Renderer renderer in previousAboveRendererBatch.Concat(previousDynamicRendererBatch).Concat(previousInterloperBatch)) {
            j++;
            if (j > 100) {
                j = 0;
                yield return waitForFrame;
            }
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        }
        previousAboveRendererBatch = new HashSet<Renderer>();
        previousInterloperBatch = nextInterloperBatch;
        previousDynamicRendererBatch = new HashSet<Renderer>();
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

            Transform findAnchor = key.transform.root.Find("clearSighterAnchor");
            if (findAnchor != null) {
                dynamicColliderRoot[key] = findAnchor;
            }

            foreach (Renderer renderer in renderers) {
                rendererTransforms[renderer] = renderer.transform;
                if (findAnchor != null) {
                    rendererTransforms[renderer] = findAnchor;
                }
                if (!initialShadowCastingMode.ContainsKey(renderer))
                    initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            }
            return renderers;
        }
    }

    public void HandleInputModeChange(InputMode oldInputMode, InputMode newInputMode) {

    }
}
