using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DataFileButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI filenameText;
    Action<DataFileButton> callback;
    public PayData payData;
    public void Initialize(PayData payData, Action<DataFileButton> callback) {
        this.payData = payData;
        this.callback = callback;
        filenameText.text = payData.filename;
    }

    public void OnClick() {
        callback(this);
    }
}
