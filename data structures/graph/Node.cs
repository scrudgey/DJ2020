using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft;
using UnityEngine;

[System.Serializable]
public class Node<U> where U : Node<U> {
    public string sceneName;
    public string idn;
    public string nodeTitle;
    public bool enabled;
    public Vector3 position;
    public NodeVisibility visibility;
    public bool fixedVisibility;
    public bool alwaysOnScreen;

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