using System.Collections.Generic;
public record LevelPlan {
    public bool startWithDisguise;
    public string insertionPointIdn;
    public string extractionPointIdn;
    public List<Tactic> activeTactics;
    public static LevelPlan Default() => new LevelPlan {
        insertionPointIdn = "",
        extractionPointIdn = "",
        startWithDisguise = false,
        activeTactics = new List<Tactic>()
    };
}