public record VRMissionDelta {
    public enum Status { inProgress, victory, fail }
    public Status status;
    public int numberTotalNPCs;
    public int numberLiveNPCs;
    public int numberNPCsKilled;
    public float NPCspawnInterval = 5f;
    public float secondsPlayed = 0f;

    public static VRMissionDelta Empty() {
        return new VRMissionDelta {
            numberTotalNPCs = 0,
            numberLiveNPCs = 0,
            numberNPCsKilled = 0,
            NPCspawnInterval = 5,
            secondsPlayed = 0f,
            status = Status.inProgress
        };
    }
}