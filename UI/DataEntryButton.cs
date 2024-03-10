using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DataEntryButton : MonoBehaviour {

    public TextMeshProUGUI nameText;
    public GraphIconReference graphIconReference;
    public Image icon;
    public PayData data;
    Action<PayData> callback;

    public void Configure(Action<PayData> callback, PayData data) {
        this.callback = callback;
        this.data = data;
        icon.sprite = graphIconReference.DataSprite(data.type, true);
        nameText.text = $"{data.filename}";
    }
    public void Clicked() {
        callback?.Invoke(data);
    }
}
