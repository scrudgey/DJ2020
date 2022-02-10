using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerNodeType { none, normal, powerSource }

[System.Serializable]
public class PowerNode : Node {
    public bool powered;
    public PowerNode() { }
}
