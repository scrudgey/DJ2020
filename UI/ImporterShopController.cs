using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Nimrod;
using TMPro;
using UnityEngine;
public class ImporterShopController : MonoBehaviour {
    public Canvas myCanvas;
    public RectTransform bottomRect;
    public GameObject UIEditorCamera;
    public AudioSource audioSource;
    public StoreDialogueController storeDialogueController;
    public Transform dealsButtonContainer;
    public Transform playerInventoryContainer;
    public GameObject dealButtonPrefab;
    public GameObject lootCounterPrefab;
    public TextMeshProUGUI playerTotalCreditsText;

    [Header("dealDialogue")]
    public GameObject dealDialogueObject;
    public RectTransform dealDialogueRect;
    public DealDialogueController dealDialogueController;
    [Header("sounds")]
    public AudioClip[] showDialogueSounds;
    public AudioClip[] discloseBottomSound;
    public AudioClip[] closeSounds;
    Grammar grammar;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
    }

    public void Initialize() {

        grammar = new Grammar();
        grammar.Load("loot");
        string informalIdentifier = "the guy";
        grammar.AddSymbol("informalIdentifier", informalIdentifier);
        grammar.AddSymbol("name", GameManager.I.gameData.filename);

        dealDialogueObject.SetActive(false);
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        ClearDealButtons();
        ClearPlayerInventoryDisplay();
        PopulateDealButtons();
        PopulatePlayerInventory();
        if (GameManager.I.gameData.dealData.Count == 0) {
            storeDialogueController.SetShopownerDialogue("{importer-empty}");
        } else {
            storeDialogueController.SetShopownerDialogue(grammar.Parse("{importer-greet}")
                );
        }
        myCanvas.enabled = true;
    }

    void ClearDealButtons() {
        foreach (Transform child in dealsButtonContainer) {
            Destroy(child.gameObject);
        }
    }
    void ClearPlayerInventoryDisplay() {
        foreach (Transform child in playerInventoryContainer) {
            if (child.name == "CreditBalanceIndicator") continue;
            Destroy(child.gameObject);
        }
    }
    void CreateDealButton(DealData data) {
        GameObject obj = GameObject.Instantiate(dealButtonPrefab);
        obj.transform.SetParent(dealsButtonContainer, false);
        DealButton button = obj.GetComponent<DealButton>();
        button.Initialize(this, data);
    }
    public void OnDealButtonClick(DealButton button) {
        storeDialogueController.SetShopownerDialogue(button.dealData.pitch);
        ShowDealDialogue(button.dealData);
    }
    public void OnDealDialogueCancel() {
        HideDealDialogue();
    }

    void PopulateDealButtons() {
        foreach (DealData deal in GameManager.I.gameData.dealData) {
            CreateDealButton(deal);
        }
    }

    void CreateLootCounter(LootCategory category, int count) {
        GameObject obj = GameObject.Instantiate(lootCounterPrefab);
        obj.transform.SetParent(playerInventoryContainer, false);
        LootCounter counter = obj.GetComponent<LootCounter>();
        counter.Initialize(category, count);
    }
    void PopulatePlayerInventory() {
        playerTotalCreditsText.text = $"{GameManager.I.gameData.playerState.credits}";
        foreach (LootCategory lootCategory in Enum.GetValues(typeof(LootCategory))) {
            int playerCount = GameManager.I.gameData.playerState.loots.Where(loot => loot.category == lootCategory).Count();
            CreateLootCounter(lootCategory, playerCount);
        }
    }


    void ShowDealDialogue(DealData data) {
        Toolbox.RandomizeOneShot(audioSource, showDialogueSounds);
        dealDialogueObject.SetActive(true);
        dealDialogueController.Initialize(data);
        StartCoroutine(Toolbox.Ease(null, 0.5f, 40f, 700f, PennerDoubleAnimation.ExpoEaseIn,
        (height) => {
            dealDialogueRect.sizeDelta = new Vector2(1200f, height);
        }, unscaledTime: true
        ));
    }

    void HideDealDialogue() {
        StartCoroutine(Toolbox.ChainCoroutines(
             Toolbox.Ease(null, 0.5f, 700f, 40f, PennerDoubleAnimation.ExpoEaseIn,
                (height) => {
                    dealDialogueRect.sizeDelta = new Vector2(1200f, height);
                }, unscaledTime: true
                ),
                Toolbox.CoroutineFunc(() => dealDialogueObject.SetActive(false))
                )
        );
    }

    public void AcceptDeal(DealData dealData, List<LootData> lootForPrice) {
        GameManager.I.gameData.dealData.Remove(dealData);
        GameManager.I.gameData.playerState.loots.RemoveAll(loot => lootForPrice.Contains(loot));
        for (int i = 0; i < dealData.offerCount; i++) {
            LootData newLoot = ScriptableObject.Instantiate(dealData.offerLoot);
            newLoot.name = Toolbox.CloneRemover(newLoot.name);
            GameManager.I.gameData.playerState.loots.Add(ScriptableObject.Instantiate(dealData.offerLoot));
        }
        HideDealDialogue();

        ClearDealButtons();
        ClearPlayerInventoryDisplay();
        PopulateDealButtons();
        PopulatePlayerInventory();

        SetStoreDialogueAccept();
    }

    public void DoneButtonCallback() {
        GameManager.I.PlayUISound(closeSounds);
        StartCoroutine(Toolbox.CloseMenu(bottomRect));
    }

    void SetStoreDialogueAccept() {
        storeDialogueController.SetShopownerDialogue("Good deal.");
    }
}
