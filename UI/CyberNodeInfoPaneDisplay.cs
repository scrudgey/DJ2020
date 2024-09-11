using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CyberNodeInfoPaneDisplay : NodeInfoPaneDisplay<CyberGraph, CyberNode, NeoCyberNodeIndicator> {
    public TextMeshProUGUI type;
    public TextMeshProUGUI status;
    public NodeDataInfoDisplay dataInfoDisplay;
    public NodeUtilityInterfaceDisplay utilityInterfaceDisplay;
    [Header("locks")]
    public GameObject lockedPane;
    public GameObject lockPaneBottom;
    public TextMeshProUGUI passwordButtonNumber;
    public Button passwordButton;
    public GameObject unlockedPane;
    [Header("colors")]
    public Color invulnerableColor;
    public Color vulnerableColor;
    public Color compromisedColor;

    public override void ConfigureNode() {
        // indicator.componen
        type.text = indicator.node.type switch {
            CyberNodeType.datanode => "data store",
            CyberNodeType.normal => "",
            CyberNodeType.player => "cyberdeck",
            CyberNodeType.utility => "utility",
            CyberNodeType.WAN => "internet"
        };
        status.text = $"{indicator.node.getStatus()}";

        if (node.lockLevel > 0) {
            lockedPane.SetActive(true);
            unlockedPane.SetActive(false);
        } else {
            lockedPane.SetActive(false);
            unlockedPane.SetActive(true);
        }

        int numberPasswords = GameManager.I.gameData.numberPasswords();
        lockPaneBottom.SetActive(numberPasswords > 0);
        passwordButtonNumber.text = $"{numberPasswords}";
        passwordButton.interactable = numberPasswords > 0;

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
        dataInfoDisplay.Configure(node);

        if (node.type == CyberNodeType.datanode && node.payData != null) {
            dataInfoDisplay.gameObject.SetActive(true);
            utilityInterfaceDisplay.gameObject.SetActive(false);
        } else if (node.type == CyberNodeType.utility) {
            utilityInterfaceDisplay.Configure(node);
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

        icon.color = mysteryColor;
        title.color = mysteryColor;
        type.color = mysteryColor;
        status.color = mysteryColor;

        dataInfoDisplay.gameObject.SetActive(false);
        utilityInterfaceDisplay.gameObject.SetActive(false);
    }

    public void PasswordDataButtonCallback() {
        PayData passwordData = GameManager.I.gameData.levelState.delta.levelAcquiredPaydata.Where(data => data.type == PayData.DataType.password).FirstOrDefault();
        if (passwordData != null) {
            node.lockLevel = 0;
            node.datafileVisibility = true;
            GameManager.I.RemovePayData(passwordData, node.position);
            GameManager.I.RefreshCyberGraph();
        }
    }
}
