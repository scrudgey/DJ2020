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

    [NonSerialized]
    Action OnValueChanged;
    // public NodeIcon icon;
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



public enum NodeIcon { normal, power, mains }
