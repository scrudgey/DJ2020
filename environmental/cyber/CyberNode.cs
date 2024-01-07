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
    public bool BeDiscovered() {
        if (visibility == NodeVisibility.unknown || visibility == NodeVisibility.mystery) {
            visibility = NodeVisibility.known;
            return true;
        } else {
            return false;
        }
    }
    public override NodeVisibility GetVisibility() {
        if (isManualHackerTarget && visibility == NodeVisibility.unknown) {
            return NodeVisibility.mystery;
        } else {
            return base.GetVisibility();
        }
    }
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