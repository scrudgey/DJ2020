using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanTacticsController : MonoBehaviour {

    [Header("audio")]
    public AudioClip[] purchaseSound;
    public AudioClip[] purchaseFailSound;
    public AudioClip[] entryClickSound;
    public AudioClip[] dialogueButtonClicked;
    public AudioClip[] openDialogueSound;
    public AudioClip[] staticSound;
    public AudioSource audioSource;

    [Header("prefabs")]
    public GameObject availbleEntryPrefab;
    public GameObject activeEntryPrefab;

    [Header("references")]
    public Transform availableEntriesContainer;
    public TextMeshProUGUI creditsText;
    public TextMeshProUGUI dialogueTacticName;
    public TextMeshProUGUI dialogueTacticsCost;

    [Header("dialogue")]
    public GameObject dialogueContainer;
    public GameObject tacticStatusContainer;
    public RectTransform dialogueRectTransform;
    public StoreDialogueController storeDialogueController;
    public Transform dialogueResponseContainer;
    public GameObject dialogueResponseButtonPrefab;
    public RectTransform leftPortraitRectTransform;
    public RectTransform rightPortraitRectTransform;
    public GameObject staticObject;
    bool dialogueOpen;
    Coroutine dialogueCoroutine;

    GameData data;
    PurchaseTacticEntry activeEntry;
    LevelPlan plan;
    LevelTemplate template;
    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.template = template;
        this.plan = plan;
        SetupPurchases(template, plan);
        SetupActiveTactics(plan);
        SetCreditsText();
        dialogueOpen = false;
        dialogueContainer.SetActive(false);
    }

    void SetupPurchases(LevelTemplate template, LevelPlan plan) {
        foreach (Transform child in availableEntriesContainer) {
            if (child.gameObject.name.ToLower().Contains("header")) continue;
            Destroy(child.gameObject);
        }
        List<string> activeTacticNames = plan.activeTactics.Select(tactic => tactic.title).ToList();

        foreach (Tactic tactic in template.availableTactics) {
            // if (activeTacticNames.Contains(tactic.title)) continue;
            GameObject obj = Instantiate(availbleEntryPrefab);
            obj.transform.SetParent(availableEntriesContainer, false);
            PurchaseTacticEntry purchaseTacticEntry = obj.GetComponent<PurchaseTacticEntry>();
            PurchaseTacticEntry.Status status = activeTacticNames.Contains(tactic.title) ? PurchaseTacticEntry.Status.purchased : PurchaseTacticEntry.Status.forSale;
            purchaseTacticEntry.Initialize(this, tactic, status);
            // if (activeEntry == null) {
            //     AvailableEntryCallback(purchaseTacticEntry);
            // }
        }
    }
    void SetupActiveTactics(LevelPlan plan) {
        // foreach (Transform child in activeEntriesContainer) {
        //     if (child.gameObject.name.ToLower().Contains("title") ||
        //         child.gameObject.name.ToLower().Contains("divider") ||
        //         child.gameObject.name.ToLower().Contains("spacer") ||
        //         child.gameObject.name.ToLower().Contains("credits")) continue;
        //     Destroy(child.gameObject);
        // }
        foreach (Tactic tactic in plan.activeTactics) {
            GameObject obj = Instantiate(activeEntryPrefab);
            // obj.transform.SetParent(activeEntriesContainer, false);
            ActiveTacticView activeTacticView = obj.GetComponent<ActiveTacticView>();
            activeTacticView.Initialize(tactic);
        }
    }

    public void AvailableEntryCallback(PurchaseTacticEntry entry) {
        Toolbox.RandomizeOneShot(audioSource, openDialogueSound);
        activeEntry = entry;
        ShowDialogue(entry);
        // ConfigurePurchaseDialogue(entry.tactic);
    }
    void SetCreditsText() {
        creditsText.text = $"{data.playerState.credits}";
    }

    void HandleBuyResponse(DialogueResponseButton button) {
        PurchaseCallback();
        HideDialogue();
    }
    void HandleCancelResponse(DialogueResponseButton button) {
        HideDialogue();
    }

    public void CreateDialogueResponse(string prefix, string response, Action<DialogueResponseButton> responseCallback) {
        DialogueResponseButton button = CreateDialogueResponseButton();
        button.Initialize(responseCallback, prefix, response, 0f);
        button.audioSource = audioSource;
        button.clickSound = dialogueButtonClicked;
    }

    public void PurchaseCallback() {
        if (activeEntry == null) {
            PurchaseFail();
            return;
        }
        if (activeEntry.tactic.cost > data.playerState.credits) {
            PurchaseFail();
        } else {
            PurchaseSuccess(activeEntry.tactic);
        }
    }

    void PurchaseSuccess(Tactic tactic) {
        Toolbox.RandomizeOneShot(audioSource, purchaseSound);
        data.playerState.credits -= tactic.cost;
        plan.activeTactics.Add(tactic);
        SetupPurchases(template, plan);
        SetupActiveTactics(plan);
        SetCreditsText();
    }
    void PurchaseFail() {
        Toolbox.RandomizeOneShot(audioSource, purchaseFailSound);
    }

    void ShowDialogue(PurchaseTacticEntry entry) {
        if (dialogueOpen) return;
        if (dialogueCoroutine != null) {
            StopCoroutine(dialogueCoroutine);
        }
        dialogueOpen = true;
        tacticStatusContainer.SetActive(false);
        dialogueContainer.SetActive(true);

        InitializeDialogue(entry);

        dialogueRectTransform.sizeDelta = new Vector2(1290f, 35f);

        dialogueCoroutine = StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.55f, 35f, 331f, PennerDoubleAnimation.QuintEaseOut, (float height) => {
                dialogueRectTransform.sizeDelta = new Vector2(1290f, height);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => Toolbox.RandomizeOneShot(audioSource, staticSound)),
            Toolbox.Ease(null, 0.75f, 1f, 220f, PennerDoubleAnimation.QuintEaseOut, (float height) => {
                leftPortraitRectTransform.sizeDelta = new Vector2(220f, height);
            }, unscaledTime: true),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => HideStatic(entry.tactic)),
            new WaitForSecondsRealtime(0.5f),
            Toolbox.CoroutineFunc(() => ShowIntroDialogue(entry.tactic)),
            // new WaitForSecondsRealtime(1f),
            Toolbox.Ease(null, 0.75f, 331f, 680f, PennerDoubleAnimation.ExpoEaseIn, (float height) => {
                dialogueRectTransform.sizeDelta = new Vector2(1290f, height);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => dialogueCoroutine = null)
        ));
    }

    void InitializeDialogue(PurchaseTacticEntry entry) {
        staticObject.SetActive(true);
        ClearDialogueResponseContainer();
        storeDialogueController.MoveDialogueBox(true);
        storeDialogueController.Clear();
        storeDialogueController.Initialize(GameManager.I.gameData.filename, entry.tactic.vendorName);
        storeDialogueController.SetImages(null);
        leftPortraitRectTransform.sizeDelta = new Vector2(220f, 1f);
    }
    void HideStatic(Tactic tactic) {
        storeDialogueController.SetImages(tactic.vendorSprite);
        staticObject.SetActive(false);
    }
    void ShowTacticInformationInDialogue(Tactic tactic) {
        dialogueTacticName.text = tactic.title;
        dialogueTacticsCost.text = $"Asking price: {tactic.cost}";
        tacticStatusContainer.SetActive(true);
    }
    void ShowPurchaseDialogue(Tactic tactic) {
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.CoroutineFunc(() => {
                storeDialogueController.SetShopownerDialogue(tactic.vendorPitch);
            }),
            new WaitForSecondsRealtime(1f),
            Toolbox.CoroutineFunc(() => {
                ShowTacticInformationInDialogue(tactic);
                CreateDialogueResponse("[BUY]", "deal.", DecorateCallback(HandleBuyResponse, prefix: "[END]"));
                CreateDialogueResponse("[CANCEL]", "no deal.", DecorateCallback(HandleCancelResponse, prefix: "[END]"));
            })
        ));
    }
    void ShowIntroDialogue(Tactic tactic) {
        storeDialogueController.SetPlayerDialogue($"Whattaya got for me, {tactic.vendorName}?");
        CreateDialogueResponse("[CONTINUE]", "", (DialogueResponseButton button) => {
            ShowPurchaseDialogue(tactic);
            ClearDialogueResponseContainer();
        });
    }
    Action<DialogueResponseButton> DecorateCallback(Action<DialogueResponseButton> callback, string prefix = "[CONTINUE]") {
        return (DialogueResponseButton button) => {
            storeDialogueController.SetPlayerDialogue(button.response);
            ClearDialogueResponseContainer();
            CreateDialogueResponse(prefix, "", callback);
        };
    }
    void HideDialogue() {
        if (!dialogueOpen) return;
        if (dialogueCoroutine != null) {
            StopCoroutine(dialogueCoroutine);
        }
        dialogueOpen = false;
        dialogueCoroutine = StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.5f, 680f, 35, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                dialogueRectTransform.sizeDelta = new Vector2(1290f, height);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                dialogueContainer.SetActive(false);
                dialogueCoroutine = null;
            })
        ));
        audioSource.Stop();
    }

    public void CloseDialogueCallback() {
        HideDialogue();
    }

    void ClearDialogueResponseContainer() {
        foreach (Transform child in dialogueResponseContainer) {
            if (child.name == "bkg") continue;
            Destroy(child.gameObject);
        }
    }

    DialogueResponseButton CreateDialogueResponseButton() {
        GameObject obj = GameObject.Instantiate(dialogueResponseButtonPrefab);
        obj.transform.SetParent(dialogueResponseContainer, false);
        DialogueResponseButton script = obj.GetComponent<DialogueResponseButton>();
        return script;
    }
}
