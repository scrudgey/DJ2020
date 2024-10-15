using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoftwareButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI levelCaption;
    public Button button;
    public Color enabledColor;
    public Color disabledColor;

    [HideInInspector]
    public SoftwareState state;
    [HideInInspector]
    public SoftwareTemplate template;
    Action<SoftwareButton> callback;
    public void Initialize(SoftwareState state) {
        this.state = state;
        icon.sprite = state.template.icon;
        levelCaption.text = state.template.infiniteCharges ? "-" : $"{state.charges}";
        caption.text = state.template.name;
    }
    public void Initialize(SoftwareTemplate template) {
        this.template = template;
        icon.sprite = template.icon;
        levelCaption.text = template.infiniteCharges ? "-" : $"{template.maxCharges}";
        caption.text = template.name;
    }

    public void Initialize(SoftwareState state, Action<SoftwareButton> callback, bool enabled) {
        this.state = state;
        this.callback = callback;
        caption.text = state.template.name;
        icon.sprite = state.template.icon;
        levelCaption.text = state.template.infiniteCharges ? "-" : $"{state.charges}";
        button.interactable = enabled;
        Color color = enabled ? enabledColor : disabledColor;
        levelCaption.color = color;
        caption.color = color;
    }

    public void OnClick() {
        callback?.Invoke(this);
    }
}

