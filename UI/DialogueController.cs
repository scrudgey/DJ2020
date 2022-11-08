using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour {
    public enum DialogueResult { success, fail }

    private const string V = "";
    public Action<DialogueResult> OnDialogueConclude;

    public GameObject UIEditorCamera;
    public Transform responsesContainer;
    public Image leftPortrait;
    public Image rightPortrait;
    public TextMeshProUGUI leftPortraitCaption;
    public TextMeshProUGUI rightPortraitCaption;
    public GameObject responsePrefab;

    [Header("Dialogue")]
    public Transform dialogueContainer;
    public GameObject dialoguePrefab;
    public RectTransform dialogueTopPadding;

    [Header("Statuses")]
    public Transform leftStatusContainer;
    public Transform rightStatusContainer;
    public GameObject statusElementPrefab;
    public TextMeshProUGUI appearanceText;

    [Header("Skillcheck")]
    public SkillCheckDialogue skillCheckDialogue;
    public TextMeshProUGUI doubterText;
    public Color red;
    DialogueInput input;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
        ClearResponseContainer();
        ClearDialogueContainer();
    }
    public void Start() {
        // Initialize();
        doubterText.enabled = false;
        skillCheckDialogue.gameObject.SetActive(false);
        responsesContainer.gameObject.SetActive(true);
    }
    void ClearResponseContainer() {
        foreach (Transform child in responsesContainer) {
            Destroy(child.gameObject);
        }
    }
    void ClearDialogueContainer() {
        foreach (Transform child in dialogueContainer) {
            if (child.name.ToLower().Contains("toppadding")) {
                continue;
            }
            Destroy(child.gameObject);
        }
    }
    void ClearStatusContainers() {
        foreach (Transform child in leftStatusContainer) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightStatusContainer) {
            if (child.name.ToLower().Contains("appearance")) {
                continue;

            }
            Destroy(child.gameObject);
        }
    }
    public void Initialize(DialogueInput input) {
        StartCoroutine(DoInitialize(input));
    }
    public IEnumerator DoInitialize(DialogueInput input) {
        CharacterController playerController = input.playerObject.GetComponentInChildren<CharacterController>();
        CharacterController npcController = input.npcObject.GetComponentInChildren<CharacterController>();

        Vector3 playerPosition = input.playerObject.transform.position;
        Vector3 npcPosition = input.npcObject.transform.position;
        // Vector3 npcToPlayer = playerPosition - npcPosition;

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
        npcController.SetInputs(npcInput);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        this.input = input;
        SetStatusContainers(input);
        SetInitialDialogueResponses(input);
        SetInitialNPCDialogue(input);
        SetPortraits(input);
        Time.timeScale = 0f;
        yield return null;
    }
    public void SetPortraits(DialogueInput input) {

    }
    public void SetStatusContainers(DialogueInput input) {
        ClearStatusContainers();
        foreach (KeyValuePair<String, SuspicionRecord> kvp in input.suspicionRecords) {
            SuspicionRecord record = kvp.Value;
            CreateStatusElement(record.content, (int)record.suspiciousness, false);
        }
        CreateStatusElement($"speech skill {input.playerSpeechSkill}", -1 * input.playerSpeechSkill, false);

        if (input.alarmActive) {
            CreateStatusElement("alarm is active", 1, true);
        }
        if (input.playerInDisguise) {
            CreateStatusElement($"in disguise", -1, false);
        }

        switch (input.NPCAI.alertness) {
            case Alertness.normal:
                CreateStatusElement("normal posture", 0, true);
                break;
            case Alertness.alert:
                CreateStatusElement("on alert", 2, true);
                break;
            case Alertness.distracted:
                CreateStatusElement("distracted", -1, true);
                break;
        }

        switch (input.levelState.template.sensitivityLevel) {
            case SensitivityLevel.publicProperty:
                CreateStatusElement("on public property", -1, true);
                break;
            case SensitivityLevel.semiprivateProperty:
            case SensitivityLevel.privateProperty:
                CreateStatusElement("on private property", 1, true);
                break;
            case SensitivityLevel.restrictedProperty:
                CreateStatusElement("in restricted area", 2, true);
                break;
        }
        SetAppearance(input);
    }

    public void SetInitialDialogueResponses(DialogueInput input) {
        ClearResponseContainer();
        // TODO: data driven.
        StartCoroutine(CreateMultipleDialogues());
    }
    public IEnumerator CreateMultipleDialogues() {
        float interval = 0.1f;
        CreateDialogueResponse("[ESCAPE] Excuse me, I think I left my identification in my car.", EscapeDialogueResponseCallback);
        yield return new WaitForSecondsRealtime(interval);
        CreateDialogueResponse("[LIE] I am P.J. Pennypacker, security inspector.", LieDialogueResponseCallback);
        yield return new WaitForSecondsRealtime(interval);
        CreateDialogueResponse("[BLUFF] Rockwell isn't going to be very happy if you delay our meeting!", BluffDialogueResponseCallback);
        yield return new WaitForSecondsRealtime(interval);
        CreateDialogueResponse("[ITEM] Sure, check my ID card.", EndDialogueResponseCallback);
    }
    public void SetInitialNPCDialogue(DialogueInput input) {
        ClearDialogueContainer();
        SetLeftDialogueText("You there, stop! You're not authorized to be in this area! Show me your identification!");
    }



    public void CreateDialogueResponse(string response, Action<DialogueResponseButton> responseCallback) {
        GameObject responseObj = GameObject.Instantiate(responsePrefab);
        responseObj.transform.SetParent(responsesContainer, false);
        DialogueResponseButton button = responseObj.GetComponent<DialogueResponseButton>();
        button.Initialize(responseCallback, response, 0f);
    }
    public void EscapeDialogueResponseCallback(DialogueResponseButton dialogueResponseButton) => EndDialogueResponseCallback(dialogueResponseButton);
    public void LieDialogueResponseCallback(DialogueResponseButton dialogueResponseButton) {
        SetRightDialogueText(dialogueResponseButton.response);
        ClearResponseContainer();
        var input = new SkillCheckDialogue.SkillCheckInput {
            checkType = "Lie",
            successResponse = "Yes, that sounds right. Ok then.",
            failResponse = "I don't think so.",
            suspicion = "identity discovered"
        };
        Action<DialogueResponseButton> callback = (DialogueResponseButton button) => {
            ActivateSkillCheck(input);
        };
        CreateDialogueResponse("[CONTINUE]", callback);
    }
    public void BluffDialogueResponseCallback(DialogueResponseButton dialogueResponseButton) {
        SetRightDialogueText(dialogueResponseButton.response);
        ClearResponseContainer();
        var input = new SkillCheckDialogue.SkillCheckInput {
            checkType = "Bluff",
            successResponse = "I'm sorry, I didn't mean to intrude. Carry on.",
            failResponse = "Rockwell, eh? Let's see what he has to say about it.",
            suspicion = "bluff called"
        };
        Action<DialogueResponseButton> callback = (DialogueResponseButton button) => {
            ActivateSkillCheck(input);
        };
        CreateDialogueResponse("[CONTINUE]", callback);
    }
    public void EndDialogueResponseCallback(DialogueResponseButton dialogueResponseButton) {
        SetRightDialogueText(dialogueResponseButton.response);
        ClearResponseContainer();
        CreateDialogueResponse("[CONTINUE]", DialogueEndCallback);
    }
    public void DialogueEndCallback(DialogueResponseButton dialogueResponseButton) {
        Debug.Log("end");
        Conclude();
    }
    public void SetLeftDialogueText(string content) {
        GameObject newDialogue = GameObject.Instantiate(dialoguePrefab);
        newDialogue.transform.SetParent(dialogueContainer, false);
        DialogueTextPackage dialogue = newDialogue.GetComponent<DialogueTextPackage>();
        dialogue.Initialize(content, true);
        if (dialogueContainer.childCount > 3) {
            Transform earliest = dialogueContainer.GetChild(1);
            DialogueTextPackage targetDialogue = earliest.GetComponent<DialogueTextPackage>();
            targetDialogue.Remove();
        }
    }
    public void SetRightDialogueText(string content) {
        GameObject newDialogue = GameObject.Instantiate(dialoguePrefab);
        newDialogue.transform.SetParent(dialogueContainer, false);
        DialogueTextPackage dialogue = newDialogue.GetComponent<DialogueTextPackage>();
        dialogue.Initialize(content, false);
        if (dialogueContainer.childCount > 3) {
            Transform earliest = dialogueContainer.GetChild(1);
            DialogueTextPackage targetDialogue = earliest.GetComponent<DialogueTextPackage>();
            targetDialogue.Remove();
        }
    }
    void Conclude() {
        OnDialogueConclude?.Invoke(DialogueResult.success);
    }

    // IEnumerator WrapCoroutine(IEnumerator enumerator, Action callback) {
    //     yield return StartCoroutine(enumerator); ;
    //     callback();
    // }

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
    public void CreateStatusElement(string content, int alarmCount, bool left) {
        GameObject statusObj = GameObject.Instantiate(statusElementPrefab);
        if (left) {
            statusObj.transform.SetParent(leftStatusContainer, false);
        } else {
            statusObj.transform.SetParent(rightStatusContainer, false);
        }
        DialogueStatusEntry status = statusObj.GetComponent<DialogueStatusEntry>();
        status.Initialize(alarmCount, content);
    }

    public void SetAppearance(DialogueInput input) {
        appearanceText.text = input.playerSuspiciousness switch {
            Suspiciousness.normal => "appearance ... OK <i>!!<.i>",
            Suspiciousness.suspicious => "appearance ... <size=22><color=#eccc68>Highly Suspicious</color></size>",
            Suspiciousness.aggressive => "appearance ... <size=22><color=#ff4757>Openly Aggressive</color></size>",
            _ => ""
        };
    }


    public void ActivateSkillCheck(SkillCheckDialogue.SkillCheckInput input) {
        skillCheckDialogue.gameObject.SetActive(true);
        responsesContainer.gameObject.SetActive(false);
        skillCheckDialogue.Initialize(HandleSkillCheckResult, input);
        StartCoroutine(PulseDoubterColor());
    }
    void HandleSkillCheckResult(SkillCheckDialogue.SkillCheckResult result) {
        skillCheckDialogue.gameObject.SetActive(false);
        responsesContainer.gameObject.SetActive(true);
        Debug.Log(result.type);
        doubterText.enabled = false;
        switch (result.type) {
            case SkillCheckDialogue.SkillCheckResult.ResultType.fail:
                SetLeftDialogueText($"[FAIL] {result.input.failResponse}");
                SuspicionRecord lieFailedRecord = new SuspicionRecord {
                    content = result.input.suspicion,
                    suspiciousness = Suspiciousness.aggressive,
                    lifetime = 120f,
                    maxLifetime = 120f
                };
                GameManager.I.AddSuspicionRecord(lieFailedRecord);
                break;
            case SkillCheckDialogue.SkillCheckResult.ResultType.success:
                SetLeftDialogueText($"[SUCCESS] {result.input.successResponse}");
                break;
        }
        ClearResponseContainer();
        CreateDialogueResponse("[CONTINUE]", DialogueEndCallback);
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
