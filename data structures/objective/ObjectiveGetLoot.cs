using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveGetLoot")]
public class ObjectiveGetLoot : Objective {
    public GameObject targetLootPrefab;
    public override ObjectiveDelta ToDelta(LevelState state) {
        string targetIdn = state.SetLocationOfObjective(this);

        ObjectiveLootSpawnpoint spawnpoint = state.spawnPoints[targetIdn];
        GameObject lootObj = GameObject.Instantiate(targetLootPrefab, spawnpoint.transform.position, Quaternion.identity);
        LootObject loot = lootObj.GetComponent<LootObject>();

        ObjectiveDelta delta = new ObjectiveDelta(this, () => lootObj.transform.position, targetIdn);
        if (state.plan.objectiveLocations.ContainsKey(name)) {
            delta.visibility = Visibility.known;
        }
        loot.onCollect += () => delta.status = ObjectiveStatus.complete;

        Debug.Log($"spawning objective {lootObj} at {spawnpoint} {spawnpoint.idn}");
        return delta;
    }

}