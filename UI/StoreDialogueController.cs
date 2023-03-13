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
    public void Initialize(string buyerName, string storeName) {
        rightDialogueName.text = buyerName.ToLower();
        leftDialogueName.text = storeName.ToLower();
    }
    public void SetShopownerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        // dialogueText.text = dialogue;
        BlitDialogue(dialogue);
    }
    public void SetPlayerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 150f;
        dialogueRightSpacer.minWidth = 20f;
        // dialogueText.text = dialogue;
        BlitDialogue(dialogue);
    }
    void BlitDialogue(string content) {
        if (blitTextRoutine != null) {
            StopCoroutine(blitTextRoutine);
        }
        blitTextRoutine = StartCoroutine(BlitDialogueText(content));
    }

    public IEnumerator BlitDialogueText(string content) {
        dialogueText.text = "";
        int index = 0;
        float timer = 0f;
        float blitInterval = 0.025f;
        audioSource.clip = blitSound;
        audioSource.Play();
        while (timer < blitInterval && index < content.Length) {
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

}
