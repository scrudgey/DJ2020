using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public partial class AsyncRaycastService : Singleton<AsyncRaycastService> {
    readonly static int BUFFER_SIZE = 500;
    readonly static int BATCH_SIZE = 50;
    int index;
    int previousIndex;
    NativeArray<Vector3> origins;
    NativeArray<Vector3> directions;
    NativeArray<float> distances;
    NativeArray<LayerMask> layerMasks;

    NativeArray<RaycastCommand> commands;
    NativeArray<RaycastHit> raycastResults;
    NativeArray<RaycastHit> results;

    JobHandle gatherJobHandle;

    Action<RaycastHit>[] callbacks;
    Action<RaycastHit>[] previousCallbacks;

    void Start() {
        previousIndex = 0;
        Physics.queriesHitTriggers = false;

        var allocator = Allocator.Persistent;

        commands = new NativeArray<RaycastCommand>(BUFFER_SIZE, allocator);

        origins = new NativeArray<Vector3>(BUFFER_SIZE, allocator);
        directions = new NativeArray<Vector3>(BUFFER_SIZE, allocator);
        distances = new NativeArray<float>(BUFFER_SIZE, allocator);
        layerMasks = new NativeArray<LayerMask>(BUFFER_SIZE, allocator);
        // raycastResults = new NativeArray<RaycastHit>(BUFFER_SIZE, allocator);
        results = new NativeArray<RaycastHit>(BUFFER_SIZE, allocator);
        callbacks = new Action<RaycastHit>[BUFFER_SIZE];
        previousCallbacks = new Action<RaycastHit>[BUFFER_SIZE];
    }

    public void RequestRaycast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, Action<RaycastHit> callback) {
        // Debug.Log("requesting raycast");
        // if (index % 10 == 0) {
        //     Debug.Log($"requesting raycast: {index}");
        // }
        origins[index] = origin;
        directions[index] = direction;
        distances[index] = distance;
        layerMasks[index] = layerMask;
        callbacks[index] = callback;
        index++;
    }

    void Update() {
        gatherJobHandle.Complete();
        // i need to double buffer the callbacks
        // Debug.Log($"******* serving results for: {previousIndex}, starting next batch of {index}");
        for (int i = 0; i < previousIndex; i++) {
            try {
                RaycastHit hit = results[i];

                previousCallbacks[i].Invoke(hit);
                if (hit.collider == null) {
                    Debug.DrawLine(origins[i], origins[i] + (directions[i].normalized * distances[i]), Color.yellow);
                } else {
                    Debug.DrawLine(origins[i], hit.point, Color.red);
                }
            }
            catch (Exception e) {
                Debug.Log($"callback failed: {i} {results[i]} {previousCallbacks[i]}");
            }
        }

        // distribute raycast results

        if (commands.IsCreated)
            commands.Dispose();
        if (raycastResults.IsCreated)
            raycastResults.Dispose();


        // allocate data shared across jobs
        var allocator = Allocator.TempJob;
        commands = new NativeArray<RaycastCommand>(index, allocator);
        raycastResults = new NativeArray<RaycastHit>(index, allocator);

        // create setup job
        var setupJob = new RaycastSetupJob();
        setupJob.origins = origins;
        setupJob.directions = directions;
        setupJob.distances = distances;
        setupJob.layerMasks = layerMasks;
        setupJob.Commands = commands;

        // create gather job
        var gatherJob = new RaycastGatherJob();
        gatherJob.Results = raycastResults;
        gatherJob.output = results;

        // schedule setup job
        var setupJobHandle = setupJob.Schedule(index, BATCH_SIZE);

        // schedule raycast job
        // specify dependency on setup job
        var rayCastJobHandle = RaycastCommand.ScheduleBatch(
            commands,
            raycastResults,
            BATCH_SIZE,
            setupJobHandle
          );

        // schedule gather job
        // specify dependency on raycast job
        gatherJobHandle = gatherJob.Schedule(index, BATCH_SIZE, rayCastJobHandle);

        // kick jobs
        JobHandle.ScheduleBatchedJobs();
        // Debug.Log($"async kicked off {index} raycasts");
        previousIndex = index;
        Array.Copy(callbacks, previousCallbacks, BUFFER_SIZE);
        // previousCallbacks = new Action<RaycastHit>[BUFFER_SIZE](callbacks);
        // callbacks = new Action<RaycastHit>[BUFFER_SIZE];

        index = 0;
    }


    public override void OnDestroy() {
        base.OnDestroy();
        if (layerMasks.IsCreated)
            layerMasks.Dispose();
        if (origins.IsCreated)
            origins.Dispose();
        if (directions.IsCreated)
            directions.Dispose();
        if (distances.IsCreated)
            distances.Dispose();

        if (raycastResults.IsCreated)
            raycastResults.Dispose();
        if (commands.IsCreated)
            commands.Dispose();
        if (raycastResults.IsCreated)
            raycastResults.Dispose();
        if (results.IsCreated)
            results.Dispose();
    }
}



struct RaycastSetupJob : IJobParallelFor {
    // public LayerMask layerMask;

    [ReadOnly]
    public NativeArray<LayerMask> layerMasks;
    [ReadOnly]
    public NativeArray<Vector3> origins;
    [ReadOnly]
    public NativeArray<Vector3> directions;
    [ReadOnly]
    public NativeArray<float> distances;

    [WriteOnly]
    public NativeArray<RaycastCommand> Commands;

    public void Execute(int index) {
        Vector3 vec = directions[index];
        Vector3 origin = origins[index];
        float distance = distances[index];
        LayerMask layerMask = layerMasks[index];

        Commands[index] = new RaycastCommand(origin, vec.normalized, distance, layerMask: layerMask.value);
    }
}

struct RaycastGatherJob : IJobParallelFor {
    [ReadOnly]
    public NativeArray<RaycastHit> Results;

    [WriteOnly]
    public NativeArray<RaycastHit> output;

    public void Execute(int index) {
        output[index] = Results[index];
    }
}