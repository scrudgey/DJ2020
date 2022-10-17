using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DialogueController : MonoBehaviour {
    public GameObject leftPortraitContainer;
    public GameObject rightPortraitContainer;
    public Transform responsesContainer;
    public Image leftPortrait;
    public Image rightPortrait;
    public TextMeshProUGUI leftPortraitCaption;
    public TextMeshProUGUI rightPortraitCaption;
    public TextMeshProUGUI dialogueText;
    public GameObject responsePrefab;
    public void Start() {
        Initialize();
    }
    void ClearResponseContainer() {
        foreach (Transform child in responsesContainer) {
            Destroy(child.gameObject);
        }
    }
    public void Initialize() {
        ClearResponseContainer();
        CreateDialogueResponse("[ESCAPE] Excuse me, I think I left my identification in my car.");
        CreateDialogueResponse("[LIE] I am J. Garage Aroo, wealthy industrialist.");
        CreateDialogueResponse("[BLUFF] Rockwell isn't going to be very happy if you delay our meeting!");
        CreateDialogueResponse("[ITEM] Sure, check my ID card.");
        SetLeftDialogueText("You there, Stop! You're not authorized to be in this area! Show me your identification!");
    }
    public void CreateDialogueResponse(string response) {
        GameObject responseObj = GameObject.Instantiate(responsePrefab);
        responseObj.transform.SetParent(responsesContainer, false);
        DialogueResponseButton button = responseObj.GetComponent<DialogueResponseButton>();
        button.Initialize(this, response);
    }
    public void CreateContinueButton() {
        GameObject responseObj = GameObject.Instantiate(responsePrefab);
        responseObj.transform.SetParent(responsesContainer, false);
        DialogueResponseButton button = responseObj.GetComponent<DialogueResponseButton>();
        button.Initialize(this, "[CONTINUE]");
        button.continueButton = true;
    }
    public void DialogueResponseCallback(DialogueResponseButton dialogueResponseButton) {
        if (dialogueResponseButton.continueButton) {
            Debug.Log("continue");
        } else {
            SetRightDialogueText(dialogueResponseButton.response);
            ClearResponseContainer();
            CreateContinueButton();
        }
    }
    public void SetLeftDialogueText(string dialogue) {
        dialogueText.text = dialogue;
        leftPortraitContainer.SetActive(true);
        rightPortraitContainer.SetActive(false);
    }
    public void SetRightDialogueText(string dialogue) {
        dialogueText.text = dialogue;
        leftPortraitContainer.SetActive(false);
        rightPortraitContainer.SetActive(true);
    }
}
