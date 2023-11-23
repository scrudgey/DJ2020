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

    Transform followTransform;
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
    Dictionary<Collider, Renderer> colliderRenderers;
    HashSet<ClearsightRendererHandler> previousBatch;
    HashSet<Transform> hiddenTransforms;
    Dictionary<Renderer, TagSystemData> rendererTagData;
    Dictionary<Collider, Renderer[]> dynamicColliderToRenderer;
    Dictionary<Collider, Transform> dynamicColliderRoot;
    Dictionary<Renderer, Transform> rendererTransforms;
    Dictionary<Transform, List<Renderer>> rendererRoots;

    Dictionary<Transform, ClearsightRendererHandler> handlers;

    public Action<float> OnTime;


    Collider[] colliderHits;
    bool initialized;

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
        initialized = true;

        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));
    }

    void InitializeTree() {
        handlers = new Dictionary<Transform, ClearsightRendererHandler>();

        colliderRenderers = new Dictionary<Collider, Renderer>();
        rendererPositions = new Dictionary<Renderer, Vector3>();
        rendererTree = new PointOctree<Renderer>(100, Vector3.zero, 1);
        rendererBounds = new Dictionary<Renderer, Bounds>();
        rendererTagData = new Dictionary<Renderer, TagSystemData>();
        dynamicColliderToRenderer = new Dictionary<Collider, Renderer[]>();
        dynamicColliderRoot = new Dictionary<Collider, Transform>();
        rendererTransforms = new Dictionary<Renderer, Transform>();
        rendererBoundsTree = new BoundsOctree<Renderer>(100, Vector3.zero, 0.5f, 1);
        rendererRoots = new Dictionary<Transform, List<Renderer>>();

        previousBatch = new HashSet<ClearsightRendererHandler>();
        hiddenTransforms = new HashSet<Transform>();

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
            if (renderer.name.Contains("cutaway")) continue;
            Vector3 position = renderer.bounds.center;
            rendererTree.Add(renderer, position);
            rendererBoundsTree.Add(renderer, renderer.bounds);
            rendererPositions[renderer] = position;
            rendererBounds[renderer] = renderer.bounds;
            rendererTagData[renderer] = Toolbox.GetTagData(renderer.gameObject);

            if (rendererRoots.ContainsKey(renderer.transform.root)) {
                rendererRoots[renderer.transform.root].Add(renderer);
            } else {
                rendererRoots[renderer.transform.root] = new List<Renderer>();
                rendererRoots[renderer.transform.root].Add(renderer);
            }


            if (!handlers.ContainsKey(renderer.transform.root)) {
                ClearsightRendererHandler handler = new ClearsightRendererHandler(this, renderer.transform.root, position, renderer.bounds);
                handlers[renderer.transform.root] = handler;
            }

            Transform findAnchor = renderer.gameObject.transform.root.Find("clearSighterAnchor");
            if (findAnchor != null) {
                rendererPositions[renderer] = findAnchor.position;
            }
            foreach (Collider collider in renderer.GetComponentsInChildren<Collider>()) {
                colliderRenderers[collider] = renderer;
            }
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
        DisposeOfNativeArrays();
    }
    void OnApplicationQuit() {
        DisposeOfNativeArrays();
    }
    void DisposeOfNativeArrays() {
        Debug.Log("disposing of native arrays");
        if (radarDirections.IsCreated)
            radarDirections.Dispose();
        if (raycastResults.IsCreated)
            raycastResults.Dispose();
        if (raycastResultsBackBuffer.IsCreated)
            raycastResultsBackBuffer.Dispose();
        if (commands.IsCreated)
            commands.Dispose();
        if (results.IsCreated)
            results.Dispose();
    }

    Vector3 getLiftedOrigin() {
        Vector3 origin = followTransform.position;
        origin.y = Mathf.Round(origin.y * 10f) / 10f;
        float lift = characterController.state == CharacterState.hvac ? 0f : 1.5f;
        return origin + new Vector3(0f, lift, 0f);
    }
    IEnumerator HandleGeometry() {
        if (initialized) {
            if ((myCamera.state == CameraState.normal || myCamera.state == CameraState.attractor)) {
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
        foreach (ClearsightRendererHandler handler in previousBatch) {
            // j++;
            // if (j > BATCHSIZE) {
            //     j = 0;
            //     yield return waitForFrame;
            // }
            handler.ChangeState(ClearsightRendererHandler.State.opaque);
        }
        previousBatch = new HashSet<ClearsightRendererHandler>();
        yield return null;
    }
    IEnumerator HandleAboveOnly() {
        HashSet<ClearsightRendererHandler> currentBatch = new HashSet<ClearsightRendererHandler>();

        Vector3 liftedOrigin = getLiftedOrigin();
        int j = 0;

        // static geometry above me
        yield return HandleStaticAbove(j, liftedOrigin, currentBatch);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, liftedOrigin, currentBatch);

        yield return waitForFrame;

        // apply transparency
        yield return ApplyCurrentBatch(currentBatch);

        // reset previous batch to opaque
        yield return ResetPreviousBatch(j);

        previousBatch.UnionWith(currentBatch);
        yield return waitForFrame;
    }

    IEnumerator HandleGeometryNormal() {
        HashSet<ClearsightRendererHandler> currentBatch = new HashSet<ClearsightRendererHandler>();
        HashSet<Transform> raycastHits = new HashSet<Transform>();

        Vector3 liftedOrigin = getLiftedOrigin();
        int j = 0;

        // static geometry above me
        yield return HandleStaticAbove(j, liftedOrigin, currentBatch);

        // static geometry interlopers
        yield return HandleInterlopersRaycast(j, currentBatch, raycastHits);

        // static geometry interlopers
        yield return HandleInterlopersFrustrum(j, currentBatch, raycastHits);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, liftedOrigin, currentBatch);

        yield return waitForFrame;

        // apply transparency
        yield return ApplyCurrentBatch(currentBatch);

        // reset previous batch to opaque
        yield return ResetPreviousBatch(j);

        previousBatch.UnionWith(currentBatch);
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
        // setupJob.indexOffset = 0;
        // Debug.Log(setupJob.indexOffset);
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
    }

    void Update() {
        UpdateExposure();
        OnTime?.Invoke(Time.unscaledDeltaTime);
    }

    IEnumerator HandleStaticAbove(int j, Vector3 liftedOrigin, HashSet<ClearsightRendererHandler> nextAboveRenderBatch) {
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
            // floor of the collider
            Vector3 position = rendererPositions[renderer] - new Vector3(0f, rendererBounds[renderer].extents.y, 0f);

            // we disable geometry above if the floor of the renderer bounds is above the lifted origin point
            // which is player position + 1.5
            if (position.y > liftedOrigin.y) {
                ClearsightRendererHandler handler = handlers[renderer.transform.root];
                handler.ChangeState(ClearsightRendererHandler.State.above);
                nextAboveRenderBatch.Add(handler);
                hiddenTransforms.Add(renderer.transform.root);
                if (previousBatch.Contains(handler)) {
                    previousBatch.Remove(handler);
                }
            }
        }
    }

    IEnumerator HandleInterlopersRaycast(int j, HashSet<ClearsightRendererHandler> currentBatch, HashSet<Transform> raycastHits) {
        Vector3 cameraPlanarDirection = myCamera.idealRotation * Vector3.forward;
        subradarIndex++;
        if (subradarIndex >= NUMBER_SUB_RADARS) subradarIndex = 0;

        cameraPlanarDirection.y = 0;
        Vector3 start = followTransform.position + Vector3.up;
        for (int i = 0; i < NUMBER_DIRECTIONS; i++) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            RaycastHit hit = raycastResults[i];

            if (hit.collider == null) continue;
            if (hit.collider.name == "cutaway") continue;

            raycastHits.Add(hit.collider.transform.root);
            if (handlers.ContainsKey(hit.collider.transform.root)) {
                ClearsightRendererHandler handler = handlers[hit.collider.transform.root];
                if (currentBatch?.Contains(handler) ?? false) continue;
                // if (rendererTagData[renderer].dontHideInterloper) continue;

                Vector3 hitNorm = hit.normal;
                hitNorm.y = 0;
                float normDot = Vector3.Dot(hitNorm, cameraPlanarDirection);
                float rayDot = Vector3.Dot(radarDirections[i + (subradarIndex * NUMBER_DIRECTIONS)], cameraPlanarDirection);
                if (normDot > 0 && (rayDot < 0 || (rayDot > 0 && normDot < 1 - rayDot)) //&& hit.distance > 4f
                     && (handler.bounds.center - followTransform.position).y > 0.2f) {
                    currentBatch.Add(handler);
                    if (previousBatch.Contains(handler)) {
                        previousBatch.Remove(handler);
                    }
                }
            } else {
                Debug.LogWarning($"renderer root not found : {hit.collider.transform.root}");
            }

        }
    }

    IEnumerator HandleDynamicAbove(int j, Vector3 liftedOrigin, HashSet<ClearsightRendererHandler> currentBatch) {
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
            Renderer[] renderers = GetDynamicRenderers(collider);
            Transform root = dynamicColliderRoot[collider];

            if (root.position.y > liftedOrigin.y) {
                foreach (Renderer renderer in renderers) {
                    ClearsightRendererHandler handler = handlers[renderer.transform.root];

                    if (renderer == null) continue;
                    if (currentBatch.Contains(handler)) continue;
                    if (renderer.CompareTag("occlusionSpecial")) continue;

                    handler.ChangeState(ClearsightRendererHandler.State.above);
                    currentBatch.Add(handler);
                    hiddenTransforms.Add(root);
                    if (previousBatch.Contains(handler)) {
                        previousBatch.Remove(handler);
                    }
                }
            }
        }
    }

    IEnumerator HandleInterlopersFrustrum(int j, HashSet<ClearsightRendererHandler> currentBatch, HashSet<Transform> raycastHits) {
        List<Renderer> interlopers = rendererBoundsTree.GetWithinFrustum(interloperFrustrum());

        Plane XPlane = new Plane(Vector3.right, followTransform.position);
        Plane ZPlane = new Plane(Vector3.forward, followTransform.position);

        Vector3 planarDisplacement = cameraTransform.position - followTransform.position;
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
            if (raycastHits.Contains(renderer.transform.root)) continue;
            // if (renderer == null) continue;
            // if (renderer.name == "cutaway") continue;

            TagSystemData tagSystemData = rendererTagData[renderer];
            if (tagSystemData.dontHideInterloper) continue;

            Vector3 rendererPosition = rendererBounds[renderer].center;
            Vector3 directionToInterloper = rendererPosition - followTransform.position;
            if (detectionPlane.GetSide(rendererBounds[renderer].center) == detectionSide && directionToInterloper.y > 0.2f) {
                ClearsightRendererHandler handler = handlers[renderer.transform.root];
                if (renderer == null) continue;
                if (currentBatch?.Contains(handler) ?? false) continue;
                currentBatch.Add(handler);
                hiddenTransforms.Add(renderer.transform.root);
                if (previousBatch.Contains(handler)) {
                    previousBatch.Remove(handler);
                }
            }
        }
    }

    IEnumerator ResetPreviousBatch(int j) {
        HashSet<ClearsightRendererHandler> handlersToRemove = new HashSet<ClearsightRendererHandler>();
        Vector3 playerPosition = getLiftedOrigin();
        foreach (ClearsightRendererHandler handler in previousBatch) {
            // j++;
            // if (j > BATCHSIZE) {
            //     j = 0;
            //     yield return waitForFrame;
            // }
            bool handlerIsDone = handler.Update(playerPosition);
            if (handlerIsDone) {
                handlersToRemove.Add(handler);
            }
        }
        yield return null;
    }

    IEnumerator ApplyCurrentBatch(HashSet<ClearsightRendererHandler> currentBatch) {
        foreach (ClearsightRendererHandler handler in currentBatch) {
            handler.ChangeState(ClearsightRendererHandler.State.transparent);
        }
        yield return null;
    }

    public static Material NewInterloperMaterial(Renderer renderer) {
        Material interloperMaterial = new Material(renderer.sharedMaterial);
        Texture albedo = renderer.sharedMaterial.mainTexture;
        interloperMaterial.shader = Resources.Load("Scripts/shaders/InterloperShadow") as Shader;
        interloperMaterial.SetTexture("_Texture", albedo);
        return interloperMaterial;
    }

    public Renderer[] GetDynamicRenderers(Collider key) {
        if (dynamicColliderToRenderer.ContainsKey(key)) {
            return dynamicColliderToRenderer[key];
        } else {
            Renderer[] renderers = key.transform.root.GetComponentsInChildren<Renderer>().Where(x => x != null &&
                                                !(x is ParticleSystemRenderer) &&
                                                !(x is LineRenderer)
                                                ).ToArray();
            dynamicColliderToRenderer[key] = renderers;
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
                if (!handlers.ContainsKey(renderer.transform.root)) {
                    ClearsightRendererHandler handler = new ClearsightRendererHandler(this, renderer.transform.root, renderer.transform.position, renderer.bounds);
                    handlers[renderer.transform.root] = handler;
                }
            }
            return renderers;
        }
    }


    Plane[] interloperFrustrum() {
        float size = 0.5f;
        // float size = 1f;
        // float size = 4f;


        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        float distance = (cameraTransform.position - followTransform.position).magnitude;
        float angle = (float)Math.Atan((myCamera.Camera.orthographicSize) / distance) * (180f / (float)Math.PI);

        Vector3 leftNormal = Quaternion.AngleAxis(1f * angle, cameraTransform.up) * cameraTransform.right;
        Vector3 rightNormal = Quaternion.AngleAxis(-1f * angle, cameraTransform.up) * (-1f * cameraTransform.right);

        Plane left = new Plane(leftNormal, followTransform.position - size * cameraTransform.right);
        Plane right = new Plane(rightNormal, followTransform.position + size * cameraTransform.right);

        Plane down = new Plane(cameraTransform.up, followTransform.position - size * cameraTransform.up);
        Plane up = new Plane(-1f * cameraTransform.up, followTransform.position + size * cameraTransform.up);
        Plane near = new Plane(cameraTransform.forward, cameraTransform.position);
        Plane far = new Plane(-1f * cameraTransform.forward, followTransform.position);

        // Toolbox.DrawPlane(followTransform.position - size * cameraTransform.right, left);
        // Toolbox.DrawPlane(followTransform.position + size * cameraTransform.right, right);
        return new Plane[] { left, right, down, up, near, far };
    }
}


