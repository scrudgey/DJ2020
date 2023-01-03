public record LevelPlan {
    public bool startWithDisguise;
    public string insertionPointIdn;
    public string extractionPointIdn;

    public static LevelPlan Default() => new LevelPlan {
        insertionPointIdn = "",
        extractionPointIdn = "",
        startWithDisguise = false
    };
}