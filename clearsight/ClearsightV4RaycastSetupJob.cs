using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
struct ClearsightV4RaycastSetupJob : IJobParallelFor {
    public LayerMask layerMask;

    [ReadOnly]
    public NativeArray<Vector3> directions;
    [ReadOnly]
    public NativeArray<Vector3> origins;
    [ReadOnly]
    public NativeArray<float> distances;

    [WriteOnly]
    public NativeArray<RaycastCommand> Commands;

    public void Execute(int index) {
        Vector3 vec = directions[index];
        float distance = distances[index];
        Vector3 origin = origins[index];
        Commands[index] = new RaycastCommand(origin, vec.normalized, distance, layerMask: layerMask);
    }
}

