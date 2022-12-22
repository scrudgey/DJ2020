public record BurglarAttackResult {
    public bool success;
    public string feedbackText;
    public static BurglarAttackResult None => new BurglarAttackResult {
        success = false,
        feedbackText = ""
    };
}