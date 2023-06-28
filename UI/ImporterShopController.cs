using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
public class ImporterShopController : MonoBehaviour {
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
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }

    public void Initialize() {
        dealDialogueObject.SetActive(false);
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        ClearDealButtons();
        ClearPlayerInventoryDisplay();
        PopulateDealButtons();
        PopulatePlayerInventory();
        if (GameManager.I.gameData.dealData.Count == 0) {
            storeDialogueController.SetShopownerDialogue("I've got nothing cooking today, decker. Check back with me tomorrow.");
        } else {
            storeDialogueController.SetShopownerDialogue("Help me out, decker. I've gotta move this shipment from singapore.");
        }
    }

    void ClearDealButtons() {
        foreach (Transform child in dealsButtonContainer) {
            Destroy(child.gameObject);
        }
    }
    void ClearPlayerInventoryDisplay() {
        foreach (Transform child in playerInventoryContainer) {
            if (child.name == "creds") continue;
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
        // LootData rushData = Resources.Load("data/loot/rush") as LootData;
        // DealData deal = DealData.FromLootData(rushData, 10, LootCategory.drug, 3);
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
        GameManager.I.HideShopMenu();
    }

    void SetStoreDialogueAccept() {
        storeDialogueController.SetShopownerDialogue("Good deal.");
    }
}
