using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveUseObject")]
public class ObjectiveUseObject : Objective {
    public string targetObject;
    bool objectHasBeenUsed;
    // protected override ObjectiveStatus EvaluateStatus(GameData data) {
    //     // if (data.levelState.delta.levelInteractedObjects.Contains(targetObject)) {
    //     //     return ObjectiveStatus.complete;
    //     // } else {
    //     return ObjectiveStatus.inProgress;
    //     // }
    // }
    // public override float Progress(GameData data) => 0f;

    public override ObjectiveDelta ToDelta(LevelState state) {
        string targetIdn = Toolbox.RandomFromList(potentialSpawnPoints);
        ObjectiveLootSpawnpoint spawnpoint = state.spawnPoints[targetIdn];

        ObjectiveDelta delta = new ObjectiveDelta(this, () => spawnpoint.transform.position);
        Interactive interactive = spawnpoint.GetComponentInChildren<Interactive>();
        interactive.OnUsed += () => delta.status = ObjectiveStatus.complete;

        return delta;
    }
}