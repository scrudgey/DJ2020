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
    public Vector3 EyePos;
    public LayerMask layerMask;

    [ReadOnly]
    public NativeArray<Vector3> directions;
    [ReadOnly]
    public NativeArray<float> distances;

    [WriteOnly]
    public NativeArray<RaycastCommand> Commands;

    public void Execute(int index) {
        Vector3 vec = directions[index];
        float distance = distances[index];
        Commands[index] = new RaycastCommand(EyePos, vec.normalized, distance, layerMask: layerMask);
    }
}

