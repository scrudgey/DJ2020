using System.Collections.Generic;
using System.Linq;

public record LevelPlan {
    public string insertionPointIdn;
    public string extractionPointIdn;
    public List<Tactic> activeTactics;
    public static LevelPlan Default() => new LevelPlan {
        insertionPointIdn = "",
        extractionPointIdn = "",
        activeTactics = new List<Tactic>()
    };

    public bool startWithDisguise() => activeTactics.Any(tactic => tactic is TacticDisguise);
    public bool startWithFakeID() => activeTactics.Any(tactic => tactic is TacticFakeID);
}