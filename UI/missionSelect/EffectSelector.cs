using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectSelector : MonoBehaviour {
    public Image icon;
    public GraphIconReference graphIconReference;
    Action<EffectSelector> callback;
    public TextMeshProUGUI title;
    public SoftwareEffect effect;
    public Button myButton;
    // [Header("default colorblock")]
    // public ColorBlock defaultColorblock;
    // [Header("disabled colorblock")]
    // public ColorBlock disabledColorblock;

    public void Initialize(SoftwareEffect effect, Action<EffectSelector> callback) {
        this.callback = callback;
        this.effect = effect;
        title.text = effect.TitleString();
        icon.sprite = graphIconReference.SoftwareEffectSprite(effect);
    }
    public void ClickCallback() {
        callback?.Invoke(this);
    }
}
