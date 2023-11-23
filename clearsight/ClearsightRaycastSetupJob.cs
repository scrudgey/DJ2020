using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
struct ClearsightRaycastSetupJob : IJobParallelFor {
    public Vector3 EyePos;
    public LayerMask layerMask;
    public int indexOffset;

    [ReadOnly]
    public NativeArray<Vector3> directions;

    [WriteOnly]
    public NativeArray<RaycastCommand> Commands;

    public void Execute(int index) {
        Vector3 vec = directions[index + indexOffset];
        Commands[index] = new RaycastCommand(EyePos, vec.normalized, 50f, layerMask: layerMask);
    }
}

