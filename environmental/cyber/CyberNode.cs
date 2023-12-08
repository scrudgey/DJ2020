using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CyberNode : Node {
    public bool compromised;
    public CyberNodeType type;
    public CyberNode() { }
}

public enum CyberNodeType { normal, datanode, utility }