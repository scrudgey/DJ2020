using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CyberComponent : GraphNodeComponent<CyberComponent, CyberNode> {
    public CyberNodeType nodeType;
    public int lockLevel;
    public Action OnDestroyCallback;
    public override CyberNode NewNode() {
        CyberNode node = base.NewNode();
        node.type = nodeType;
        node.lockLevel = lockLevel;
        node.utilityActive = true;
        return node;
    }
    override public void OnDestroy() {
        base.OnDestroy();
        if (GameManager.I == null || GameManager.I.gameData.levelState == null || GameManager.I.gameData.levelState.template == null) return;
        OnDestroyCallback?.Invoke();
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
