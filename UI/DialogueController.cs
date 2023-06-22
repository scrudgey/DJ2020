using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DialogueController : MonoBehaviour {
    public enum DialogueResult { success, fail, stun }
    DialogueResult dialogueResult;
    static public Action<DialogueResult> OnDialogueConclude;
    public float currentThreshold;
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
    [Header("audio")]
    public AudioSource audioSource;
    public AudioClip[] nextDialogueSound;
    public AudioClip[] openSkillCheckSound;
    DialogueInput input;
    Stack<SuspicionRecord> unresolvedSuspicionRecords;
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
        unresolvedSuspicionRecords = new Stack<SuspicionRecord>(input.suspicionRecords.Values);
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
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        this.input = input;
        SetStatusContainers(input);
        SetPortraits(input);

        ClearDialogueContainer();
        SuspicionRecord identityChallenge = SuspicionRecord.identitySuspicion(input);
        StartNextChallenge(manualSuspicionRecord: identityChallenge);

        yield return null;
    }

    void StartNextChallenge(SuspicionRecord manualSuspicionRecord = null) {
        ClearResponseContainer();
        SuspicionRecord nextRecord = manualSuspicionRecord == null ? unresolvedSuspicionRecords.Pop() : manualSuspicionRecord;
        SetNPCChallenge(input, nextRecord);
        SetDialogueResponses(input, nextRecord);
    }
    public void SetPortraits(DialogueInput input) {
        leftPortrait.sprite = input.NPCAI.portrait;
        rightPortrait.sprite = input.playerState.portrait;
    }
    public void SetStatusContainers(DialogueInput input) {
        ClearStatusContainers();

        // player side
        foreach (KeyValuePair<String, SuspicionRecord> kvp in input.suspicionRecords) {
            SuspicionRecord record = kvp.Value;
            CreateStatusElement(record.content, (int)record.suspiciousness, false);
        }
        CreateStatusElement($"speech skill {input.playerSpeechSkill}", -1 * input.playerSpeechSkill, false);
        foreach (SpeechEtiquette etiquette in input.playerState.etiquettes) {
            CreateStatusElement($"etiquette: {etiquette}", 0, false);
        }

        // npc side
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
        foreach (SpeechEtiquette etiquette in input.NPCAI.etiquettes) {
            CreateStatusElement($"etiquette: {etiquette}", 0, true);
        }
        SetAppearance(input);
    }

    void SetNPCChallenge(DialogueInput input, SuspicionRecord record) {
        // SetLeftDialogueText("You there, stop! You're not authorized to be in this area! Show me your identification!");
        SetLeftDialogueText(record.dialogue.challenge);
    }
    void SetDialogueResponses(DialogueInput input, SuspicionRecord record) {
        StartCoroutine(CreateMultipleResponses(record.dialogue.tactics));
    }
    IEnumerator CreateMultipleResponses(List<DialogueTactic> tactics) {
        float interval = 0.1f;
        foreach (DialogueTactic tactic in tactics) {
            CreateResponse(tactic);
            yield return new WaitForSecondsRealtime(interval);
        }
        CreateDialogueResponse("<color=#ff4757>[ESCAPE]</color> Excuse me, I think I left my identification in my car.", EscapeDialogueResponseCallback);
    }
    void CreateResponse(DialogueTactic tactic) {
        string colorString = tactic.tacticType switch {
            DialogueTacticType.bluff or DialogueTacticType.lie or DialogueTacticType.redirect => "<color=#ffa502>",
            DialogueTacticType.challenge or DialogueTacticType.escape or DialogueTacticType.deny => "<color=#ff4757>",
            DialogueTacticType.item => "<color=#2ed573>",
            _ => "<color=#ffa502>"
        };
        string prefixString = tactic.tacticType switch {
            DialogueTacticType.bluff => "[BLUFF]",
            DialogueTacticType.challenge => "[CHALLENGE]",
            DialogueTacticType.deny => "[DENY]",
            DialogueTacticType.escape => "[ESCAPE]",
            DialogueTacticType.item => "[ITEM]",
            DialogueTacticType.lie => "[LIE]",
            DialogueTacticType.redirect => "[REDIRECT]",
            _ => ""
        };
        string content = $"{colorString}{prefixString}</color> {tactic.content}";
        Action<DialogueResponseButton> callback = tactic.tacticType switch {
            DialogueTacticType.escape => EscapeDialogueResponseCallback,
            _ => (DialogueResponseButton dialogueResponseButton) => TacticResponseCallback(dialogueResponseButton, tactic)
        };
        CreateDialogueResponse(content, callback);
    }

    public void CreateDialogueResponse(string response, Action<DialogueResponseButton> responseCallback) {
        GameObject responseObj = GameObject.Instantiate(responsePrefab);
        responseObj.transform.SetParent(responsesContainer, false);
        DialogueResponseButton button = responseObj.GetComponent<DialogueResponseButton>();
        button.Initialize(responseCallback, "", response, 0f);
    }
    public void EscapeDialogueResponseCallback(DialogueResponseButton dialogueResponseButton) {
        dialogueResult = DialogueResult.stun;
        EndDialogueResponseCallback(dialogueResponseButton);
    }
    public void TacticResponseCallback(DialogueResponseButton dialogueResponseButton, DialogueTactic tactic) {
        SetRightDialogueText(dialogueResponseButton.response);
        ClearResponseContainer();

        string resultString = tactic.tacticType switch {
            DialogueTacticType.bluff => "bluff called",
            DialogueTacticType.challenge => "tried to intimidate a guard",
            DialogueTacticType.deny => "obvious denial of facts",
            // DialogueTacticType.escape => "[ESCAPE]",
            DialogueTacticType.item => "used counterfeit credentials",
            DialogueTacticType.lie => "caught in a lie",
            DialogueTacticType.redirect => "shadiness",
            _ => "general awkwardness"
        };

        // TODO: responses driven by the specific check
        var input = new SkillCheckDialogue.SkillCheckInput {
            checkType = tactic.tacticType.ToString(),
            successResponse = tactic.successResponse,
            failResponse = tactic.failResponse,
            suspicion = resultString,
            threshold = currentThreshold
        };

        Action<DialogueResponseButton> callback = (DialogueResponseButton button) => {
            if (tactic.tacticType == DialogueTacticType.item) {
                ByPassSkillCheck(input);
            } else {
                ActivateSkillCheck(input);
            }
        };

        CreateDialogueResponse("[CONTINUE]", callback);
    }

    public void EndDialogueResponseCallback(DialogueResponseButton dialogueResponseButton) {
        SuspicionRecord record = SuspicionRecord.fledSuspicion();
        GameManager.I.AddSuspicionRecord(record);
        SetRightDialogueText(dialogueResponseButton.response);
        ClearResponseContainer();
        CreateDialogueResponse("[CONTINUE]", DialogueEndCallback);
    }
    public void DialogueEndCallback(DialogueResponseButton dialogueResponseButton) {
        Conclude();
    }
    public void DialogueNextChallengeCallback(DialogueResponseButton dialogueResponseButton) {
        // Conclude();
        StartNextChallenge();
    }
    public void SetLeftDialogueText(string content) {
        Toolbox.RandomizeOneShot(audioSource, nextDialogueSound, randomPitchWidth: 0.05f);
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
        Toolbox.RandomizeOneShot(audioSource, nextDialogueSound, randomPitchWidth: 0.05f);
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
        OnDialogueConclude?.Invoke(dialogueResult);
        GameManager.I.CloseMenu();
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
        Toolbox.RandomizeOneShot(audioSource, openSkillCheckSound, randomPitchWidth: 0.05f);
        skillCheckDialogue.gameObject.SetActive(true);
        responsesContainer.gameObject.SetActive(false);
        skillCheckDialogue.Initialize(HandleSkillCheckResult, input);
        StartCoroutine(PulseDoubterColor());
    }
    public void ByPassSkillCheck(SkillCheckDialogue.SkillCheckInput input) {
        HandleSkillCheckResult(new SkillCheckDialogue.SkillCheckResult {
            type = SkillCheckDialogue.SkillCheckResult.ResultType.success,
            input = input,
            advanceThreshold = false
        });
    }
    void HandleSkillCheckResult(SkillCheckDialogue.SkillCheckResult result) {
        skillCheckDialogue.gameObject.SetActive(false);
        responsesContainer.gameObject.SetActive(true);
        Debug.Log(result.type);
        doubterText.enabled = false;
        switch (result.type) {
            case SkillCheckDialogue.SkillCheckResult.ResultType.fail:
                SetLeftDialogueText($"<color=#ff4757>[FAIL]</color> {result.input.failResponse}");
                SuspicionRecord lieFailedRecord = new SuspicionRecord {
                    content = result.input.suspicion,
                    suspiciousness = Suspiciousness.aggressive,
                    lifetime = 120f,
                    maxLifetime = 120f
                };
                GameManager.I.AddSuspicionRecord(lieFailedRecord);
                dialogueResult = DialogueResult.fail;
                break;
            case SkillCheckDialogue.SkillCheckResult.ResultType.success:
                SetLeftDialogueText($"<color=#2ed573>[SUCCESS]</color> {result.input.successResponse}");
                dialogueResult = DialogueResult.success;
                break;
        }
        ClearResponseContainer();

        if (result.advanceThreshold) {
            currentThreshold = (100 + 2f * currentThreshold) / 3f;
        }
        if (dialogueResult == DialogueResult.fail) {
            CreateDialogueResponse("[CONTINUE]", DialogueEndCallback);
        } else if (unresolvedSuspicionRecords.Count > 0) {
            CreateDialogueResponse("[CONTINUE]", DialogueNextChallengeCallback);
        } else {
            CreateDialogueResponse("[CONTINUE]", DialogueEndCallback);
        }
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
