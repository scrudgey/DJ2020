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
    public bool isManualHackerTarget;
    public CyberNodeStatus status;
    public CyberNodeStatus getStatus() {
        if (compromised) {
            return CyberNodeStatus.compromised;
        } else if (isManualHackerTarget && status < CyberNodeStatus.vulnerable) {
            return CyberNodeStatus.vulnerable;
        } else return status;
    }
    public CyberNode() { }
}


public enum CyberNodeType { normal, datanode, utility, WAN }

public enum CyberNodeStatus { invulnerable, vulnerable, compromised }