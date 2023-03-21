using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarShopController : MonoBehaviour {
    public AudioSource audioSource;
    public GameObject UIEditorCamera;
    public GameObject marketReportPrefab;
    public Transform marketReportContainer;
    public StoreDialogueController dialogueController;
    public MarketData marketData;
    public AudioClip[] askSound;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }

    public void Initialize() {
        foreach (Transform child in marketReportContainer) {
            Destroy(child.gameObject);
        }
        dialogueController.SetShopownerDialogue("come in to my underground black market bar.");
        marketData = GameManager.I.gameData.marketData;
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
        GameManager.I.HideShopMenu();
    }
}
