using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlarmNodeInfoDisplay : NodeInfoPaneDisplay<AlarmGraph, AlarmNode, AlarmNodeIndicator> {
    public TextMeshProUGUI nodeTypeText;
    public TextMeshProUGUI triggeredText;
    public TextMeshProUGUI countdownText;
    public override void ConfigureNode() {
        switch (node.nodeType) {
            case AlarmNode.AlarmNodeType.normal:
                nodeTypeText.text = "alarm node";
                break;
            case AlarmNode.AlarmNodeType.terminal:
                nodeTypeText.text = "alarm terminal";
                break;
            case AlarmNode.AlarmNodeType.radio:
                nodeTypeText.text = "radio";
                break;
        }

        if (node.alarmTriggered) {
            triggeredText.text = "triggered";
            countdownText.text = $"{node.countdownTimer}";
        } else {
            triggeredText.text = "not triggered";
            countdownText.text = $"";
        }
    }
    public override void ConfigureMysteryNode() {

        title.text = "unknown";
        icon.sprite = mysteryIcon;
        lockStatus.text = "";
        nodeTypeText.text = "";
        triggeredText.text = "";

        icon.color = mysteryColor;
        title.color = mysteryColor;
    }

}
