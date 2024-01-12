using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PowerNode : Node<PowerNode> {
    public enum NodeType { none, powerSource }
    public NodeType type;
    public bool powered;
    public PowerNode() { }
}
