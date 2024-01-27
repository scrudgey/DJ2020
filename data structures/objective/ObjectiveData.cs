using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveData")]
public class ObjectiveData : Objective {
    public PayData targetPaydata;
    public override ObjectiveDelta ToDelta(LevelState state) {
        List<CyberNode> dataNodes = state.delta.cyberGraph.nodes.Values.Where(node => node.type == CyberNodeType.datanode).ToList();
        if (dataNodes.Count == 0) {
            Debug.LogError("Not enough data nodes to support level objectives! Mission is not possible.");
        }
        CyberNode target = Toolbox.RandomFromList(dataNodes);
        target.payData = targetPaydata;

        // dataNodes.Remove(target); // TODO: fix this so that multiple objectives can't select the same target node!

        ObjectiveDelta delta = new ObjectiveDelta(this, () => target.position);
        target.OnDataStolen += () => delta.status = ObjectiveStatus.complete;

        Debug.Log($"[objective data] applying objective data {name}:{targetPaydata} -> {target.nodeTitle}:{target.idn}");

        if (visibility == Visibility.known) {
            if (target.visibility < NodeVisibility.known) {
                target.visibility = NodeVisibility.known;
            }
        }

        return delta;
    }
}