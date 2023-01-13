using System.Collections.Generic;
[System.Serializable]
public record LevelDelta {
    public enum MissionPhase { action, extractionSuccess, extractionFail, playerDead }
    public MissionPhase phase;
    public PowerGraph powerGraph;
    public CyberGraph cyberGraph;
    public AlarmGraph alarmGraph;
    public int strikeTeamMaxSize;
    public ObjectiveStatus objectiveStatus;
    public bool disguise;

    public Dictionary<Objective, ObjectiveStatus> objectivesState;
    public HashSet<Objective> failedObjectives;
    public static LevelDelta Empty() => new LevelDelta {
        objectivesState = new Dictionary<Objective, ObjectiveStatus>(),
        failedObjectives = new HashSet<Objective>()
    };
}