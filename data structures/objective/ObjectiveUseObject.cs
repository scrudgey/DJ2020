using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveUseObject")]
public class ObjectiveUseObject : Objective {
    public string targetObject;
    bool objectHasBeenUsed;
    protected override ObjectiveStatus EvaluateStatus(GameData data) {
        if (data.levelState.delta.levelInteractedObjects.Contains(targetObject)) {
            return ObjectiveStatus.complete;
        } else {
            return ObjectiveStatus.inProgress;
        }
    }
    public override float Progress(GameData data) => 0f;
}