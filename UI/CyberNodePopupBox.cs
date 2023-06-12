using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CyberNodePopupBox : NodePopupBox<CyberNode, CyberGraph> {

    public TextMeshProUGUI compromisedText;
    public Color compromisedColor;
    protected override void SetGraphicalState(CyberNode node) {
        idText.text = node.idn.Substring(0, node.idn.Length / 2);
        nameText.text = node.nodeTitle;

        if (node.compromised) {
            compromisedText.text = $"COMPROMISED";
        } else {
            compromisedText.text = $"uncompromised";
        }
        Color activeColor = enabledColor;
        if (node.getEnabled()) {
            enabledText.text = $"Enabled: Y";
            if (node.compromised) {
                activeColor = compromisedColor;
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
