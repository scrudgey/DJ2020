using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public class WorkerSpawnPoint : MonoBehaviour {
    public WorkerNPCAI.WorkerType workerType;
    public NPCTemplate myTemplate;
    public Transform guardPoint;
    public Transform lookAtPoint;
    PrefabPool NPCPool;
    public List<WorkerLandmark> landmarks;
    // Stack<WorkerLandmark> stations;
    // List<WorkerLandmark> pointsOfInterest;
    void Start() {
        InitializePools();
        // stations = new Stack<WorkerLandmark>(landmarks.Where(landmark => {
        //     return landmark.landmarkType == WorkerLandmark.LandmarkType.station && !landmark.stationIsClaimed;
        // }
        //     ));
        // pointsOfInterest = new List<WorkerLandmark>(landmarks.Where(landmark => landmark.landmarkType == WorkerLandmark.LandmarkType.pointOfInterest));
    }
    void InitializePools() {
        NPCPool = PoolManager.I?.RegisterPool("prefabs/WorkerNPC", poolSize: 10);
    }
    public GameObject SpawnNPC(NPCTemplate template, bool useSpawnEffect = true) {
        if (NPCPool == null) {
            InitializePools();
        }
        GameObject npc = NPCPool.GetObject(transform.position);

        PatrolRoute route = Toolbox.RandomFromList(GameObject.FindObjectsOfType<PatrolRoute>());
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();

        WorkerNPCAI ai = npc.GetComponentInChildren<WorkerNPCAI>();
        LegsAnimation legsAnimation = npc.GetComponentInChildren<LegsAnimation>();
        legsAnimation.characterCamera = cam;
        controller.OrbitCamera = cam;

        ai.lookAtPoint = lookAtPoint;
        ai.guardPoint = guardPoint;
        ApplyLandmarksToNPC(ai);

        motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyNPCState(template, npc);

        ai.Initialize(workerType);
        return npc;
    }

    void ApplyLandmarksToNPC(WorkerNPCAI ai) {
        ai.landmarkPointsOfInterest = landmarks;
        var stations = landmarks.FirstOrDefault(landmark => landmark.landmarkType == WorkerLandmark.LandmarkType.station && !landmark.stationIsClaimed);
        if (stations != null) {
            ai.landmarkStation = stations;
            stations.stationIsClaimed = true;
        }
    }

    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.activeGun = 0;
        state.ApplyState(npcObject);
    }

    public GameObject SpawnTemplated() {
        return SpawnNPC(myTemplate, useSpawnEffect: false);
    }
}
