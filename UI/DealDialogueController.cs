using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DealDialogueController : MonoBehaviour {
    public ImporterShopController importerShopController;
    public AudioSource audioSource;
    [Header("receive")]
    public TextMeshProUGUI receiveCaptionText;
    public TextMeshProUGUI receiveValueText;
    public TextMeshProUGUI receiveCountText;
    public Image receiveIcon;
    [Header("spend")]
    public TextMeshProUGUI spendCaptionText;
    public TextMeshProUGUI spendValueText;
    public GameObject lootTradeButtonPrefab;
    public Transform lootTradeButtonsContainer;
    private List<LootTradeButton> lootTradeButtons;
    [Header("buttons")]
    public Button acceptButton;
    private DealData dealData;
    [Header("sounds")]
    public AudioClip[] acceptSounds;
    public AudioClip[] cancelSounds;
    public void Initialize(DealData data) {
        this.dealData = data;

        acceptButton.interactable = false;

        SetInitialText(data);
        ClearLootTradeButtons();
        PopulateAllLootTradeButtons();
    }
    void SetInitialText(DealData data) {
        receiveCaptionText.text = $"{data.offerCount}x {data.offerName}";
        receiveValueText.text = $"{data.offerCount * data.offerValue}";
        receiveCountText.text = $"{data.offerCount}x";
        receiveIcon.sprite = data.offerIcon;
        SetTally(0, 0);
    }

    void ClearLootTradeButtons() {
        foreach (Transform child in lootTradeButtonsContainer) {
            Destroy(child.gameObject);
        }
    }
    LootTradeButton CreateLootTradeButton() {
        GameObject obj = GameObject.Instantiate(lootTradeButtonPrefab);
        obj.transform.SetParent(lootTradeButtonsContainer, false);
        LootTradeButton button = obj.GetComponent<LootTradeButton>();
        return button;
    }

    void PopulateAllLootTradeButtons() {
        lootTradeButtons = new List<LootTradeButton>();
        foreach (IGrouping<string, LootData> group in GameManager.I.gameData.playerState.loots
            .Where(loot => loot.category == dealData.priceType)
            .GroupBy(loot => loot.lootName).ToList()) {
            List<LootData> lootData = group.ToList();
            LootTradeButton button = CreateLootTradeButton();
            button.Initialize(this, lootData, dealData, audioSource);
            lootTradeButtons.Add(button);
        }
    }

    public void OnCancelCallback() {
        importerShopController.OnDealDialogueCancel();
        Toolbox.RandomizeOneShot(audioSource, cancelSounds);
    }
    public void OnAcceptCallback() {
        acceptButton.interactable = false;
        List<LootData> lootForPrice = lootTradeButtons.SelectMany(button => button.GetLootDataForOffer()).ToList();
        importerShopController.AcceptDeal(dealData, lootForPrice);
        Toolbox.RandomizeOneShot(audioSource, acceptSounds);
    }

    public void LootTradeButtonCountCallback(LootTradeButton button) {
        List<LootData> offeredLootData = button.GetLootDataForOffer();
        (int totalCount, int totalCost) = TallyAllLootForSpend();
        if (totalCount > dealData.priceCount) {
            button.UndoPlusClick();
        } else {
            SetTally(totalCount, totalCost);
        }
    }

    public void SetTally(int totalCount, int totalCost) {
        spendCaptionText.text = $"{dealData.priceType}: {totalCount}/{dealData.priceCount}";
        acceptButton.interactable = totalCount >= dealData.priceCount;
        spendValueText.text = $"{totalCost}";
    }

    (int, int) TallyAllLootForSpend() {
        int totalCount = 0;
        int totalCost = 0;
        foreach (LootTradeButton button in lootTradeButtons) {
            int count = button.GetLootDataForOffer().Count();
            totalCount += count;
            totalCost += count * button.LootValue();
        }
        return (totalCount, totalCost);
    }
}
