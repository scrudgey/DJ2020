using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public IEnumerator ShopownerCoroutine(string dialogue) {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        return BlitDialogueText(dialogue);
    }
    public void SetPlayerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 150f;
        dialogueRightSpacer.minWidth = 20f;
        BlitDialogue(dialogue);
    }
    public IEnumerator PlayerCoroutine(string dialogue) {
        dialogueLeftSpacer.minWidth = 150f;
        dialogueRightSpacer.minWidth = 20f;
        return BlitDialogueText(dialogue);
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
        blitTextRoutine = StartCoroutine(BlitDialogueText(content));
    }
    public void Clear() {
        if (blitTextRoutine != null) {
            StopCoroutine(blitTextRoutine);
        }
        dialogueText.text = "";
    }

    public IEnumerator BlitDialogueText(string content) {
        // Debug.Log($"blit dialogue text {content}");
        dialogueText.text = "";
        int index = 0;
        float timer = 0f;
        float blitInterval = 0.025f;
        audioSource.clip = blitSound;
        audioSource.Play();
        while (index < content.Length) {
            timer += Time.unscaledDeltaTime;
            if (timer >= blitInterval) {
                index += 1;
                timer -= blitInterval;
                dialogueText.text = content.Substring(0, index);
            }
            yield return null;
        }
        audioSource.Stop();
        dialogueText.text = content;
        blitTextRoutine = null;
    }

    public IEnumerator CutsceneDialogue(string content) {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        return BlitDialogueText(content);
    }


}
