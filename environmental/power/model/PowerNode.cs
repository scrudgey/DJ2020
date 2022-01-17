using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerNodeType { none, normal, powerSource }

[System.Serializable]
public class PowerNode {
    public string idn;
    public bool powered;
    public bool enabled;
    public PowerNodeType type;
    // public bool powerSource;
    public Vector3 position;
    public PowerNode() { }
}
