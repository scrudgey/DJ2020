using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveGetLoot")]
public class ObjectiveGetLoot : Objective {
    public string targeLootName;
    bool objectHasBeenUsed;
    protected override ObjectiveStatus EvaluateStatus(GameData data) {
        if (data.levelState.delta.levelAcquiredLoot.Any(loot => loot.lootName.ToLower().Equals(targeLootName.ToLower()))) {
            return ObjectiveStatus.complete;
        } else {
            return ObjectiveStatus.inProgress;
        }
    }
    public override float Progress(GameData data) => 0f;
}