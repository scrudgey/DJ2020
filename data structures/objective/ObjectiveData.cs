using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveData")]
public class ObjectiveData : Objective {
    public List<PayData> targetPaydata;
    protected override ObjectiveStatus EvaluateStatus(GameData data) {
        // if (targetPaydata.All(payData => data.playerState.payDatas.Select(dat => dat.filename).Contains(payData.filename))) {
        if (targetPaydata.All(payData => data.levelState.delta.levelAcquiredPaydata.Select(dat => dat.filename).Contains(payData.filename))) {
            return ObjectiveStatus.complete;
        } else {
            return ObjectiveStatus.inProgress;
        }
    }
    public override float Progress(GameData data) => 0f;
}