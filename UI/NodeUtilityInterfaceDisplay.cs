using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NodeUtilityInterfaceDisplay : MonoBehaviour {
    [Header("button")]
    public TextMeshProUGUI titleText;
    // public Button onButton;
    // public TextMeshProUGUI onButtonText;
    // public Button offButton;
    // public TextMeshProUGUI offButtonText;
    [Header("image")]
    // public Image background;
    public TextMeshProUGUI statusText;
    // [Header("colors")]
    // public Color green;
    // public Color black;
    // public Color backgroundDisabled;
    // public Color backgroundEnabled;
    public Color textDisabled;
    public Color textEnabled;
    bool value;
    CyberNode node;

    public void Configure(CyberNode node) {
        this.node = node;
        titleText.text = node.nodeTitle;
        value = node.utilityActive;
        SetButtons(value);
    }

    public void OnButtonClick() {
        value = true;
        GameManager.I.SetCyberNodeUtilityState(node, value);
        SetButtons(value);
    }

    public void OffButtonClick() {
        value = false;
        GameManager.I.SetCyberNodeUtilityState(node, value);
        SetButtons(value);
    }

    void SetButtons(bool value) {
        if (value) {
            // ColorBlock onColorBlock = onButton.colors;
            // onColorBlock.normalColor = green;
            // onButton.colors = onColorBlock;

            // ColorBlock offColorBlock = offButton.colors;
            // offColorBlock.normalColor = black;
            // offButton.colors = offColorBlock;

            // onButtonText.color = black;
            // offButtonText.color = green;


            // background.color = backgroundEnabled;
            statusText.color = textEnabled;
            statusText.text = "ENABLED";

        } else {
            // ColorBlock onColorBlock = onButton.colors;
            // onColorBlock.normalColor = black;
            // onButton.colors = onColorBlock;

            // ColorBlock offColorBlock = offButton.colors;
            // offColorBlock.normalColor = green;
            // offButton.colors = offColorBlock;

            // onButtonText.color = green;
            // offButtonText.color = black;


            // background.color = backgroundDisabled;
            statusText.color = textDisabled;
            statusText.text = "DISABLED";
        }

    }
}

