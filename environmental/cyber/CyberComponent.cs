using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberComponent : GraphNodeComponent<CyberComponent, CyberNode> {
    public CyberNodeType nodeType;
    public override CyberNode NewNode() {
        CyberNode node = base.NewNode();
        node.type = nodeType;
        return node;
    }
    override public void OnDestroy() {
        base.OnDestroy();
        // check if we invalidate an objective
        if (GameManager.I == null || GameManager.I.gameData.levelState == null || GameManager.I.gameData.levelState.template == null) return;
        CyberNode node = GameManager.I.GetCyberNode(idn);
        foreach (Objective objective in GameManager.I.gameData.levelState.template.objectives) {
            if (objective is ObjectiveData) {
                ObjectiveData objectiveData = (ObjectiveData)objective;
                if (node.payData == objectiveData.targetPaydata) {
                    GameManager.I.FailObjective(objective);
                }
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos() {
        foreach (CyberComponent other in edges) {
            if (other == null)
                continue;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(NodePosition(), other.NodePosition());
        }
    }
#endif
}
