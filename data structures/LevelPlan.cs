using System.Collections.Generic;
using System.Linq;
using Items;
using Newtonsoft.Json;
using UnityEngine;
public record LevelPlan {
    public string insertionPointIdn;
    public string extractionPointIdn;
    // TODO: converter for list
    [JsonConverter(typeof(ObjectListJsonConverter<Tactic>))]
    public List<Tactic> activeTactics;
    public List<BaseItem> items;
    public static LevelPlan Default(List<BaseItem> allItems) {
        List<BaseItem> itemList = new List<BaseItem>() { null, null, null, null };
        if (allItems.Count >= 1) {
            itemList[0] = allItems[0];
        }
        if (allItems.Count >= 2) {
            itemList[1] = allItems[1];
        }
        if (allItems.Count >= 3) {
            itemList[2] = allItems[2];
        }
        return new LevelPlan {
            insertionPointIdn = "",
            extractionPointIdn = "",
            items = itemList,
            activeTactics = new List<Tactic>()
            // activeTactics = new List<Tactic>() { new TacticDisguise(), new TacticFakeID() }
        };
    }

    public bool startWithDisguise() => activeTactics.Any(tactic => tactic is TacticDisguise);
    public bool startWithFakeID() => activeTactics.Any(tactic => tactic is TacticFakeID);

    public void ApplyState(GameObject playerObject) {
        foreach (ItemHandler itemHandler in playerObject.GetComponentsInChildren<ItemHandler>()) {
            itemHandler.LoadItemState(items);
            if (startWithFakeID()) {
                itemHandler.items.Add(BaseItem.LoadItem("ID"));
            }
        }
    }

}