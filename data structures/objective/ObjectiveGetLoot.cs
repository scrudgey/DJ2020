using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveGetLoot")]
public class ObjectiveGetLoot : Objective {
    public string targeLootName;
    public GameObject targetLootPrefab;
    protected override ObjectiveStatus EvaluateStatus(GameData data) {
        if (data.levelState.delta.levelAcquiredLoot.Any(loot => loot.lootName.ToLower().Equals(targeLootName.ToLower()))) {
            return ObjectiveStatus.complete;
        } else {
            return ObjectiveStatus.inProgress;
        }
    }
    public override float Progress(GameData data) => 0f;

    public override ObjectiveDelta ToDelta(LevelState state) {
        string targetIdn = Toolbox.RandomFromList(potentialSpawnPoints);

        ObjectiveLootSpawnpoint spawnpoint = state.spawnPoints[targetIdn];
        GameObject lootObj = GameObject.Instantiate(targetLootPrefab, spawnpoint.transform.position, Quaternion.identity);
        LootObject loot = lootObj.GetComponent<LootObject>();

        ObjectiveDelta delta = new ObjectiveDelta(this, () => lootObj.transform.position);
        loot.onCollect += () => delta.status = ObjectiveStatus.complete;

        Debug.Log($"spawning objective {lootObj} at {spawnpoint} {spawnpoint.idn}");
        return delta;
    }

}