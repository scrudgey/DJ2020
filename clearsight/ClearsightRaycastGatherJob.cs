using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
struct ClearsightRaycastGatherJob : IJobParallelFor {
    [ReadOnly]
    public NativeArray<RaycastHit> Results;

    [WriteOnly]
    public NativeArray<RaycastHit> output;

    public void Execute(int index) {
        output[index] = Results[index];
    }
}

