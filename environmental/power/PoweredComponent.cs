using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PoweredComponent : GraphNodeComponent<PoweredComponent, PowerNode> {
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
