using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class AlertCanvasHandler : MonoBehaviour {
    public TitleController titleController;
    public TextMeshProUGUI contentText;
    public void ShowAlert(string content) {
        contentText.text = content;
    }
    public void CancelCallback() {
        titleController.AlertCancelCallback();
    }
}
