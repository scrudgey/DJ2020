public record BurglarAttackResult {
    public bool success;
    public string feedbackText;
    public bool finish;
    public bool createTamperEvidence;
    public NoiseData noiseData;
    public AttackSurfaceElement element;
    public CyberDataStore attachedDataStore;
    public static BurglarAttackResult None => new BurglarAttackResult {
        success = false,
        feedbackText = ""
    };
}