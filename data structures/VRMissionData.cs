using UnityEngine;

public record VRMissionData {

    public string sceneName;
    public VRMissionType missionType;
    public SensitivityLevel sensitivityLevel;
    public int maxNumberNPCs;
    public int numberConcurrentNPCs;
    public PlayerState playerState;
    public NPCState npc1State;
    public NPCState npc2State;

    public VRMissionMutableData data;
    public static VRMissionData DefaultData() => new VRMissionData {
        sceneName = "VR_infiltration",
        missionType = VRMissionType.hunt,
        sensitivityLevel = SensitivityLevel.restrictedProperty,
        maxNumberNPCs = 10,
        numberConcurrentNPCs = 3,
        playerState = PlayerState.DefaultGameData(),
        npc1State = ScriptableObject.Instantiate(Resources.Load("data/npc/guard1") as NPCState),
        npc2State = ScriptableObject.Instantiate(Resources.Load("data/npc/guard2") as NPCState),
        data = VRMissionMutableData.Empty()
    };

}