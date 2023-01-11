using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
public record LevelPlan {
    public string insertionPointIdn;
    public string extractionPointIdn;
    public List<Tactic> activeTactics;
    public string[] items = new string[4];
    public static LevelPlan Default() => new LevelPlan {
        insertionPointIdn = "",
        extractionPointIdn = "",
        // items = new string[4] { "deck", "", "", "" },
        items = new string[4] { "rocket", "deck", "C4", "tools" },
        // activeTactics = new List<Tactic>()
        activeTactics = new List<Tactic>() { new TacticDisguise() }
    };

    public bool startWithDisguise() => activeTactics.Any(tactic => tactic is TacticDisguise);
    public bool startWithFakeID() => activeTactics.Any(tactic => tactic is TacticFakeID);

    public void ApplyState(GameObject playerObject) {
        foreach (ItemHandler itemHandler in playerObject.GetComponentsInChildren<ItemHandler>()) {
            itemHandler.LoadItemState(items);
            if (startWithFakeID()) {
                itemHandler.items.Add(ItemInstance.LoadItem("ID"));
            }
        }
    }

}