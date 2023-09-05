using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerLandmark : MonoBehaviour {
    public enum LandmarkType { pointOfInterest, station }
    public LandmarkType landmarkType;
    public bool stationIsClaimed;
    public static Dictionary<WorkerNPCAI, WorkerLandmark> visitors;
    public bool excludable;
    public bool excluded;
    public bool isExcluded() => excludable && excluded;
}
