using UnityEngine;
public class VRMissionData {
    public string sceneName;
    public VRMissionType missionType;
    public SensitivityLevel sensitivityLevel;
    public int numberNPCs;
    public PlayerState playerState;
    public NPCState npc1State;
    public NPCState npc2State;

    public static VRMissionData DefaultData() => new VRMissionData {
        sceneName = "VR_infiltration",
        missionType = VRMissionType.hunt,
        sensitivityLevel = SensitivityLevel.restrictedProperty,
        numberNPCs = 10,
        playerState = PlayerState.DefaultGameData(),
        npc1State = Resources.Load("data/npc/guard1") as NPCState, // TODO: load resources
        npc2State = Resources.Load("data/npc/guard2") as NPCState
    };

}