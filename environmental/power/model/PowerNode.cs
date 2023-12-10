using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType { none, powerSource }

[System.Serializable]
public class PowerNode : Node<PowerNode> {
    public NodeType type;
    public bool powered;
    public PowerNode() { }
}
