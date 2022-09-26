using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public record VRMissionTemplate {
    public string sceneName;
    public VRMissionType missionType;
    public SensitivityLevel sensitivityLevel;
    public int maxNumberNPCs;
    public int numberConcurrentNPCs;
    public PlayerTemplate playerState;

    [JsonConverter(typeof(NPCTemplateJsonConverter))]
    public NPCTemplate npc1State;

    [JsonConverter(typeof(NPCTemplateJsonConverter))]
    public NPCTemplate npc2State;

    public static VRMissionTemplate Default() => new VRMissionTemplate {
        sceneName = "VR_infiltration",
        missionType = VRMissionType.hunt,
        sensitivityLevel = SensitivityLevel.restrictedProperty,
        maxNumberNPCs = 10,
        numberConcurrentNPCs = 3,
        playerState = PlayerTemplate.Default(),
        npc1State = ScriptableObject.Instantiate(Resources.Load("data/npc/guard1") as NPCTemplate),
        npc2State = ScriptableObject.Instantiate(Resources.Load("data/npc/guard2") as NPCTemplate),
    };
}