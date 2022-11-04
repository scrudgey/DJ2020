using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class DialogueResponseButton : MonoBehaviour {
    public string response;
    public TextMeshProUGUI text;
    public Action<DialogueResponseButton> responseCallback;
    public void Initialize(Action<DialogueResponseButton> responseCallback, string response) {
        this.responseCallback = responseCallback;
        this.response = response;
        text.text = response;
    }
    public void OnClick() {
        responseCallback(this);
    }
}
