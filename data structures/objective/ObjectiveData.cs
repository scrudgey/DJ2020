using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveData")]
public class ObjectiveData : Objective {
    public PayData targetPaydata;
    protected override ObjectiveStatus EvaluateStatus(GameData data) {
        if (data.levelState.delta.levelAcquiredPaydata.Contains(targetPaydata)) {
            return ObjectiveStatus.complete;
        } else {
            return ObjectiveStatus.inProgress;
        }
    }
    public override float Progress(GameData data) => 0f;
}