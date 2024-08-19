using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveData")]
public class ObjectiveData : Objective {
    public PayData targetPaydata;

    public override ObjectiveDelta ToDelta(LevelState state) {
        string targetIdn = state.SetLocationOfObjective(this);

        CyberNode target = state.delta.cyberGraph.nodes[targetIdn];
        target.payData = targetPaydata;

        ObjectiveDelta delta = new ObjectiveDelta(this, () => target.position, targetIdn);
        target.OnDataStolen += () => delta.status = ObjectiveStatus.complete;

        Debug.Log($"[objective data] applying objective data {name}:{targetPaydata} -> {target.nodeTitle}:{target.idn}");
        if (state.plan.objectiveLocations.ContainsKey(name)) {
            delta.visibility = Visibility.known;
        }
        if (delta.visibility == Visibility.known) {
            if (target.visibility < NodeVisibility.known) {
                target.visibility = NodeVisibility.known;
            }
        }

        foreach (CyberComponent cyberComponent in GameObject.FindObjectsOfType<CyberComponent>()) {
            if (cyberComponent.idn == targetIdn) {
                cyberComponent.OnDestroyCallback += () => delta.status = ObjectiveStatus.failed;
            }
        }

        return delta;
    }
}