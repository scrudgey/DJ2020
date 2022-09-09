using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PowerNodePopupBox : NodePopupBox<PowerNode, PowerGraph> {
    public TextMeshProUGUI poweredText;
    public Color unpoweredColor;
    protected override void SetGraphicalState(PowerNode node) {
        idText.text = node.idn.Substring(0, node.idn.Length / 2);
        nameText.text = node.nodeTitle;

        if (node.powered) {
            poweredText.text = $"Power: Y";
        } else {
            poweredText.text = $"Power: N";
        }
        Color activeColor = enabledColor;
        if (node.enabled) {
            enabledText.text = $"Enabled: Y";
            if (node.powered) {
                activeColor = enabledColor;
            } else {
                activeColor = unpoweredColor;
            }
        } else {
            enabledText.text = $"Enabled: N";
            activeColor = disabledColor;
        }

        foreach (GameObject dataObject in dataObjects) {
            TextMeshProUGUI text = dataObject.GetComponent<TextMeshProUGUI>();
            if (text != null)
                text.color = activeColor;
        }
        boxImage.color = activeColor;
    }
}
