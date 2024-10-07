using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class StoreDialogueController : MonoBehaviour {
    public AudioSource audioSource;

    [Header("dialogue")]
    public Image leftImage;
    public Image rightImage;
    public TextMeshProUGUI dialogueText;
    public LayoutElement dialogueLeftSpacer;
    public LayoutElement dialogueRightSpacer;
    public TextMeshProUGUI leftDialogueName;
    public TextMeshProUGUI rightDialogueName;
    public AudioClip blitSound;
    Coroutine blitTextRoutine;

    public void SetImages(Sprite left) {
        // if (left != null) {
        leftImage.sprite = left;
        // }
        // if (right != null) {
        // rightImage.sprite = right;
        // }
    }
    public void Initialize(string buyerName, string storeName) {
        rightDialogueName.text = buyerName.ToLower();
        leftDialogueName.text = storeName.ToLower();
    }

    public void SetShopownerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        BlitDialogue(dialogue);
    }
    public void SetPlayerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 150f;
        dialogueRightSpacer.minWidth = 20f;
        BlitDialogue(dialogue);
    }
    public IEnumerator ShopownerCoroutine(string dialogue) {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        return CutsceneDialogue(dialogue);
    }
    public void MoveDialogueBox(bool playerSide) {
        if (playerSide) {
            dialogueLeftSpacer.minWidth = 150f;
            dialogueRightSpacer.minWidth = 20f;
        } else {
            dialogueLeftSpacer.minWidth = 20f;
            dialogueRightSpacer.minWidth = 150f;
        }
    }
    void BlitDialogue(string content) {
        if (blitTextRoutine != null) {
            StopCoroutine(blitTextRoutine);
        }
        blitTextRoutine = StartCoroutine(CutsceneDialogue(content));
    }
    public void Clear() {
        if (blitTextRoutine != null) {
            StopCoroutine(blitTextRoutine);
        }
        dialogueText.text = "";
    }
    public IEnumerator CutsceneDialogue(string content, string trailer = "") {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        audioSource.clip = blitSound;
        audioSource.Play();
        yield return Toolbox.BlitText(dialogueText, content, interval: 0.025f, trailer: "▮");
        audioSource.Stop();
    }


}
