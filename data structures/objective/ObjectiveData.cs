using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveData")]
public class ObjectiveData : Objective {
    public List<string> targetFileNames;
    protected override ObjectiveStatus EvaluateStatus(GameData data) {
        if (targetFileNames.All(filename => data.playerState.payDatas.Select(dat => dat.filename).Contains(filename))) {
            return ObjectiveStatus.complete;
        } else {
            return ObjectiveStatus.inProgress;
        }
    }
    public override float Progress(GameData data) => 0f;
}