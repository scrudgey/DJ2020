using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft;
using UnityEngine;

[System.Serializable]
public class Node<U> : IMarkerProvider where U : Node<U> {
    public string sceneName;
    public string idn;
    public string nodeTitle;
    public bool enabled;
    public Vector3 position;
    public NodeVisibility visibility;
    public EdgeVisibility minimumEdgeVisibility;
    public NodeVisibility minimumNodeVisibility;
    public bool fixedVisibility;
    public bool alwaysOnScreen;
    public bool straightLine;
    public bool notClickable;
    public bool onlyShowIfHackDeployed;
    public virtual MarkerConfiguration GetConfiguration(GraphIconReference graphIconReference) {
        return new MarkerConfiguration();
    }
    public virtual NodeVisibility GetVisibility() {
        return visibility;
    }
    [NonSerialized]
    Action OnValueChanged;
    public Node() { }
    public virtual bool getEnabled() {
        return enabled;
    }
    public void Bind(Action binder) {
        OnValueChanged += binder;
    }
    public void setEnabled(bool value) {
        enabled = value;
    }
    public void ValueChanged() {
        OnValueChanged?.Invoke();
    }
}



public enum NodeVisibility { unknown, mystery, known }

public enum EdgeVisibility { unknown, known }