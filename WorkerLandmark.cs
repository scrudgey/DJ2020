using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerLandmark : MonoBehaviour {
    public enum LandmarkType { pointOfInterest, station }
    public LandmarkType landmarkType;
    public bool stationIsClaimed;
    // public static Dictionary<WorkerLandmark, HashSet<WorkerNPCAI>> visitors = new Dictionary<WorkerLandmark, HashSet<WorkerNPCAI>>();
    public static Dictionary<WorkerNPCAI, WorkerLandmark> visitors;
}
