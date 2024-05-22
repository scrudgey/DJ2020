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
    Action<SoftwareSelector> mouseoverCallback;
    Action<SoftwareSelector> mouseExitCallback;

    public TextMeshProUGUI title;
    public TextMeshProUGUI sizeText;
    public SoftwareState softwareState;
    public SoftwareTemplate softwareTemplate;
    public Button myButton;
    [Header("default colorblock")]
    public ColorBlock defaultColorblock;
    [Header("disabled colorblock")]
    public ColorBlock disabledColorblock;
    public bool softwareEnabled;
    public List<CyberNode> path;

    public void Initialize(SoftwareState softwareState, Action<SoftwareSelector> callback, CyberNode target, CyberNode origin, List<CyberNode> path) {
        this.callback = callback;
        this.softwareState = softwareState;
        this.path = path;
        title.text = softwareState.template.name;
        sizeText.text = $"{softwareState.template.CalculateSize()} MB";
        sizeText.gameObject.SetActive(false);
        softwareButton.Initialize(softwareState);

        softwareEnabled = true;

        if (target != null) {
            softwareEnabled = softwareState.EvaluateCondition(target, origin, path);
        }

        myButton.colors = softwareEnabled ? defaultColorblock : disabledColorblock;
    }
    public void Initialize(SoftwareTemplate template, Action<SoftwareSelector> callback, Action<SoftwareSelector> mouseoverCallback, Action<SoftwareSelector> mouseExitCallback) {
        this.callback = callback;
        this.mouseoverCallback = mouseoverCallback;
        this.softwareTemplate = template;
        this.mouseExitCallback = mouseExitCallback;
        title.text = template.name;
        sizeText.text = $"{template.CalculateSize()} MB";
        softwareButton.Initialize(template);
        myButton.colors = defaultColorblock;
    }
    public void ClickCallback() {
        callback?.Invoke(this);
    }
    public void MouseoverCallback() {
        mouseoverCallback?.Invoke(this);
    }
    public void MouseExitCallback() {
        mouseExitCallback?.Invoke(this);
    }
    public void SetInteractivility(bool value) {
        myButton.interactable = value;
    }
}
