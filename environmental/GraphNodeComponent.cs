using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GraphNodeComponent<T, U> : MonoBehaviour where T : GraphNodeComponent<T, U> where U : Node {
    public string idn;
    public string nodeTitle;
    public NodeIcon icon;

    public T[] edges = new T[0];

    public Vector3 NodePosition() {
        Transform nodePositioner = transform.Find("node");
        if (nodePositioner) {
            return nodePositioner.position;
        } else return transform.position;
    }

    public Action<T> OnStateChange;

    public virtual void DisableSource() {
        GameManager.I?.SetNodeEnabled<T, U>((T)this, false);
    }
    public virtual void EnableSource() {
        GameManager.I?.SetNodeEnabled<T, U>((T)this, true);
    }

    void OnDisable() {
        DisableSource();
    }
    void OnEnable() {
        EnableSource();
    }
    void OnDestroy() {
        DisableSource();
    }
    void Start() {
        if (enabled) {
            EnableSource();
        } else {
            DisableSource();
        }
    }
    public virtual void ConfigureNode(U node) {

    }

    public abstract U GetNode();

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
