using System.Collections;
using System.Collections.Generic;
using Easings;
using Nimrod;
using UnityEngine;

public class BarShopController : MonoBehaviour {
    public Canvas myCanvas;
    public AudioSource audioSource;
    public GameObject UIEditorCamera;

    [Header("tabs")]
    public Transform marketReportContainer;
    public Transform fenceLocationContainer;

    [Header("pieces")]
    public Transform fenceButtonContainer;
    public RectTransform bottomRect;
    public GameObject marketReportPrefab;
    public GameObject fenceLocationButtonPrefab;
    public StoreDialogueController dialogueController;

    [Header("sounds")]
    public AudioClip[] askSound;
    public AudioClip[] discloseBottomSound;
    public AudioClip[] closeSounds;

    Grammar grammar;
    MarketData marketData;
    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }

    public void Initialize() {
        foreach (Transform child in marketReportContainer) {
            Destroy(child.gameObject);
        }
        grammar = new Grammar();
        grammar.Load("bar");
        grammar.SetSymbol("name", GameManager.I.gameData.filename);

        dialogueController.Initialize(GameManager.I.gameData.filename, "Mikey");
        ClearMarketData();
        marketReportContainer.gameObject.SetActive(true);
        fenceLocationContainer.gameObject.SetActive(false);
        dialogueController.SetShopownerDialogue(grammar.Parse("{greet}"));
        marketData = GameManager.I.gameData.marketData;
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        myCanvas.enabled = true;
    }

    void PopulateMarketData() {
        int index = 1;
        foreach (LootPreferenceData lootPreference in marketData.preferences) {
            GameObject obj = GameObject.Instantiate(marketReportPrefab);
            MarketInfoIndicator indicator = obj.GetComponent<MarketInfoIndicator>();
            indicator.Initialize(lootPreference);
            indicator.SetBackgroundColor(index);
            obj.transform.SetParent(marketReportContainer, false);
            index = index == 1 ? 2 : 1;
        }

        dialogueController.SetShopownerDialogue(marketData.description);
    }

    void PopulateFenceData() {
        foreach (FenceData fenceData in GameManager.I.gameData.fenceData) {
            GameObject obj = GameObject.Instantiate(fenceLocationButtonPrefab);
            FenceLocationButton button = obj.GetComponent<FenceLocationButton>();
            button.Initialize(fenceData, FenceButtonCallback);
            obj.transform.SetParent(fenceButtonContainer);
        }
        dialogueController.SetShopownerDialogue("Who you lookin for?");
    }
    void ClearMarketData() {
        foreach (Transform child in marketReportContainer) {
            Destroy(child.gameObject);
        }
    }
    void ClearFenceData() {
        foreach (Transform child in fenceButtonContainer) {
            Destroy(child.gameObject);
        }
    }
    void PopulateRumorData() {

    }

    public void MarketInfoCallback() {
        ClearMarketData();
        PopulateMarketData();
        marketReportContainer.gameObject.SetActive(true);
        fenceLocationContainer.gameObject.SetActive(false);
        Toolbox.RandomizeOneShot(audioSource, askSound);
    }

    public void RumorCallback() {
        ClearMarketData();
        PopulateRumorData();
        Toolbox.RandomizeOneShot(audioSource, askSound);
    }
    public void WhereIsCallback() {
        marketReportContainer.gameObject.SetActive(false);
        fenceLocationContainer.gameObject.SetActive(true);
        ClearFenceData();
        PopulateFenceData();
        Toolbox.RandomizeOneShot(audioSource, askSound);
    }

    public void DoneButtonCallback() {
        GameManager.I.PlayUISound(closeSounds);
        StartCoroutine(Toolbox.CloseMenu(bottomRect));
    }

    public void FenceButtonCallback(FenceLocationButton fenceLocationButton) {
        Toolbox.RandomizeOneShot(audioSource, askSound);
        dialogueController.SetShopownerDialogue(fenceLocationButton.data.barLocatorDescription);
    }
}
