using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Node {
    public string sceneName;
    public string idn;
    public string nodeTitle;
    public bool enabled;
    public Vector3 position;
    public NodeIcon icon;
    public NodeType type;
    public Node() { }
    public virtual bool getEnabled() {
        return enabled;
    }
    public void setEnabled(bool value) {
        enabled = value;
    }
}


public enum NodeType { none, normal, powerSource, WAN }
public enum NodeIcon { normal, power, mains }
