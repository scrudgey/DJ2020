using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DialogueController : MonoBehaviour {
    public enum DialogueResult { success, fail }
    public GameObject UIEditorCamera;
    public GameObject leftDialogueContainer;
    public GameObject rightDialogueContainer;
    public Transform responsesContainer;
    public Image leftPortrait;
    public Image rightPortrait;
    public TextMeshProUGUI leftPortraitCaption;
    public TextMeshProUGUI rightPortraitCaption;
    public TextMeshProUGUI leftDialogueText;
    public TextMeshProUGUI rightDialogueText;
    public GameObject responsePrefab;
    public Action<DialogueResult> OnDialogueConclude;
    [Header("Easings")]
    public RectTransform leftDialogueContainerRect;
    public RectTransform rightDialogueContainerRect;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    // public void Start() {
    //     Initialize();
    // }
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
            Conclude();
        } else {
            SetRightDialogueText(dialogueResponseButton.response);
            ClearResponseContainer();
            CreateContinueButton();
        }
    }
    public void SetLeftDialogueText(string dialogue) {
        leftDialogueText.text = "";
        leftDialogueContainer.SetActive(true);
        rightDialogueContainer.SetActive(false);
        StartCoroutine(WrapCoroutine(EaseInDialogueBox(leftDialogueContainerRect), () => {
            leftDialogueText.text = dialogue;
        }));
    }
    public void SetRightDialogueText(string dialogue) {
        rightDialogueText.text = "";
        leftDialogueContainer.SetActive(false);
        rightDialogueContainer.SetActive(true);
        StartCoroutine(WrapCoroutine(EaseInDialogueBox(rightDialogueContainerRect), () => {
            rightDialogueText.text = dialogue;
        }));
    }
    void Conclude() {
        OnDialogueConclude?.Invoke(DialogueResult.success);
    }

    IEnumerator WrapCoroutine(IEnumerator enumerator, Action callback) {
        // Coroutine coroutine = StartCoroutine(enumerator);
        yield return StartCoroutine(enumerator); ;
        callback();
    }

    IEnumerator EaseInDialogueBox(RectTransform rect) {
        // Rect rect = rectTransform.rect;
        float timer = 0f;
        float duration = 0.1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float width = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0f, 600f, duration);
            // element.minWidth = width;
            rect.sizeDelta = new Vector2(width, 200f);
            yield return null;
        }
        // element.minWidth = 600f;
        rect.sizeDelta = new Vector2(600f, 200f);

    }
}
