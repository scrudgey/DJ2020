using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberComponent : GraphNodeComponent<CyberComponent, CyberNode> {
    protected bool _compromised;
    public virtual bool compromised {
        get { return _compromised; }
        set {
            bool dirty = _compromised != value;
            _compromised = value;
            // if (dirty)
            OnStateChange?.Invoke(this);
        }
    }

    private float _progress;
    public float progress {
        get { return _progress; }
        set {
            _progress = value;
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
