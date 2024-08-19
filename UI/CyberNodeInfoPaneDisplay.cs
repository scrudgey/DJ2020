using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CyberNodeInfoPaneDisplay : NodeInfoPaneDisplay<CyberGraph, CyberNode, NeoCyberNodeIndicator> {
    public TextMeshProUGUI type;
    public TextMeshProUGUI status;
    public NodeDataInfoDisplay dataInfoDisplay;
    public NodeUtilityInterfaceDisplay utilityInterfaceDisplay;
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;
    public GameObject lockBlock;

    public override void ConfigureNode() {
        type.text = indicator.node.type switch {
            CyberNodeType.datanode => "data store",
            CyberNodeType.normal => "",
            CyberNodeType.player => "cyberdeck",
            CyberNodeType.utility => "utility",
            CyberNodeType.WAN => "internet"
        };
        status.text = $"{indicator.node.getStatus()}";
        if (indicator.node.lockLevel > 0) {
            lockStatus.text = $"LOCKED";
        } else {
            lockStatus.text = $"ACCESS GRANTED";
        }
        // lockStatus.text = $"lock: {indicator.node.lockLevel}";
        // lockBlock.SetActive(indicator.node.lockLevel > 0);
        lockBlock.SetActive(false);

        Color statusColor = indicator.node.getStatus() switch {
            CyberNodeStatus.invulnerable => invulnerableColor,
            CyberNodeStatus.vulnerable => vulnerableColor,
            CyberNodeStatus.compromised => compromisedColor,
            _ => invulnerableColor
        };
        icon.color = statusColor;
        title.color = statusColor;
        type.color = statusColor;
        status.color = statusColor;

        if (node.datafileVisibility && indicator.node.type == CyberNodeType.datanode && indicator.node.payData != null) {
            dataInfoDisplay.Configure(indicator.node.payData, indicator.node.dataStolen);
            dataInfoDisplay.gameObject.SetActive(true);
            utilityInterfaceDisplay.gameObject.SetActive(false);
        } else if (indicator.node.type == CyberNodeType.utility) {
            utilityInterfaceDisplay.Configure(indicator.node);
            dataInfoDisplay.gameObject.SetActive(false);
            utilityInterfaceDisplay.gameObject.SetActive(true);
        } else {
            dataInfoDisplay.gameObject.SetActive(false);
            utilityInterfaceDisplay.gameObject.SetActive(false);
        }
    }

    public override void ConfigureMysteryNode() {
        title.text = "unknown";
        icon.sprite = mysteryIcon;
        type.text = $"???";
        status.text = "";
        lockStatus.text = "";

        icon.color = mysteryColor;
        title.color = mysteryColor;
        type.color = mysteryColor;
        status.color = mysteryColor;

        dataInfoDisplay.gameObject.SetActive(false);
        utilityInterfaceDisplay.gameObject.SetActive(false);
    }
}
