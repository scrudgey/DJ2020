using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GraphNodeComponent<T, U> : MonoBehaviour where T : GraphNodeComponent<T, U> where U : Node<U>, new() {
    public string idn;
    public string nodeTitle;
    [HideInInspector]
    public bool nodeEnabled;
    [HideInInspector]
    public U node;
    public T[] edges = new T[0];
    public EdgeVisibility minimumEdgeVisibility;
    public NodeVisibility minimumNodeVisibility;
    public virtual U NewNode() {
        return new U() {
            idn = idn,
            position = NodePosition(),
            anchorPosition = AnchorPosition(),
            enabled = true,
            nodeTitle = nodeTitle,
            visibility = NodeVisibility.known,
            minimumEdgeVisibility = minimumEdgeVisibility,
            minimumNodeVisibility = minimumNodeVisibility
        };
    }

    public Vector3 NodePosition() {
        Transform nodePositioner = transform.Find("node");
        if (nodePositioner) {
            return nodePositioner.position;
        } else return transform.position;
    }

    public Vector3 AnchorPosition() {
        Transform nodePositioner = transform.Find("anchor");
        if (nodePositioner) {
            return nodePositioner.position;
        } else return transform.position;
    }


    public Action<T> OnStateChange;

    public virtual void DisableSource() {
        GameManager.I?.SetNodeEnabled<U>(node, false);
    }
    public virtual void EnableSource() {
        GameManager.I?.SetNodeEnabled<U>(node, true);
    }
    void OnDisable() {
        if (GameManager.I.isLoadingLevel) return;
        DisableSource();
    }
    void OnEnable() {
        // TODO: move this to level initialization!
        EnableSource();
    }
    virtual public void OnDestroy() {
        if (GameManager.I.isLoadingLevel) return;

        DisableSource();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        foreach (T other in edges) {
            if (other == null)
                continue;
            Gizmos.DrawLine(NodePosition(), other.NodePosition());
        }
    }
#endif
}


