public record VRMissionMutableData {

    // mutable state
    public int numberTotalNPCs;
    public int numberLiveNPCs;
    public int numberNPCsKilled;
    public float NPCspawnInterval = 5f;
    public float secondsPlayed = 0f;

    public static VRMissionMutableData Empty() {
        return new VRMissionMutableData {
            numberTotalNPCs = 0,
            numberLiveNPCs = 0,
            numberNPCsKilled = 0,
            NPCspawnInterval = 5,
            secondsPlayed = 0f
        };
    }
}