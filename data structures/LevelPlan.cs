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

    public List<SoftwareTemplate> softwareTemplates;

    public SerializableDictionary<string, NodeVisibility> nodeVisibility;

    public SerializableDictionary<string, PayData> nodePayData;

    public SerializableDictionary<string, string> objectiveLocations;

    public static LevelPlan Default(PlayerState playerState) {
        List<ItemTemplate> itemList = new List<ItemTemplate>() { null, null, null, null, null };
        SoftwareTemplate[] softwares = new SoftwareTemplate[playerState.cyberdeck.softwareSlots];

        // initialize item loadout
        if (playerState.allItems.Count >= 1) {
            itemList[0] = playerState.allItems[0];
        }
        if (playerState.allItems.Count >= 2) {
            itemList[1] = playerState.allItems[1];
        }
        if (playerState.allItems.Count >= 3) {
            itemList[2] = playerState.allItems[2];
        }
        if (playerState.allItems.Count >= 4) {
            itemList[3] = playerState.allItems[3];
        }

        // initialize software loadout
        for (int i = 0; i < softwares.Length; i++) {
            SoftwareTemplate template = i < playerState.softwareTemplates.Count ? playerState.softwareTemplates[i] : null;
            softwares[i] = template;
        }

        return new LevelPlan {
            insertionPointIdn = "",
            extractionPointIdn = "",
            items = itemList,
            nodeVisibility = new SerializableDictionary<string, NodeVisibility>(),
            objectiveLocations = new SerializableDictionary<string, string>(),
            activeTactics = new List<Tactic>(),
            softwareTemplates = softwares.ToList()
            // activeTactics = new List<Tactic>() { new TacticDisguise(), new TacticFakeID() }
            // activeTactics = new List<Tactic>() { Resources.Load("data/tactics/fakeID") as Tactic }
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

    public void SetObjectiveLocation(Objective objective) {
        if (objectiveLocations.ContainsKey(objective.name)) {
            Debug.Log($"set objective location: skipping already set {objective.name}");
            return;
        } else {
            string target = Toolbox.RandomFromListExcept(objective.potentialSpawnPoints, objectiveLocations.Values.ToArray());
            Debug.Log($"tactic intel is setting objective {objective.name} spawn point: {target}");
            objectiveLocations[objective.name] = target;
        }
    }

}