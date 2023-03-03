using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
public record LevelPlan {
    public string insertionPointIdn;
    public string extractionPointIdn;
    public List<Tactic> activeTactics;
    // public string[] items = new string[4];
    public List<BaseItem> items;
    public static LevelPlan Default() => new LevelPlan {
        insertionPointIdn = "",
        extractionPointIdn = "",
        // items = new string[4] { "deck", "", "", "" },
        // items = new string[4] { "rocket", "deck", "C4", "tools" },
        // items = new string[4] { "deck", "C4", "tools", "" },
        // items = new string[4] { "grenade", "deck", "rocket", "tools" },
        items = new List<BaseItem> {
            ItemInstance.LoadItem("deck"),
            ItemInstance.LoadItem("tools"),
            ItemInstance.LoadItem("C4"),
        },
        activeTactics = new List<Tactic>()
        // activeTactics = new List<Tactic>() { new TacticDisguise(), new TacticFakeID() }
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