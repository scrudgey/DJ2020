using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CyberNode : Node<CyberNode> {
    public bool compromised;    // TODO: why this instead of status? status is derived.
    public PayData payData;
    public CyberNodeType type;
    public int lockLevel;
    public bool dataSink;
    public bool dataStolen;
    public bool utilityActive;
    public CyberNodeStatus status;
    public CyberNode() { }
}


public enum CyberNodeType { normal, datanode, utility, WAN }

public enum CyberNodeStatus { invulnerable, vulnerable, compromised }