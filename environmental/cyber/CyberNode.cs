using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
[System.Serializable]
public class CyberNode : Node<CyberNode> {
    public bool compromised;    // TODO: why this instead of status? status is derived.
    public PayData payData;
    public CyberNodeType type;
    public int lockLevel;
    public bool dataSink;
    public bool dataStolen;
    public bool datafileVisibility;
    public CyberNodeStatus status;

    public bool utilityActive;
    public string utilityEnabledText = "ENABLED";
    public string utilityDisabledText = "DISABLED";

    [System.NonSerialized]
    [XmlIgnore]
    public Action OnDataStolen;
    public override NodeVisibility GetVisibility() {
        // TODO: this is weird
        if (datafileVisibility) {
            return NodeVisibility.known;
        } else return base.GetVisibility();
    }
    public CyberNodeStatus getStatus() {
        if (compromised) {
            return CyberNodeStatus.compromised;
        } else if (lockLevel <= 0) {
            return CyberNodeStatus.vulnerable;
        } else return CyberNodeStatus.invulnerable;
    }
    public CyberNode() { }

    public override MarkerConfiguration GetConfiguration(GraphIconReference graphIconReference) {
        return new MarkerConfiguration() {
            icon = graphIconReference.CyberNodeSprite(this),
            color = graphIconReference.minimapCyberColor,
            worldPosition = position,
            nodeVisibility = visibility
        };
    }

    public void DownloadData() {
        GameManager.I.AddPayDatas(payData, position);
        dataStolen = true;
        OnDataStolen?.Invoke();
        CutsceneManager.I.HandleTrigger("download_complete");
    }
}


public enum CyberNodeType { normal, datanode, utility, WAN, player }

public enum CyberNodeStatus { invulnerable, vulnerable, compromised }