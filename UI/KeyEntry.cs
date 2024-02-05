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
    Action<KeyData> callback;

    public void Configure(Action<KeyData> callback, KeyData data) {
        this.callback = callback;
        this.data = data;
        icon.sprite = data.type switch {
            KeyType.keycard => graphIconReference.keyCard,
            KeyType.physical => graphIconReference.physicalKey,
            KeyType.password => graphIconReference.password,
            _ => graphIconReference.keyCard
        };
        icon.color = data.type == KeyType.password ? green : Color.white;
        nameText.text = $"{data.idn}";
    }
    public void Clicked() {
        callback?.Invoke(data);
    }
}
