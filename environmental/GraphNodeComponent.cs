using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GraphNodeComponent<T, U> : MonoBehaviour where T : GraphNodeComponent<T, U> where U : Node {
    public string idn;
    public string nodeTitle;
    public bool nodeEnabled;
    public NodeIcon icon;
    private bool applicationIsQuitting = false;

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
        OnStateChange?.Invoke((T)this);
    }
    public virtual void EnableSource() {
        GameManager.I?.SetNodeEnabled<T, U>((T)this, true);
        OnStateChange?.Invoke((T)this);
    }

    void OnDisable() {
        if (GameManager.I != null && !GameManager.I.isLoadingLevel)
            DisableSource();
    }
    void OnEnable() {
        // TODO: move this to level initialization!
        if (!GameManager.I.isLoadingLevel)
            EnableSource();
    }
    virtual public void OnDestroy() {
        DisableSource();
    }
    virtual public void Start() {
        // TODO: is this fine? why doesn't it work via enable source?
        if (enabled) {
            nodeEnabled = true;
            EnableSource();
        } else {
            nodeEnabled = false;
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
