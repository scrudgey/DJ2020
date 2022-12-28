
[System.Serializable]
public record LevelDelta {
    public PowerGraph powerGraph;
    public CyberGraph cyberGraph;
    public AlarmGraph alarmGraph;
    public int strikeTeamMaxSize;
    public ObjectiveStatus objectiveStatus;

    public static LevelDelta Empty() => new LevelDelta {

    };
}