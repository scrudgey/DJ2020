using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NeoDialogueController : MonoBehaviour {
    public float currentThreshold;
    public GameObject UIEditorCamera;
    public Image leftPortrait;
    public Image rightPortrait;
    public TextMeshProUGUI leftPortraitCaption;
    public TextMeshProUGUI rightPortraitCaption;
    public TextMeshProUGUI doubterText;
    public Color red;

    [Header("Dialogue")]
    public Transform dialogueContainer;
    public GameObject dialoguePrefab;
    public RectTransform dialogueTopPadding;

    [Header("Statuses")]
    public Transform leftStatusContainer;
    public Transform rightStatusContainer;
    public GameObject statusElementPrefab;
    public TextMeshProUGUI appearanceText;

    [Header("audio")]
    public AudioSource audioSource;
    public AudioClip[] nextDialogueSound;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
        ClearDialogueContainer();
    }
    public void Start() {
        doubterText.enabled = false;
    }
    void ClearDialogueContainer() {
        foreach (Transform child in dialogueContainer) {
            if (child == dialogueTopPadding.transform) {
                continue;
            }
            Destroy(child.gameObject);
        }
    }
    public void Initialize(DialogueInput input) {
        CharacterController playerController = input.playerObject.GetComponentInChildren<CharacterController>();
        CharacterController npcController = input.npcObject.GetComponentInChildren<CharacterController>();
        currentThreshold = 25f;

        Vector3 playerPosition = input.playerObject.transform.position;
        Vector3 npcPosition = input.npcObject.transform.position;

        PlayerInput playerInput = new PlayerInput {
            lookAtPosition = npcPosition,
            snapToLook = true,
            Fire = PlayerInput.FireInputs.none
        };
        PlayerInput npcInput = new PlayerInput {
            lookAtPosition = playerPosition,
            snapToLook = true,
            Fire = PlayerInput.FireInputs.none
        };
        playerController.SetInputs(playerInput);
        if (npcController != null)
            npcController.SetInputs(npcInput);
        // yield return new WaitForEndOfFrame();
        // yield return new WaitForEndOfFrame();

        SetAppearance(input);
        SetPortraits(input);
        ClearDialogueContainer();
    }

    public void SetPortraits(DialogueInput input) {
        leftPortrait.sprite = input.npcCharacter.portrait;
        rightPortrait.sprite = input.playerState.portrait;

        leftPortraitCaption.text = input.npcCharacter.name;
        rightPortraitCaption.text = input.playerName;
    }

    public void SetAppearance(DialogueInput input) {
        appearanceText.text = input.playerSuspiciousness switch {
            Suspiciousness.normal => "appearance ... OK <i>!!<.i>",
            Suspiciousness.suspicious => "appearance ... <size=22><color=#eccc68>Highly Suspicious</color></size>",
            Suspiciousness.aggressive => "appearance ... <size=22><color=#ff4757>Openly Aggressive</color></size>",
            _ => ""
        };
    }

    IEnumerator easeOutDialogue(RectTransform container) {
        // LayoutElement containerElement = container.GetComponent<LayoutElement>();
        LayoutElement paddingElement = dialogueTopPadding.GetComponent<LayoutElement>();
        float baseHeight = container.rect.height;
        IEnumerator shrinker = Toolbox.Ease(null, 1f, baseHeight, 0f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            // container.minHeight = amount;
            // dialogueTopPadding.sizeDelta = new Vector2(1000f, amount);
            paddingElement.minHeight = amount;
        }, unscaledTime: true);
        IEnumerator destroyer = Toolbox.CoroutineFunc(() => {
            Destroy(container.gameObject);
            // dialogueTopPadding.sizeDelta = new Vector2(1000f, baseHeight);
            paddingElement.minHeight = baseHeight;
        });
        return Toolbox.ChainCoroutines(shrinker, destroyer);
    }
    void CheckAndEaseOutEarliestDialogue() {
        if (dialogueContainer.childCount > 6) {
            // Transform earliest = dialogueContainer.GetChild(1);
            // RectTransform container = earliest.GetComponent<RectTransform>();
            // StartCoroutine(easeOutDialogue(container));
            Destroy(dialogueContainer.transform.GetFirstChild(true));
        }
    }
    public IEnumerator SetLeftDialogueText(string content, string challengeContent) {
        yield return null;
        Toolbox.RandomizeOneShot(audioSource, nextDialogueSound, randomPitchWidth: 0.05f);
        GameObject newDialogue = GameObject.Instantiate(dialoguePrefab);
        newDialogue.transform.SetParent(dialogueContainer, false);
        DialogueTextPackage dialogue = newDialogue.GetComponent<DialogueTextPackage>();
        IEnumerator blitter = dialogue.Initialize(content, challengeContent, true);
        CheckAndEaseOutEarliestDialogue();
        yield return blitter;
    }
    public IEnumerator SetRightDialogueText(string content) {
        yield return null;
        Toolbox.RandomizeOneShot(audioSource, nextDialogueSound, randomPitchWidth: 0.05f);
        GameObject newDialogue = GameObject.Instantiate(dialoguePrefab);
        newDialogue.transform.SetParent(dialogueContainer, false);
        DialogueTextPackage dialogue = newDialogue.GetComponent<DialogueTextPackage>();
        IEnumerator blitter = dialogue.Initialize(content, "", false);
        CheckAndEaseOutEarliestDialogue();
        yield return blitter;
    }

    IEnumerator EaseInDialogueBox(RectTransform rect) {
        float timer = 0f;
        float duration = 0.1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float width = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0f, 600f, duration);
            rect.sizeDelta = new Vector2(width, 200f);
            yield return null;
        }
        rect.sizeDelta = new Vector2(600f, 200f);

    }

    public IEnumerator PulseDoubterColor() {
        doubterText.enabled = true;
        float timer = 0f;
        Color color = red;
        int pulses = 0;
        while (pulses < 3) {
            timer += Time.unscaledDeltaTime;
            float factor = (float)PennerDoubleAnimation.CircEaseIn(timer, 1f, -1f, 1f);
            doubterText.color = new Color(red.r, red.g, red.b, factor);
            if (timer > 1f) {
                pulses += 1;
                timer -= 1f;
            }
            yield return null;
        }
        doubterText.color = red;
    }
}
