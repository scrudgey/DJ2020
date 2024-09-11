using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NodeUtilityInterfaceDisplay : MonoBehaviour {
    [Header("button")]
    public TextMeshProUGUI titleText;
    public Toggle toggle;
    [Header("image")]
    public TextMeshProUGUI statusText;
    public Color textDisabled;
    public Color textEnabled;
    bool value;
    CyberNode node;

    public void Configure(CyberNode node) {
        this.node = node;
        titleText.text = node.nodeTitle;
        value = node.utilityActive;
        SetButtons(value);
        toggle.gameObject.SetActive(node.lockLevel == 0);
    }
    public void UtilityButtonCallback(Toggle changer) {
        bool value = changer.isOn;
        GameManager.I.SetCyberNodeUtilityState(node, value);
        // terminalAnimation.HandleUtility();
    }

    void SetButtons(bool value) {
        toggle.SetIsOnWithoutNotify(value);
        if (value) {
            statusText.color = textEnabled;
            statusText.text = node.utilityEnabledText;
        } else {
            statusText.color = textDisabled;
            statusText.text = node.utilityDisabledText;
        }

    }

}

