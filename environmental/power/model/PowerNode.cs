using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerNodeType { none, normal, powerSource }

[System.Serializable]
public class PowerNode {
    public string idn;
    public string nodeTitle;
    public PowerNodeIcon icon;

    public bool powered;
    public bool enabled;
    public PowerNodeType type;
    public Vector3 position;
    public PowerNode() { }
}
