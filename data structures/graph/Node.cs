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
    // public NodeIcon icon;
    public Node() { }
    public virtual bool getEnabled() {
        return enabled;
    }
    public void setEnabled(bool value) {
        enabled = value;
    }
}


public enum NodeIcon { normal, power, mains }
