using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PoweredComponent : GraphNodeComponent<PoweredComponent, PowerNode> {
    private bool _power;
    public bool power {
        get { return _power; }
        set {
            bool dirty = _power != value;
            _power = value;
            // if (dirty)
            OnStateChange?.Invoke(this);
        }
    }
    public override PowerNode GetNode() => GameManager.I.GetPowerNode(idn);

#if UNITY_EDITOR
    protected override void OnDrawGizmos() {
        foreach (PoweredComponent other in edges) {
            if (other == null)
                continue;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(NodePosition(), other.NodePosition());
        }
    }
#endif
}
