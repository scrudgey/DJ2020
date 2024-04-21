using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class NeoClearsighterV4 : MonoBehaviour {
    enum State { normal, showAll, aboveOnly }
    enum FloorState { visible, invisible }
    State state;
    static readonly int BATCHSIZE = 500;
    readonly static int jobBatchSize = 18;

    public Transform followTransform;
    public CharacterCamera characterCamera;
    public CharacterController characterController;
    float betweenFloorBuffer = 1f;

    Vector3 playerPosition;
    int playerFloor;
    int playerBetweenFloorHigh;
    int playerBetweenFloorLow;
    string playerRoofZone;

    WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();
    CullingVolume cullingVolume;
    Dictionary<string, CullingComponent> cullingComponents;
    Dictionary<string, Dictionary<int, List<CullingComponent>>> cullingComponentsByZoneAndFloors;
    Dictionary<string, Dictionary<int, FloorState>> floorStatesPerRoofZone;
    Dictionary<string, FloorState> roofZoneStates;
    Dictionary<string, RooftopZone> roofZones;
    Dictionary<Collider, CullingComponent> dynamicCullingComponents;


    bool initialized;
    Dictionary<CullingComponent, int> activeInterlopers;
    Dictionary<CullingComponent, int> activeDynamicInterlopers;

    Dictionary<string, int> activeRoofZoneInterlopers;


    // async raycast components
    JobHandle gatherJobHandle;
    NativeArray<Vector3> radarDirections;
    NativeArray<float> distances;
    NativeArray<RaycastHit> raycastResults;
    NativeArray<RaycastHit> raycastResultsBackBuffer;
    NativeArray<RaycastCommand> commands;
    NativeArray<RaycastHit> results;
    LayerMask defaultLayerMask;
    List<CullingGridPoint> previousFramePoints;
    List<CullingGridPoint> points;

    List<CullingCommand> commandQueue;

    Coroutine floorRoutine;
    Collider[] colliderHits;



    public static string PathToCullingDataResource(string levelName, string sceneName) {
        return $"data/missions/{levelName}/culling_{sceneName}";
    }
    public void Initialize(Transform focus, CharacterCamera characterCamera, CharacterController characterController, LevelTemplate template, string sceneName) {
        this.followTransform = focus;
        this.characterCamera = characterCamera;
        this.characterController = characterController;
        defaultLayerMask = LayerUtil.GetLayerMask(Layer.def, Layer.bulletPassThrough, Layer.clearsighterBlock);

        commandQueue = new List<CullingCommand>();
        cullingComponents = new Dictionary<string, CullingComponent>();
        cullingComponentsByZoneAndFloors = new Dictionary<string, Dictionary<int, List<CullingComponent>>>();//  Dictionary<int, List<CullingComponent>>();
        floorStatesPerRoofZone = new Dictionary<string, Dictionary<int, FloorState>>();// Dictionary<int, FloorState>();
        roofZoneStates = new Dictionary<string, FloorState>();
        roofZones = new Dictionary<string, RooftopZone>();
        dynamicCullingComponents = new Dictionary<Collider, CullingComponent>();
        colliderHits = new Collider[5000];


        previousFramePoints = new List<CullingGridPoint>();
        points = new List<CullingGridPoint>();
        activeInterlopers = new Dictionary<CullingComponent, int>();
        activeDynamicInterlopers = new Dictionary<CullingComponent, int>();
        activeRoofZoneInterlopers = new Dictionary<string, int>();


        cullingVolume = CullingVolume.Load(template.levelName, sceneName);

        roofZoneStates["-1"] = FloorState.visible;
        floorStatesPerRoofZone["-1"] = new Dictionary<int, FloorState>();
        cullingComponentsByZoneAndFloors["-1"] = new Dictionary<int, List<CullingComponent>>();
        for (int i = -1; i < template.floorHeights.Count + 1; i++) {
            floorStatesPerRoofZone["-1"][i] = FloorState.visible;
            cullingComponentsByZoneAndFloors["-1"][i] = new List<CullingComponent>();
        }

        foreach (RooftopZone zone in GameObject.FindObjectsOfType<RooftopZone>()) {
            roofZoneStates[zone.idn] = FloorState.visible;
            roofZones[zone.idn] = zone;
            floorStatesPerRoofZone[zone.idn] = new Dictionary<int, FloorState>();
            cullingComponentsByZoneAndFloors[zone.idn] = new Dictionary<int, List<CullingComponent>>();
            for (int i = -1; i < template.floorHeights.Count + 1; i++) {
                floorStatesPerRoofZone[zone.idn][i] = FloorState.visible;
                cullingComponentsByZoneAndFloors[zone.idn][i] = new List<CullingComponent>();
            }
        }

        foreach (CullingComponent cullingComponent in GameObject.FindObjectsOfType<CullingComponent>()) {
            int floor = cullingComponent.floor;
            cullingComponentsByZoneAndFloors[cullingComponent.rooftopZoneIdn][floor].Add(cullingComponent);
            cullingComponents[cullingComponent.idn] = cullingComponent;
            cullingComponent.Initialize();
        }
        Debug.Log($"cached {cullingComponents.Count} culling components...");

        playerPosition = followTransform.position;
        playerRoofZone = "-1";
        playerFloor = -99;

        RectifyPlayerFloor();

        InitializeNativeArrays(2000);
        initialized = true;
        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));
        StartCoroutine(Toolbox.RunJobRepeatedly(HandleDynamicAbove));
    }
    void InitializeNativeArrays(int NUMBER_DIRECTIONS) {
        var allocator = Allocator.Persistent;
        radarDirections = new NativeArray<Vector3>(NUMBER_DIRECTIONS, allocator);
        distances = new NativeArray<float>(NUMBER_DIRECTIONS, allocator);
        raycastResults = new NativeArray<RaycastHit>(NUMBER_DIRECTIONS, allocator);
        raycastResultsBackBuffer = new NativeArray<RaycastHit>(NUMBER_DIRECTIONS, allocator);
    }


    /**------------------------------------------------------------------------
     *                           main loop
     *------------------------------------------------------------------------**/
    void TransitionToState(State newstate) {
        if (newstate == state) return;
        // exit state
        switch (state) {
            case State.normal:
                break;
            case State.aboveOnly:
                SetFloorCulling();
                break;
            case State.showAll:
                SetFloorCulling();
                break;
        }
        state = newstate;
    }
    IEnumerator HandleGeometry() {
        while (!initialized || followTransform == null) {
            yield return waitForFrame;
        }

        switch (characterCamera.state) {
            case CameraState.normal:
            case CameraState.attractor:
            case CameraState.overlayView:
                TransitionToState(State.normal);
                break;
            case CameraState.burgle:
                TransitionToState(State.aboveOnly);
                break;
            default:
                TransitionToState(State.showAll);
                break;
        }

        int j = 0;
        commandQueue.Clear();

        playerPosition = followTransform.position;
        RectifyPlayerFloor();

        if (state == State.normal) {

            yield return GatherRaycastResults();

            yield return EnqueueInterloperCulling(j, playerPosition, playerFloor);

        } else if (state == State.showAll) {

            for (int i = cullingVolume.floorHeights.Count - 1; i > 0; i--) {
                yield return ChangeFloorState(i, 0);
            }

        }

        yield return ApplyCommands(playerPosition);

        yield return waitForFrame;

        yield return ApplyTimestep();

    }




    /**========================================================================
     *                           floor logic
     *========================================================================**/

    void RectifyPlayerFloor() {
        bool resetFloorCulling = false;
        string newPlayerRoofzone = "-1";

        // set floor and roof zone
        int newPlayerFloor = cullingVolume.GetFloorIndexPosition(playerPosition);
        int transitionFloor = cullingVolume.GetTransitionFloor(playerPosition, betweenFloorBuffer);
        if (transitionFloor != playerBetweenFloorLow) {
            resetFloorCulling = true;
        }
        if (transitionFloor != -99) {
            playerBetweenFloorHigh = transitionFloor + 1;
            playerBetweenFloorLow = transitionFloor;
            // Debug.Log($"between floors {playerBetweenFloorLow}-{playerBetweenFloorHigh}");
        } else {
            playerBetweenFloorHigh = -99;
            playerBetweenFloorLow = -99;
        }

        foreach (RooftopZone zone in roofZones.Values) {
            if (zone.ContainsPoint(playerPosition)) {
                newPlayerRoofzone = zone.idn;
            }
        }

        // check for differences
        if (newPlayerFloor != playerFloor) {
            resetFloorCulling = true;
        }
        if (newPlayerRoofzone != playerRoofZone) {
            roofZoneStates[newPlayerRoofzone] = FloorState.invisible;
            roofZoneStates[playerRoofZone] = FloorState.visible;
            playerRoofZone = newPlayerRoofzone;
            resetFloorCulling = true;
        }
        playerFloor = newPlayerFloor;

        // set floor / ceiling culling accordingly
        if (resetFloorCulling) {
            SetFloorCulling();
        }

    }
    void SetFloorCulling() {
        if (floorRoutine != null) {
            StopCoroutine(floorRoutine);
        }
        floorRoutine = StartCoroutine(HideAllAboveFloors());
    }
    IEnumerator HideAllAboveFloors() {
        // int floor = playerFloor;
        int k = 0;
        for (int i = cullingVolume.floorHeights.Count - 1; i > playerFloor; i--) {
            yield return ChangeFloorState(i, k);
        }
        yield return ChangeFloorState(playerFloor, k);
    }
    IEnumerator ChangeFloorState(int floorIndex, int k) {
        foreach (KeyValuePair<string, Dictionary<int, FloorState>> kvp in floorStatesPerRoofZone) {
            string zoneId = kvp.Key;
            Dictionary<int, FloorState> floorStates = kvp.Value;

            // if playerfloor >= floorIndex or (the player is not in the roof zone and the roof zone is not an interloper), the desired state is visible
            // if there is a between floor
            FloorState desiredState;
            int maxFloor = Mathf.Max(playerBetweenFloorHigh - 1, playerFloor);
            if (maxFloor >= floorIndex || (playerRoofZone != zoneId && !activeRoofZoneInterlopers.ContainsKey(zoneId))) {
                desiredState = FloorState.visible;
            } else {
                desiredState = FloorState.invisible;
            }

            if (floorStates[floorIndex] != desiredState) {
                List<CullingComponent> floorComponents = cullingComponentsByZoneAndFloors[zoneId][floorIndex];

                for (int j = 0; j < floorComponents.Count; j++) {
                    CullingComponent cullingComponent = floorComponents[j];

                    if (desiredState == FloorState.visible) {
                        cullingComponent.ChangeState(CullingComponent.CullingState.normal);
                    } else if (desiredState == FloorState.invisible) {
                        cullingComponent.ChangeState(CullingComponent.CullingState.above);
                    }

                    if (activeInterlopers.ContainsKey(cullingComponent)) {
                        activeInterlopers.Remove(cullingComponent);
                    }

                    k++;
                    if (k > BATCHSIZE) {
                        k = 0;
                        yield return waitForFrame;
                    }
                }
                floorStates[floorIndex] = desiredState;
            }
        }
    }

    /**------------------------------------------------------------------------
     *                           raycast job handling
     *------------------------------------------------------------------------**/
    void SwapExposureBackBuffer() {
        points = previousFramePoints;

        var tmp = raycastResults;
        raycastResults = raycastResultsBackBuffer;
        raycastResultsBackBuffer = tmp;

        tmp = default;
    }

    IEnumerator GatherRaycastResults() {
        // wait for jobs from last frame to complete
        gatherJobHandle.Complete();

        // double-buffering
        SwapExposureBackBuffer();

        if (commands.IsCreated)
            commands.Dispose();
        if (results.IsCreated)
            results.Dispose();


        var allocator = Allocator.TempJob;
        previousFramePoints = cullingVolume.SubgridAroundWorldPoint(followTransform.position, 15f);
        if (playerBetweenFloorHigh != -99) {
            List<CullingGridPoint> additionalPoints = cullingVolume.SubgridAroundWorldPoint(playerBetweenFloorHigh - 1, followTransform.position, 15f);
            previousFramePoints.AddRange(additionalPoints);
        }
        int numberOfPoints = previousFramePoints.Count;

        // allocate data shared across jobs
        commands = new NativeArray<RaycastCommand>(numberOfPoints, allocator);
        results = new NativeArray<RaycastHit>(numberOfPoints, allocator);

        Vector3 followPosition = followTransform.position;
        for (int i = 0; i < numberOfPoints; i++) {
            CullingGridPoint point = previousFramePoints[i];
            Vector3 displacement = point.rayCastDirection(followPosition);
            radarDirections[i] = displacement;
            distances[i] = displacement.magnitude;
        }

        // create setup job
        var setupJob = new ClearsightV4RaycastSetupJob();
        setupJob.EyePos = followTransform.position + Vector3.up; // TODO: this probably must change?
        setupJob.directions = radarDirections;
        setupJob.distances = distances;
        setupJob.layerMask = defaultLayerMask;
        setupJob.Commands = commands;

        // create gather job
        var gatherJob = new ClearsightRaycastGatherJob();
        gatherJob.Results = results;
        gatherJob.output = raycastResultsBackBuffer;

        // schedule setup job
        var setupJobHandle = setupJob.Schedule(numberOfPoints, jobBatchSize);

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
        gatherJobHandle = gatherJob.Schedule(numberOfPoints, jobBatchSize, rayCastJobHandle);

        // kick jobs
        JobHandle.ScheduleBatchedJobs();

        yield return null;
    }


    /**------------------------------------------------------------------------
     *                           asynchronous geometry logic
     *------------------------------------------------------------------------**/
    IEnumerator EnqueueInterloperCulling(int j, Vector3 playerPosition, int playerFloor) {
        Vector3 followPoint = followTransform.position + Vector3.up;
        int numberPoints = points.Count;
        HashSet<string> hits = new HashSet<string>();
        HashSet<string> roofZoneHits = new HashSet<string>();
        for (int i = 0; i < numberPoints; i++) {
            RaycastHit result = raycastResults[i];
            CullingGridPoint point = points[i];
            if (result.collider == null) {
                Debug.DrawLine(followPoint, point.position + (2 * Vector3.up), Color.green);
                // point.DrawRay(orientation);
                CharacterCamera.IsometricOrientation orientation = (CharacterCamera.IsometricOrientation)(((int)characterCamera.currentOrientation + 3) % 4);
                foreach (string idn in point.GetInterlopers(orientation)) {
                    hits.Add(idn);
                }
                foreach (string zoneIdn in point.GetRooftopZones(orientation)) {
                    if (zoneIdn != "-1") {
                        Debug.DrawLine(followPoint, point.position + (2 * Vector3.up), Color.green);
                        roofZoneHits.Add(zoneIdn);
                    }
                }
            } else {
                // Debug.DrawLine(followPoint, point.position + (2 * Vector3.up), Color.red);
            }
        }

        // enqueue culling commands
        foreach (string idn in hits) {
            CullingComponent cullingComponent = cullingComponents[idn];
            if (cullingComponent.floor != playerFloor && cullingComponent.floor != playerBetweenFloorHigh - 1) continue;
            commandQueue.Add(new CullingCommand(cullingComponent, CullingCommand.Command.interloper));
        }
        bool updateBecauseRoofZone = false;
        foreach (string idn in roofZoneHits) {
            updateBecauseRoofZone |= !activeRoofZoneInterlopers.ContainsKey(idn);
            activeRoofZoneInterlopers[idn] = 2;
        }
        if (updateBecauseRoofZone) {
            yield return HideAllAboveFloors();
        }

        yield return null;
    }

    IEnumerator ApplyCommands(Vector3 playerPosition) {
        foreach (CullingCommand command in commandQueue) {
            if (command.command == CullingCommand.Command.interloper) {
                command.component.ApplyCulling(playerPosition);
            } else if (command.command == CullingCommand.Command.above) {
                command.component.ChangeState(CullingComponent.CullingState.above);
            }
            activeInterlopers[command.component] = 2;
        }
        yield return null;
    }

    IEnumerator ApplyTimestep() {
        // update timer to roof zones
        List<string> zoneKeys = new List<string>(activeRoofZoneInterlopers.Keys);
        List<string> roofKeysToRemove = new List<string>();
        foreach (string key in zoneKeys) {
            activeRoofZoneInterlopers[key] -= 1;
            if (activeRoofZoneInterlopers[key] <= 0) {
                roofKeysToRemove.Add(key);
            }
        }
        foreach (string key in roofKeysToRemove) {
            activeRoofZoneInterlopers.Remove(key);
        }
        if (roofKeysToRemove.Count > 0) {
            yield return HideAllAboveFloors();
        }

        // update timer to interlopers
        List<CullingComponent> keys = new List<CullingComponent>(activeInterlopers.Keys);
        List<CullingComponent> keysToRemove = new List<CullingComponent>();
        foreach (CullingComponent key in keys) {
            activeInterlopers[key] -= 1;
            if (activeInterlopers[key] <= 0) {
                key.StopCulling();
                keysToRemove.Add(key);
            }
        }
        foreach (CullingComponent key in keysToRemove) {
            activeInterlopers.Remove(key);
        }
        yield return null;
    }

    IEnumerator HandleDynamicAbove() {
        int numberHits = Physics.OverlapSphereNonAlloc(playerPosition, 40f, colliderHits, LayerUtil.GetLayerMask(Layer.obj, Layer.bulletPassThrough, Layer.shell, Layer.bulletOnly, Layer.interactive), QueryTriggerInteraction.Ignore);
        yield return waitForFrame;
        for (int i = 0; i < numberHits; i++) {
            Collider collider = colliderHits[i];
            if (collider == null || collider.name == "cutaway" || collider.gameObject == null || collider.transform.IsChildOf(followTransform))
                continue;

            Transform root = collider.transform.root;
            if (root.position.y > playerPosition.y + 1.5f) {
                CullingComponent handler = GetDynamicHandler(collider, root);
                handler.ChangeState(CullingComponent.CullingState.above);
                activeDynamicInterlopers[handler] = 2;
            }
        }

        yield return waitForFrame;

        // apply timestep
        List<CullingComponent> zoneKeys = new List<CullingComponent>(activeDynamicInterlopers.Keys);
        List<CullingComponent> keysToRemove = new List<CullingComponent>();
        foreach (CullingComponent key in zoneKeys) {
            activeDynamicInterlopers[key] -= 1;
            if (activeDynamicInterlopers[key] <= 0) {
                key.StopCulling();
                keysToRemove.Add(key);
            }
        }
        foreach (CullingComponent key in keysToRemove) {
            activeDynamicInterlopers.Remove(key);
        }
    }

    CullingComponent GetDynamicHandler(Collider key, Transform root) {
        if (dynamicCullingComponents.ContainsKey(key)) {
            return dynamicCullingComponents[key];
        } else {
            CullingComponent component = Toolbox.GetOrCreateComponent<CullingComponent>(root.gameObject);
            dynamicCullingComponents[key] = component;
            component.Initialize();
            return component;
        }
    }


    /**------------------------------------------------------------------------
     *                           cleanup
     *------------------------------------------------------------------------**/
    void OnDestroy() {
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
    }


}


public record CullingCommand {
    public CullingComponent component;
    public enum Command { interloper, above }
    public Command command;
    public CullingCommand(CullingComponent component, Command command) {
        this.command = command;
        this.component = component;
    }
}