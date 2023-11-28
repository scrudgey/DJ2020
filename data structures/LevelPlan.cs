using System.Collections.Generic;
using System.Linq;
using Items;
using Newtonsoft.Json;
using UnityEngine;
public record LevelPlan {
    public string insertionPointIdn;
    public string extractionPointIdn;
    [JsonConverter(typeof(ObjectListJsonConverter<Tactic>))]
    public List<Tactic> activeTactics;
    [JsonConverter(typeof(ObjectListJsonConverter<ItemTemplate>))]
    public List<ItemTemplate> items;
    public static LevelPlan Default(List<ItemTemplate> allItems) { // TODO: replace argument with playerstate?
        List<ItemTemplate> itemList = new List<ItemTemplate>() { null, null, null, null, null };
        if (allItems.Count >= 1) {
            itemList[0] = allItems[0];
        }
        if (allItems.Count >= 2) {
            itemList[1] = allItems[1];
        }
        if (allItems.Count >= 3) {
            itemList[2] = allItems[2];
        }
        if (allItems.Count >= 4) {
            itemList[3] = allItems[3];
        }
        return new LevelPlan {
            insertionPointIdn = "",
            extractionPointIdn = "",
            items = itemList,
            activeTactics = new List<Tactic>()
            // activeTactics = new List<Tactic>() { new TacticDisguise(), new TacticFakeID() }
            // activeTactics = new List<Tactic>() { new TacticFakeID() }
        };
    }

    public bool startWithDisguise() => activeTactics.Any(tactic => tactic is TacticDisguise);
    // public bool startWithFakeID() => activeTactics.Any(tactic => tactic is TacticFakeID);

    public void ApplyState(GameObject playerObject) {
        foreach (ItemHandler itemHandler in playerObject.GetComponentsInChildren<ItemHandler>()) {
            itemHandler.LoadItemState(items);
            // if (startWithFakeID()) {
            //     itemHandler.items.Add(ItemInstance.LoadItem("ID"));
            // }
        }
    }

}