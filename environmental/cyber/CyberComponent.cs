using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberComponent : GraphNodeComponent<CyberComponent, CyberNode> {
    protected bool _compromised;
    public virtual bool compromised {
        get { return _compromised; }
        set {
            _compromised = value;
            OnStateChange?.Invoke(this);
        }
    }

    public override CyberNode GetNode() => GameManager.I.GetCyberNode(idn);


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
