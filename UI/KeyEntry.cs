using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class KeyEntry : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public GraphIconReference graphIconReference;
    public Image icon;
    public KeyData data;
    public Color green;
    public Button button;
    Action<KeyData> callback;

    public void Configure(Action<KeyData> callback, KeyData data, bool enabled = true) {
        this.callback = callback;
        this.data = data;
        icon.sprite = graphIconReference.KeyinfoSprite(data);
        icon.color = green;
        nameText.text = $"{data.idn}";
        if (!enabled) {
            button.interactable = false;
        }
    }
    public void Clicked() {
        callback?.Invoke(data);
    }
}
