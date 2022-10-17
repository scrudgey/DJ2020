using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class DialogueResponseButton : MonoBehaviour {
    public DialogueController dialogueController;
    public string response;
    public TextMeshProUGUI text;
    public bool continueButton;
    public void Initialize(DialogueController dialogueController, string response) {
        this.dialogueController = dialogueController;
        this.response = response;
        text.text = response;
    }
    public void OnClick() {
        dialogueController.DialogueResponseCallback(this);
    }
}
