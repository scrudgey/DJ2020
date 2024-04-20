using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
public class NeoClearsighterV3 : MonoBehaviour {
    enum State { normal, showAll, aboveOnly }
    State state;
    static readonly int BATCHSIZE = 500;
    WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();

    public Transform followTransform;
    Transform cameraTransform;
    Transform myTransform;
    CharacterCamera myCamera;
    CharacterController characterController;
    NativeArray<Vector3> radarDirections;
    NativeArray<RaycastHit> raycastResults;
    NativeArray<RaycastHit> raycastResultsBackBuffer;
    NativeArray<RaycastCommand> commands;
    NativeArray<RaycastHit> results;
    LayerMask defaultLayerMask;
    readonly static int NUMBER_DIRECTIONS = 180;
    readonly static int NUMBER_SUB_RADARS = 2;
    readonly static int jobBatchSize = 18;
    JobHandle gatherJobHandle;
    int subradarIndex;
    PointOctree<Renderer> rendererTree;
    Dictionary<Renderer, Vector3> rendererPositions;
    Dictionary<Renderer, Bounds> rendererBounds;
    BoundsOctree<Renderer> rendererBoundsTree;
    Dictionary<Collider, ClearsightRendererHandler> colliderRenderers;
    Dictionary<ClearsightRendererHandler, ClearsightRendererHandler.CullingState> currentBatch;
    HashSet<ClearsightRendererHandler> unhandledPreviousBatch;
    // Dictionary<Collider, Renderer[]> dynamicColliderToRenderer;
    // Dictionary<Collider, Transform> dynamicColliderRoot;
    // Dictionary<Renderer, Transform> rendererTransforms;
    Dictionary<Transform, List<Renderer>> rendererRoots;

    Dictionary<Transform, ClearsightRendererHandler> handlers;

    HashSet<Transform> alreadyHandledRootTransforms = new HashSet<Transform>();
    public Action<float> OnTime;
    public Vector3 playerPosition;

    Collider[] colliderHits;
    bool initialized;

    int j;


    public void Initialize(Transform followTransform, CharacterCamera camera, CharacterController characterController) {
        this.followTransform = followTransform;
        this.myCamera = camera;
        this.characterController = characterController;

        cameraTransform = myCamera.transform;
        myTransform = transform;
        colliderHits = new Collider[5000];

        defaultLayerMask = LayerUtil.GetLayerMask(Layer.def, Layer.bulletPassThrough, Layer.clearsighterBlock);
        InitializeTree();
        SetUpRadar();
        j = 0;
        initialized = true;
        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));
    }
    void Start() {
        OverlayHandler.OnSelectedNodeChange += HandleNodeFocusChange;
    }

    void HandleNodeFocusChange(INodeCameraProvider indicator) {
        if (indicator == null) {
            followTransform = GameManager.I.playerObject.transform;
        } else {
            string nodeId = indicator.GetNodeId();
            GameObject newFocus = GameManager.I.GetNodeComponent(nodeId);
            followTransform = newFocus.transform;
        }
    }

    void InitializeTree() {
        handlers = new Dictionary<Transform, ClearsightRendererHandler>();

        colliderRenderers = new Dictionary<Collider, ClearsightRendererHandler>();
        rendererPositions = new Dictionary<Renderer, Vector3>();
        rendererTree = new PointOctree<Renderer>(100, Vector3.zero, 1);
        rendererBounds = new Dictionary<Renderer, Bounds>();
        // rendererTagData = new Dictionary<Renderer, TagSystemData>();
        // dynamicColliderToRenderer = new Dictionary<Collider, Renderer[]>();
        // dynamicColliderRoot = new Dictionary<Collider, Transform>();
        rendererBoundsTree = new BoundsOctree<Renderer>(100, Vector3.zero, 0.5f, 1);
        rendererRoots = new Dictionary<Transform, List<Renderer>>();

        unhandledPreviousBatch = new HashSet<ClearsightRendererHandler>();
        currentBatch = new Dictionary<ClearsightRendererHandler, ClearsightRendererHandler.CullingState>();
        // hiddenTransforms = new HashSet<Transform>();


        List<Renderer> staticRenderers = GameObject.FindObjectsOfType<Renderer>()
            .Where(renderer => renderer.isPartOfStaticBatch)
            .Concat(
                GameObject.FindObjectsOfType<SpriteRenderer>()
                    .Where(obj => obj.CompareTag("decor"))
                    .Select(obj => obj.GetComponent<Renderer>())
            )
            .Concat(
                GameObject.FindObjectsOfType<TextMeshPro>()
                    .Select(obj => obj.GetComponent<Renderer>())
            )
            .ToList();


        foreach (Renderer renderer in staticRenderers) {
            AddNewRendererHandler(renderer);
        }
    }
    void SetUpRadar() {
        var allocator = Allocator.Persistent;

        commands = new NativeArray<RaycastCommand>(NUMBER_DIRECTIONS, allocator);
        results = new NativeArray<RaycastHit>(NUMBER_DIRECTIONS, allocator);

        radarDirections = new NativeArray<Vector3>(NUMBER_DIRECTIONS * NUMBER_SUB_RADARS, allocator);
        raycastResults = new NativeArray<RaycastHit>(NUMBER_DIRECTIONS, allocator);
        raycastResultsBackBuffer = new NativeArray<RaycastHit>(NUMBER_DIRECTIONS, allocator);

        // create an array of 360degree angles
        float delta_theta = 360f / (float)NUMBER_DIRECTIONS;
        for (int j = 0; j < NUMBER_SUB_RADARS; j++) {
            float offset = delta_theta / (float)NUMBER_SUB_RADARS * (float)j;
            for (int i = 0; i < NUMBER_DIRECTIONS; i++) {
                float theta = i * delta_theta + offset;
                Quaternion rotation = Quaternion.AngleAxis(theta, Vector3.up);
                radarDirections[i + (j * NUMBER_DIRECTIONS)] = rotation * Vector3.right;
            }
        }
    }

    void OnDestroy() {
        OverlayHandler.OnSelectedNodeChange -= HandleNodeFocusChange;
        DisposeOfNativeArrays();
    }
    void OnApplicationQuit() {
        DisposeOfNativeArrays();
    }
    void DisposeOfNativeArrays() {
        // if (!gatherJobHandle.IsCompleted) {
        gatherJobHandle.Complete();
        // }
        if (radarDirections.IsCreated) {
            radarDirections.Dispose();
            radarDirections = default;
        }
        if (raycastResults.IsCreated) {
            raycastResults.Dispose();
            raycastResults = default;
        }
        if (raycastResultsBackBuffer.IsCreated) {
            raycastResultsBackBuffer.Dispose();
            raycastResultsBackBuffer = default;
        }
        if (commands.IsCreated) {
            commands.Dispose();
            commands = default;
        }
        if (results.IsCreated) {
            results.Dispose();
            results = default;
        }

        colliderRenderers.Clear();
        rendererPositions.Clear();
        rendererTree = null;
        rendererBounds.Clear();
        // dynamicColliderToRenderer.Clear();
        // dynamicColliderRoot.Clear();
        rendererBoundsTree = null;
        rendererRoots.Clear();
        unhandledPreviousBatch.Clear();

        // TODO: something better?
        OnTime = null;
    }

    Vector3 getLiftedOrigin() {
        Vector3 origin = followTransform.position;
        if (myCamera.state == CameraState.overlayView) {
            return origin + 0.5f * Vector3.up;
        } else {
            float lift = characterController.state == CharacterState.hvac ? 0f : 1.5f;
            return origin + lift * Vector3.up;
        }

    }
    IEnumerator HandleGeometry() {
        if (initialized) {
            if ((myCamera.state == CameraState.normal || myCamera.state == CameraState.attractor || myCamera.state == CameraState.overlayView)) {
                state = State.normal;
            } else if (myCamera.state == CameraState.burgle) {
                state = State.aboveOnly;
            } else {
                state = State.showAll;
            }
            switch (state) {
                default:
                case State.normal:
                    yield return HandleGeometryNormal();
                    break;
                case State.aboveOnly:
                    yield return HandleAboveOnly();
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
        foreach (ClearsightRendererHandler handler in unhandledPreviousBatch) {
            // j++;
            // if (j > BATCHSIZE) {
            //     j = 0;
            //     yield return waitForFrame;
            // }
            handler.ChangeState(ClearsightRendererHandler.CullingState.normal);
        }
        unhandledPreviousBatch = new HashSet<ClearsightRendererHandler>();
        yield return null;
    }
    IEnumerator HandleAboveOnly() {
        currentBatch.Clear();
        alreadyHandledRootTransforms.Clear();
        playerPosition = getLiftedOrigin();

        // static geometry above me
        yield return HandleStaticAbove(j, playerPosition);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, playerPosition);

        yield return waitForFrame;

        // apply transparency
        ApplyCurrentBatch();

        // reset previous batch to opaque
        ResetPreviousBatch(j);

        unhandledPreviousBatch = currentBatch.Keys.ToHashSet();
        yield return waitForFrame;
    }

    IEnumerator HandleGeometryNormal() {
        currentBatch.Clear();
        alreadyHandledRootTransforms.Clear();
        playerPosition = getLiftedOrigin();
        Vector3 basePosition = followTransform.position;

        // static geometry above me
        yield return HandleStaticAbove(j, playerPosition);

        // static geometry interlopers
        yield return HandleInterlopersRaycast(j, basePosition);

        // static geometry interlopers
        yield return HandleInterlopersFrustrum(j, playerPosition);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, playerPosition);

        yield return waitForFrame;

        // apply transparency
        ApplyCurrentBatch();

        // reset previous batch to opaque
        ResetPreviousBatch(j);

        // unhandledPreviousBatch = currentBatch.Select(tuple => tuple.Item1).ToHashSet();
        unhandledPreviousBatch = currentBatch.Keys.ToHashSet();
        yield return waitForFrame;
    }

    void UpdateExposure() {
        // wait for jobs from last frame to complete
        gatherJobHandle.Complete();

        // double-buffering
        SwapExposureBackBuffer();

        if (commands.IsCreated)
            commands.Dispose();
        if (results.IsCreated)
            results.Dispose();

        // allocate data shared across jobs
        var allocator = Allocator.TempJob;
        commands = new NativeArray<RaycastCommand>(NUMBER_DIRECTIONS, allocator);
        results = new NativeArray<RaycastHit>(NUMBER_DIRECTIONS, allocator);

        // create setup job
        var setupJob = new ClearsightRaycastSetupJob();
        setupJob.EyePos = followTransform.position + Vector3.up;
        setupJob.directions = radarDirections;
        setupJob.indexOffset = NUMBER_DIRECTIONS * subradarIndex;
        setupJob.layerMask = defaultLayerMask;
        setupJob.Commands = commands;

        // create gather job
        var gatherJob = new ClearsightRaycastGatherJob();
        gatherJob.Results = results;
        gatherJob.output = raycastResultsBackBuffer;

        // schedule setup job
        var setupJobHandle = setupJob.Schedule(NUMBER_DIRECTIONS, jobBatchSize);

        // schedule raycast job
        // specify dependency on setup job
        var rayCastJobHandle = RaycastCommand.ScheduleBatch(
            commands,
            results,
            jobBatchSize,
            setupJobHandle
          );

        // schedule gather job
        // specify dependency on raycast job
        gatherJobHandle = gatherJob.Schedule(NUMBER_DIRECTIONS, jobBatchSize, rayCastJobHandle);

        // kick jobs
        JobHandle.ScheduleBatchedJobs();
    }

    void SwapExposureBackBuffer() {
        var tmp = raycastResults;
        raycastResults = raycastResultsBackBuffer;
        raycastResultsBackBuffer = tmp;

        tmp = default;
    }

    void Update() {
        if (followTransform == null) return;
        UpdateExposure();
        OnTime?.Invoke(Time.unscaledDeltaTime);
    }

    IEnumerator HandleStaticAbove(int j, Vector3 liftedOrigin) {
        Ray upRay = new Ray(liftedOrigin, Vector3.up);
        Renderer[] above = rendererTree.GetNearby(upRay, 50f);
        for (int i = 0; i < above.Length; i++) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer renderer = above[i];
            if (renderer == null) continue;

            Transform root = renderer.transform.root;
            if (alreadyHandledRootTransforms.Contains(root)) continue;
            ClearsightRendererHandler handler = handlers[root];
            // we disable geometry above if the floor of the renderer bounds is above the lifted origin point
            // which is player position + 1.5
            if (handler.IsAbove(liftedOrigin)) {
                handler.ChangeState(ClearsightRendererHandler.CullingState.above);
                currentBatch[handler] = ClearsightRendererHandler.CullingState.above;
                alreadyHandledRootTransforms.Add(root);
                unhandledPreviousBatch.Remove(handler);
            }
        }
    }

    IEnumerator HandleInterlopersRaycast(int j, Vector3 playerposition) {
        Vector3 cameraPlanarDirection = myCamera.idealRotation * Vector3.forward;
        subradarIndex++;
        if (subradarIndex >= NUMBER_SUB_RADARS) subradarIndex = 0;

        cameraPlanarDirection.y = 0;
        Vector3 start = playerposition + Vector3.up;
        // Vector3 start = playerposition;
        for (int i = 0; i < NUMBER_DIRECTIONS; i++) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            RaycastHit hit = raycastResults[i];

            if (hit.collider == null) continue;
            if (hit.collider.name == "cutaway") continue;

            Transform root = hit.collider.transform.root;
            if (alreadyHandledRootTransforms.Contains(root)) continue;

            if (handlers.ContainsKey(root)) {
                ClearsightRendererHandler handler = handlers[root];

                Vector3 hitNorm = hit.normal;
                hitNorm.y = 0;
                float normDot = Vector3.Dot(hitNorm, cameraPlanarDirection);
                float rayDot = Vector3.Dot(radarDirections[i + (subradarIndex * NUMBER_DIRECTIONS)], cameraPlanarDirection);
                if (normDot > 0 && (rayDot < 0 || (rayDot > 0 && normDot < 1 - rayDot)) //&& hit.distance > 4f
                     && (handler.bounds.center - playerposition).y > 0.2f) {

                    currentBatch[handler] = ClearsightRendererHandler.CullingState.interloper;
                    alreadyHandledRootTransforms.Add(root);
                    unhandledPreviousBatch.Remove(handler);
                }
            }
        }
    }

    IEnumerator HandleDynamicAbove(int j, Vector3 liftedOrigin) {
        int numberHits = Physics.OverlapSphereNonAlloc(liftedOrigin, 40f, colliderHits, LayerUtil.GetLayerMask(Layer.obj, Layer.bulletPassThrough, Layer.shell, Layer.bulletOnly, Layer.interactive), QueryTriggerInteraction.Ignore);
        for (int k = 0; k < numberHits; k++) {
            Collider collider = colliderHits[k];
            if (collider == null || collider.name == "cutaway" || collider.gameObject == null || collider.transform.IsChildOf(myTransform) || collider.transform.IsChildOf(followTransform))
                continue;
            j += 1;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            Transform root = collider.transform.root;
            if (alreadyHandledRootTransforms.Contains(root)) continue;

            if (root.position.y > liftedOrigin.y) {
                // Renderer[] renderers = GetDynamicRenderers(collider);
                ClearsightRendererHandler handler = GetDynamicHandler(collider);

                currentBatch[handler] = ClearsightRendererHandler.CullingState.above;
                alreadyHandledRootTransforms.Add(root);
                unhandledPreviousBatch.Remove(handler);
            }
        }
    }

    IEnumerator HandleInterlopersFrustrum(int j, Vector3 playerposition) {
        List<Renderer> interlopers = rendererBoundsTree.GetWithinFrustum(interloperFrustrum(playerposition));

        Plane XPlane = new Plane(Vector3.right, playerposition);
        Plane ZPlane = new Plane(Vector3.forward, playerposition);

        Vector3 planarDisplacement = cameraTransform.position - playerposition;
        planarDisplacement.y = 0;
        planarDisplacement = planarDisplacement.normalized;

        Plane detectionPlane;
        if (Math.Abs(planarDisplacement.x) > Math.Abs(planarDisplacement.z)) {
            detectionPlane = XPlane;
        } else {
            detectionPlane = ZPlane;
        }
        bool detectionSide = detectionPlane.GetSide(cameraTransform.position);

        for (int i = 0; i < interlopers.Count; i++) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }

            Renderer renderer = interlopers[i];
            if (renderer == null) continue;

            Transform root = renderer.transform.root;
            if (alreadyHandledRootTransforms.Contains(root)) continue;

            Vector3 rendererPosition = rendererBounds[renderer].center;
            Vector3 directionToInterloper = rendererPosition - playerposition;
            if (detectionPlane.GetSide(rendererBounds[renderer].center) == detectionSide && directionToInterloper.y > 0.2f) {
                ClearsightRendererHandler handler = handlers[root];
                currentBatch[handler] = ClearsightRendererHandler.CullingState.interloper;
                alreadyHandledRootTransforms.Add(root);
                unhandledPreviousBatch.Remove(handler);
            }
        }
    }

    void ApplyCurrentBatch() {
        foreach (KeyValuePair<ClearsightRendererHandler, ClearsightRendererHandler.CullingState> kvp in currentBatch) {
            kvp.Key.ChangeState(kvp.Value);
        }
    }
    void ResetPreviousBatch(int j) {
        foreach (ClearsightRendererHandler handler in unhandledPreviousBatch) {
            handler.ChangeState(ClearsightRendererHandler.CullingState.normal);
        }
    }


    public static Material NewInterloperMaterial(Renderer renderer) {
        Material interloperMaterial = new Material(renderer.sharedMaterial);
        Texture albedo = renderer.sharedMaterial.mainTexture;
        interloperMaterial.shader = Resources.Load("Scripts/shaders/InterloperShadow") as Shader;
        interloperMaterial.SetTexture("_Texture", albedo);
        return interloperMaterial;
    }

    ClearsightRendererHandler GetDynamicHandler(Collider key) {
        // TODO: what to do if this is called on a static renderer?
        if (handlers.ContainsKey(key.transform.root)) {
            return handlers[key.transform.root];
        } else {
            if (colliderRenderers.ContainsKey(key)) {
                return colliderRenderers[key];
            } else {
                ClearsightRendererHandler handler = null;
                foreach (Renderer renderer in key.transform.root.GetComponentsInChildren<Renderer>()) {
                    handler = AddNewRendererHandler(renderer, isDynamic: true);
                    if (handler != null) break;
                }
                colliderRenderers[key] = handler;
                return handler;
            }
        }
    }

    Plane[] interloperFrustrum(Vector3 playerPosition) {
        float size = 0.15f;
        // float size = 1f;
        // float size = 4f;

        Vector3 followPosition = playerPosition;

        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        float distance = (cameraTransform.position - followPosition).magnitude;
        float angle = (float)Math.Atan((myCamera.Camera.orthographicSize) / distance) * (180f / (float)Math.PI);

        Vector3 leftNormal = Quaternion.AngleAxis(1f * angle, cameraTransform.up) * cameraTransform.right;
        Vector3 rightNormal = Quaternion.AngleAxis(-1f * angle, cameraTransform.up) * (-1f * cameraTransform.right);

        Plane left = new Plane(leftNormal, followPosition - size * cameraTransform.right);
        Plane right = new Plane(rightNormal, followPosition + size * cameraTransform.right);

        Plane down = new Plane(cameraTransform.up, followPosition - size * cameraTransform.up);
        Plane up = new Plane(-1f * cameraTransform.up, followPosition + size * cameraTransform.up);
        Plane near = new Plane(cameraTransform.forward, cameraTransform.position);
        Plane far = new Plane(-1f * cameraTransform.forward, followPosition);

        // Toolbox.DrawPlane(followTransform.position - size * cameraTransform.right, left);
        // Toolbox.DrawPlane(followTransform.position + size * cameraTransform.right, right);
        return new Plane[] { left, right, down, up, near, far };
    }

    public bool IsObjectVisible(GameObject obj) {
        ClearsightRendererHandler handler;
        if (handlers.TryGetValue(obj.transform.root, out handler)) {
            return handler.IsVisible();
        } else {
            return true;
        }
        // ClearsightRendererHandler handler = handlers[]
    }


    public void AddStatic(Transform obj) {
        if (obj.root == obj) {
            // object is base- create new RenderHandler
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>()) {
                AddNewRendererHandler(renderer);
            }
        } else {
            // object is a child- add SubRenderHandler to RenderHandler
            AddSubrenderers(obj.gameObject, obj.root);
        }
    }
    public void RemoveStatic(Transform obj) {
        if (obj.root == obj) {
            // object is base- remove RenderHandler
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>()) {
                RemoveRendererHandler(renderer);
            }
        } else {
            // object is a child- remove SubRenderHandler from RenderHandler
            RemoveSubRenderers(obj.gameObject, obj.root);
        }
    }
    public void AddSubrenderers(GameObject newObject, Transform parentRoot) {
        if (handlers.ContainsKey(parentRoot)) {
            ClearsightRendererHandler handler = handlers[parentRoot];
            foreach (Renderer renderer in newObject.GetComponentsInChildren<Renderer>()) {
                handler.AddSubRenderHandler(renderer);
            }
        }
    }
    public void RemoveSubRenderers(GameObject newObject, Transform parentRoot) {
        if (handlers.ContainsKey(parentRoot)) {
            ClearsightRendererHandler handler = handlers[parentRoot];
            foreach (Renderer renderer in newObject.GetComponentsInChildren<Renderer>()) {
                handler.RemoveSubRenderHandler(renderer);
            }
        }
    }

    public ClearsightRendererHandler AddNewRendererHandler(Renderer renderer, bool isDynamic = false) {
        if (renderer.name.Contains("cutaway")) return null;
        if (handlers.ContainsKey(renderer.transform.root)) {
            return handlers[renderer.transform.root];
        } else {
            Vector3 position = renderer.bounds.center;
            if (!isDynamic) {
                rendererTree.Add(renderer, position);
                rendererBoundsTree.Add(renderer, renderer.bounds);
                rendererPositions[renderer] = renderer.bounds.center - renderer.bounds.extents;
            }
            rendererBounds[renderer] = renderer.bounds;

            // root keyed
            if (rendererRoots.ContainsKey(renderer.transform.root)) {
                rendererRoots[renderer.transform.root].Add(renderer);
            } else {
                rendererRoots[renderer.transform.root] = new List<Renderer>();
                rendererRoots[renderer.transform.root].Add(renderer);
            }

            Transform findAnchor = renderer.gameObject.transform.root.Find("clearSighterAnchor");
            if (findAnchor != null) {
                rendererPositions[renderer] = findAnchor.position;
            }

            // instantiated at the root- it will create a subrenderhandler for each child.
            ClearsightRendererHandler handler = new ClearsightRendererHandler(this, renderer.transform.root, position);
            handlers[renderer.transform.root] = handler;

            foreach (Collider collider in renderer.GetComponentsInChildren<Collider>()) {
                colliderRenderers[collider] = handler;
            }
            return handler;
        }
    }

    public void RemoveRendererHandler(Renderer renderer) {
        rendererTree.Remove(renderer);
        rendererBoundsTree.Remove(renderer, renderer.bounds);
        rendererBounds.Remove(renderer);
        rendererPositions.Remove(renderer);// [renderer] = renderer.bounds.center - renderer.bounds.extents;
        rendererRoots.Remove(renderer.transform.root);
        handlers.Remove(renderer.transform.root);
        foreach (Collider collider in renderer.GetComponentsInChildren<Collider>()) {
            colliderRenderers.Remove(collider);
        }
    }
}


