using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

[System.Serializable]
public record LevelDelta {
    public enum MissionPhase { action, extractionSuccess, extractionFail, playerDead }
    public MissionPhase phase;
    [JsonConverter(typeof(PowerGraphConverter))]
    public PowerGraph powerGraph;
    [JsonConverter(typeof(CyberGraphConverter))]
    public CyberGraph cyberGraph;
    [JsonConverter(typeof(AlarmGraphConverter))]
    public AlarmGraph alarmGraph;
    public int strikeTeamCount = 0;
    public int npcCount = 0;
    public float npcSpawnTimer = 0f;
    public float strikeTeamSpawnTimer = 0f;
    public float strikeTeamResponseTimer = 0f;
    public ObjectiveStatus missionStatus;
    public bool disguise;
    public List<PayData> levelAcquiredPaydata;
    [JsonConverter(typeof(ObjectListJsonConverter<LootData>))]
    public List<LootData> levelAcquiredLoot;
    public int levelAcquiredCredits;
    public GameManager.HQPhase hqPhase;
    public bool alarmTerminalActive;
    public float strikeTeamMissionTimer;
    public LevelTemplate.StrikeTeamResponseBehavior strikeTeamBehavior;
    public List<ObjectiveDelta> objectiveDeltas;
    public List<ObjectiveDelta> optionalObjectiveDeltas;
    // dialogue
    public List<DialogueCard> dialogueCards;
    public int bullshitLevel;
    public Stack<DialogueTacticType> lastTactics;
    public int stallsAvailable = 1;
    public int easesAvailable = 1;
    public HashSet<KeyData> keys;
    public int humansKilled;
    public SerializableDictionary<string, string> objectiveLocations;
    public List<SoftwareState> softwareStates;
    public SerializableDictionary<(KeyType, KeyClass), int> keyClassMap;
    public static LevelDelta Empty() => new LevelDelta {
        levelAcquiredPaydata = new List<PayData>(),
        levelAcquiredLoot = new List<LootData>(),
        dialogueCards = new List<DialogueCard>(),
        lastTactics = new Stack<DialogueTacticType>(),
        objectiveDeltas = new List<ObjectiveDelta>(),
        optionalObjectiveDeltas = new List<ObjectiveDelta>(),
        keys = new HashSet<KeyData>(),
        objectiveLocations = new SerializableDictionary<string, string>(),
        softwareStates = new List<SoftwareState>()
    };
    public List<ObjectiveDelta> AllObjectives() => objectiveDeltas.Concat(optionalObjectiveDeltas).ToList();

}