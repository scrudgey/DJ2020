using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNodeComponent<T> : MonoBehaviour where T : GraphNodeComponent<T> {
    public string idn;
    public string nodeTitle;
    public PowerNodeIcon icon;

    public T[] edges = new T[0];

    public Vector3 NodePosition() {
        Transform nodePositioner = transform.Find("node");
        if (nodePositioner) {
            return nodePositioner.position;
        } else return transform.position;
    }

    public Action<T> OnStateChange;


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        // string customName = "Relic\\" + relicType.ToString() + ".png";
        // Gizmos.DrawIcon(transform.position, customName, true);
        foreach (T other in edges) {
            if (other == null)
                continue;
            Gizmos.DrawLine(NodePosition(), other.NodePosition());
        }
    }
#endif
}
