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
    Dictionary<Collider, Renderer> colliderRenderers;
    HashSet<ClearsightRendererHandler> previousBatch;
    Dictionary<Collider, Renderer[]> dynamicColliderToRenderer;
    Dictionary<Collider, Transform> dynamicColliderRoot;
    Dictionary<Renderer, Transform> rendererTransforms;
    Dictionary<Transform, List<Renderer>> rendererRoots;

    Dictionary<Transform, ClearsightRendererHandler> handlers;

    HashSet<Tuple<ClearsightRendererHandler, ClearsightRendererHandler.State>> currentBatch = new HashSet<Tuple<ClearsightRendererHandler, ClearsightRendererHandler.State>>();
    HashSet<Transform> alreadyHandled = new HashSet<Transform>();
    public Action<float> OnTime;


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
        initialized = true;
        j = 0;


        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));
    }

    void InitializeTree() {
        handlers = new Dictionary<Transform, ClearsightRendererHandler>();

        colliderRenderers = new Dictionary<Collider, Renderer>();
        rendererPositions = new Dictionary<Renderer, Vector3>();
        rendererTree = new PointOctree<Renderer>(100, Vector3.zero, 1);
        rendererBounds = new Dictionary<Renderer, Bounds>();
        // rendererTagData = new Dictionary<Renderer, TagSystemData>();
        dynamicColliderToRenderer = new Dictionary<Collider, Renderer[]>();
        dynamicColliderRoot = new Dictionary<Collider, Transform>();
        rendererTransforms = new Dictionary<Renderer, Transform>();
        rendererBoundsTree = new BoundsOctree<Renderer>(100, Vector3.zero, 0.5f, 1);
        rendererRoots = new Dictionary<Transform, List<Renderer>>();

        previousBatch = new HashSet<ClearsightRendererHandler>();
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
            if (renderer.name.Contains("cutaway")) continue;
            Vector3 position = renderer.bounds.center;
            rendererTree.Add(renderer, position);
            rendererBoundsTree.Add(renderer, renderer.bounds);
            rendererBounds[renderer] = renderer.bounds;
            rendererPositions[renderer] = renderer.bounds.center - renderer.bounds.extents;

            // rendererTagData[renderer] = Toolbox.GetTagData(renderer.gameObject);

            if (rendererRoots.ContainsKey(renderer.transform.root)) {
                rendererRoots[renderer.transform.root].Add(renderer);
            } else {
                rendererRoots[renderer.transform.root] = new List<Renderer>();
                rendererRoots[renderer.transform.root].Add(renderer);
            }


            if (!handlers.ContainsKey(renderer.transform.root)) {
                ClearsightRendererHandler handler = new ClearsightRendererHandler(this, renderer.transform.root, position);
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
    }

    Vector3 getLiftedOrigin() {
        Vector3 origin = followTransform.position;
        // origin.y = Mathf.Round(origin.y * 10f) / 10f;
        float lift = characterController.state == CharacterState.hvac ? 0f : 1.5f;
        return origin + new Vector3(0f, lift, 0f);
        // return origin;
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
            handler.ChangeState(ClearsightRendererHandler.State.forceOpaque);
        }
        previousBatch = new HashSet<ClearsightRendererHandler>();
        yield return null;
    }
    IEnumerator HandleAboveOnly() {
        currentBatch.Clear();
        alreadyHandled.Clear();
        Vector3 liftedOrigin = getLiftedOrigin();

        // static geometry above me
        yield return HandleStaticAbove(j, liftedOrigin);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, liftedOrigin);

        yield return waitForFrame;

        // apply transparency
        yield return ApplyCurrentBatch();

        // reset previous batch to opaque
        yield return ResetPreviousBatch(j);

        previousBatch.UnionWith(currentBatch.Select(tuple => tuple.Item1));
        yield return waitForFrame;
    }

    IEnumerator HandleGeometryNormal() {
        currentBatch.Clear();
        alreadyHandled.Clear();
        Vector3 liftedOrigin = getLiftedOrigin();

        // static geometry above me
        yield return HandleStaticAbove(j, liftedOrigin);

        // static geometry interlopers
        yield return HandleInterlopersRaycast(j);

        // static geometry interlopers
        yield return HandleInterlopersFrustrum(j);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, liftedOrigin);

        yield return waitForFrame;

        // apply transparency
        yield return ApplyCurrentBatch();

        // reset previous batch to opaque
        yield return ResetPreviousBatch(j);

        previousBatch.UnionWith(currentBatch.Select(tuple => tuple.Item1));
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

            if (alreadyHandled.Contains(root)) continue;
            ClearsightRendererHandler handler = handlers[root];

            // we disable geometry above if the floor of the renderer bounds is above the lifted origin point
            // which is player position + 1.5
            if (handler.IsAbove(liftedOrigin)) {
                handler.ChangeState(ClearsightRendererHandler.State.above);
                currentBatch.Add(Tuple.Create(handler, ClearsightRendererHandler.State.above));
                alreadyHandled.Add(root);
                if (previousBatch.Contains(handler)) {
                    previousBatch.Remove(handler);
                }
            }
        }
    }

    IEnumerator HandleInterlopersRaycast(int j) {
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

            Transform root = hit.collider.transform.root;
            if (alreadyHandled.Contains(root)) continue;

            if (handlers.ContainsKey(root)) {
                ClearsightRendererHandler handler = handlers[root];

                Vector3 hitNorm = hit.normal;
                hitNorm.y = 0;
                float normDot = Vector3.Dot(hitNorm, cameraPlanarDirection);
                float rayDot = Vector3.Dot(radarDirections[i + (subradarIndex * NUMBER_DIRECTIONS)], cameraPlanarDirection);
                if (normDot > 0 && (rayDot < 0 || (rayDot > 0 && normDot < 1 - rayDot)) //&& hit.distance > 4f
                     && (handler.bounds.center - followTransform.position).y > 0.2f) {

                    currentBatch.Add(Tuple.Create(handler, ClearsightRendererHandler.State.interloper));
                    alreadyHandled.Add(hit.collider.transform.root);

                    if (previousBatch.Contains(handler)) {
                        previousBatch.Remove(handler);
                    }
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
            Renderer[] renderers = GetDynamicRenderers(collider);
            Transform root = dynamicColliderRoot[collider];
            if (alreadyHandled.Contains(root)) continue;

            if (root.position.y > liftedOrigin.y) {
                foreach (Renderer renderer in renderers) {
                    if (renderer == null) continue;
                    if (renderer.CompareTag("occlusionSpecial")) continue;

                    Transform renderRoot = renderer.transform.root;
                    ClearsightRendererHandler handler = handlers[renderRoot];
                    handler.ChangeState(ClearsightRendererHandler.State.above);
                    currentBatch.Add(Tuple.Create(handler, ClearsightRendererHandler.State.above));
                    alreadyHandled.Add(root);
                    alreadyHandled.Add(renderRoot);
                    if (previousBatch.Contains(handler)) {
                        previousBatch.Remove(handler);
                    }
                }
            }
        }
    }

    IEnumerator HandleInterlopersFrustrum(int j) {
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
            if (renderer == null) continue;

            Transform root = renderer.transform.root;
            if (alreadyHandled.Contains(root)) continue;

            Vector3 rendererPosition = rendererBounds[renderer].center;
            Vector3 directionToInterloper = rendererPosition - followTransform.position;
            if (detectionPlane.GetSide(rendererBounds[renderer].center) == detectionSide && directionToInterloper.y > 0.2f) {
                ClearsightRendererHandler handler = handlers[root];

                currentBatch.Add(Tuple.Create(handler, ClearsightRendererHandler.State.interloper));
                alreadyHandled.Add(root);

                // hiddenTransforms.Add(renderer.transform.root);
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
        foreach (ClearsightRendererHandler removal in handlersToRemove) {
            previousBatch.Remove(removal);
        }
        yield return null;
    }

    IEnumerator ApplyCurrentBatch() {
        foreach (Tuple<ClearsightRendererHandler, ClearsightRendererHandler.State> tuple in currentBatch) {
            ClearsightRendererHandler handler = tuple.Item1;
            ClearsightRendererHandler.State desiredState = tuple.Item2;
            handler.ChangeState(desiredState);
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
                    ClearsightRendererHandler handler = new ClearsightRendererHandler(this, renderer.transform.root, renderer.transform.position);//, isDynamic: true);
                    handlers[renderer.transform.root] = handler;
                }
            }
            return renderers;
        }
    }


    Plane[] interloperFrustrum() {
        float size = 0.15f;
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

    public bool IsObjectVisible(GameObject obj) {
        ClearsightRendererHandler handler;
        if (handlers.TryGetValue(obj.transform.root, out handler)) {
            return handler.IsVisible();
        } else {
            return true;
        }
        // ClearsightRendererHandler handler = handlers[]
    }
}


