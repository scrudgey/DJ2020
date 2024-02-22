using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SoftwareSelector : MonoBehaviour {
    public SoftwareButton softwareButton;
    Action<SoftwareSelector> callback;
    public TextMeshProUGUI title;
    public SoftwareState softwareState;
    public Button myButton;
    [Header("default colorblock")]
    public ColorBlock defaultColorblock;
    [Header("disabled colorblock")]
    public ColorBlock disabledColorblock;
    public bool softwareEnabled;

    public void Initialize(SoftwareState softwareState, Action<SoftwareSelector> callback, CyberNode target) {
        this.callback = callback;
        this.softwareState = softwareState;
        title.text = softwareState.template.name;
        softwareButton.Initialize(softwareState);

        softwareEnabled = true;

        if (target != null) {
            softwareEnabled = softwareState.EvaluateCondition(target);
        }

        myButton.colors = softwareEnabled ? defaultColorblock : disabledColorblock;
    }
    public void ClickCallback() {
        callback?.Invoke(this);
    }
}
