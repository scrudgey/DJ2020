using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerNodeType { none, normal, powerSource }

[System.Serializable]
public class PowerNode : Node {
    public PowerNodeIcon icon;
    public bool powered;
    public PowerNodeType type;
    public PowerNode() { }
}
