using System.Collections.Generic;
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
    public int strikeTeamMaxSize;
    public int strikeTeamCount = 0;
    public float strikeTeamSpawnTimer = 0f;
    public float strikeTeamResponseTimer = 0f;
    public ObjectiveStatus objectiveStatus;
    public bool disguise;
    public int npcsSpawned;
    [JsonIgnore]
    public Dictionary<Objective, ObjectiveStatus> objectivesState;
    public HashSet<Objective> failedObjectives;
    public List<PayData> levelAcquiredPaydata;
    public static LevelDelta Empty() => new LevelDelta {
        objectivesState = new Dictionary<Objective, ObjectiveStatus>(),
        failedObjectives = new HashSet<Objective>(),
        levelAcquiredPaydata = new List<PayData>()
    };
}