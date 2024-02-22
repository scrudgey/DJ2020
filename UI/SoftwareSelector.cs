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
    public void Initialize(SoftwareState softwareState, Action<SoftwareSelector> callback) {
        this.callback = callback;
        this.softwareState = softwareState;
        title.text = softwareState.template.name;
        softwareButton.Initialize(softwareState);
    }
    public void ClickCallback() {
        callback?.Invoke(this);
    }
}
