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
    Action<RaycastHit>[] callbacks;

    NativeArray<Vector3> backBufferOrigins;
    NativeArray<Vector3> backBufferDirections;
    NativeArray<float> backBufferDistances;
    NativeArray<LayerMask> backBufferLayerMasks;

    NativeArray<RaycastCommand> commands;
    NativeArray<RaycastHit> raycastResults;
    NativeArray<RaycastHit> results;

    JobHandle gatherJobHandle;

    Action<RaycastHit>[] previousCallbacks;

    void Start() {
        previousIndex = 0;

        var allocator = Allocator.Persistent;

        // commands = new NativeArray<RaycastCommand>(BUFFER_SIZE, allocator);

        origins = new NativeArray<Vector3>(BUFFER_SIZE, allocator);
        directions = new NativeArray<Vector3>(BUFFER_SIZE, allocator);
        distances = new NativeArray<float>(BUFFER_SIZE, allocator);
        layerMasks = new NativeArray<LayerMask>(BUFFER_SIZE, allocator);

        backBufferOrigins = new NativeArray<Vector3>(BUFFER_SIZE, allocator);
        backBufferDirections = new NativeArray<Vector3>(BUFFER_SIZE, allocator);
        backBufferDistances = new NativeArray<float>(BUFFER_SIZE, allocator);
        backBufferLayerMasks = new NativeArray<LayerMask>(BUFFER_SIZE, allocator);

        // raycastResults = new NativeArray<RaycastHit>(BUFFER_SIZE, allocator);
        results = new NativeArray<RaycastHit>(BUFFER_SIZE, allocator);
        callbacks = new Action<RaycastHit>[BUFFER_SIZE];
        previousCallbacks = new Action<RaycastHit>[BUFFER_SIZE];
    }

    public void RequestRaycast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, Action<RaycastHit> callback) {
        backBufferOrigins[index] = origin;
        backBufferDirections[index] = direction;
        backBufferDistances[index] = distance;
        backBufferLayerMasks[index] = layerMask;
        callbacks[index] = callback;
        index++;
    }

    void Update() {
        gatherJobHandle.Complete();

        // distribute raycast results
        for (int i = 0; i < previousIndex; i++) {
            try {
                RaycastHit hit = results[i];
                previousCallbacks[i].Invoke(hit);
                previousCallbacks[i] = null;
            }
            catch (Exception e) {
                Debug.LogError($"callback failed: {i} {results[i]} {previousCallbacks[i]}");
                Debug.LogError($"previous index: {previousIndex}, index: {index}");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
        }

        SwapBackBuffer();

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

        index = 0;
    }

    void SwapBackBuffer() {
        previousIndex = index;
        Array.Copy(callbacks, previousCallbacks, BUFFER_SIZE);

        var tmp = origins;
        origins = backBufferOrigins;
        backBufferOrigins = tmp;

        tmp = directions;
        directions = backBufferDirections;
        backBufferDirections = tmp;

        var tmp2 = distances;
        distances = backBufferDistances;
        backBufferDistances = tmp2;

        var tmp3 = layerMasks;
        layerMasks = backBufferLayerMasks;
        backBufferLayerMasks = tmp3;

        // tmp.r
        // tmp2 = null;
        // tmp3 = null;
    }


    public override void OnDestroy() {
        base.OnDestroy();
        DisposeOfNativeArrays();
    }
    void OnApplicationQuit() {
        DisposeOfNativeArrays();
    }

    void DisposeOfNativeArrays() {
        gatherJobHandle.Complete();

        if (layerMasks.IsCreated) {
            layerMasks.Dispose();
            layerMasks = default;
        }
        if (origins.IsCreated) {
            origins.Dispose();
            origins = default;
        }
        if (directions.IsCreated) {
            directions.Dispose();
            directions = default;
        }
        if (distances.IsCreated) {
            distances.Dispose();
            distances = default;
        }

        if (backBufferLayerMasks.IsCreated) {
            backBufferLayerMasks.Dispose();
            backBufferLayerMasks = default;
        }
        if (backBufferOrigins.IsCreated) {
            backBufferOrigins.Dispose();
            backBufferOrigins = default;
        }

        if (backBufferDirections.IsCreated) {
            backBufferDirections.Dispose();
            backBufferDirections = default;
        }
        if (backBufferDistances.IsCreated) {
            backBufferDistances.Dispose();
            backBufferDistances = default;
        }

        if (raycastResults.IsCreated) {
            raycastResults.Dispose();
            raycastResults = default;
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



struct RaycastSetupJob : IJobParallelFor {
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