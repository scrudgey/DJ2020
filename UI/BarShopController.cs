using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

public class BarShopController : MonoBehaviour {
    public Canvas myCanvas;

    public AudioSource audioSource;
    public GameObject UIEditorCamera;
    public RectTransform bottomRect;
    public GameObject marketReportPrefab;
    public Transform marketReportContainer;
    public StoreDialogueController dialogueController;
    public MarketData marketData;
    [Header("sounds")]
    public AudioClip[] askSound;
    public AudioClip[] discloseBottomSound;
    public AudioClip[] closeSounds;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }

    public void Initialize() {
        foreach (Transform child in marketReportContainer) {
            Destroy(child.gameObject);
        }
        dialogueController.Initialize(GameManager.I.gameData.filename, "Boston");

        dialogueController.SetShopownerDialogue("come in to my underground black market bar.");
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
    void ClearMarketData() {
        foreach (Transform child in marketReportContainer) {
            Destroy(child.gameObject);
        }
    }
    void PopulateRumorData() {

    }

    public void MarketInfoCallback() {
        ClearMarketData();
        PopulateMarketData();
        Toolbox.RandomizeOneShot(audioSource, askSound);
    }

    public void RumorCallback() {
        ClearMarketData();
        PopulateRumorData();
        Toolbox.RandomizeOneShot(audioSource, askSound);
    }

    public void DoneButtonCallback() {
        GameManager.I.PlayUISound(closeSounds);
        StartCoroutine(Toolbox.CloseMenu(bottomRect));
    }
}
