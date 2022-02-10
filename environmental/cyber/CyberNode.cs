using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CyberNodeType { none, normal, powerSource }

[System.Serializable]
public class CyberNode : Node {
    // public PowerNodeIcon icon;
    public bool compromised;
    // public CyberNodeType type;
    public CyberNode() { }
}
