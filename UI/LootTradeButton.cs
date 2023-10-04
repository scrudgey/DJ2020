using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootTradeButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI lootName;
    public TextMeshProUGUI playerCount;
    public TextMeshProUGUI tradeCountText;
    public TextMeshProUGUI lootValueText;
    public DealDialogueController dealDialogueController;
    public AudioSource audioSource;
    private List<LootData> lootDatas;
    private int tradeCount;
    private int totalCount;
    private DealData dealData;
    [Header("sound effects")]
    public AudioClip[] clickButtonSound;
    public void Initialize(DealDialogueController dealDialogueController, List<LootData> lootDatas, DealData dealData, AudioSource audioSource) {
        this.audioSource = audioSource;
        this.dealDialogueController = dealDialogueController;
        this.lootDatas = lootDatas;
        this.dealData = dealData;
        if (lootDatas == null || lootDatas.Count == 0) return;
        LootData lootData = lootDatas[0];
        icon.sprite = lootData.portrait;
        lootName.text = lootData.lootName;
        lootValueText.text = $"{lootData.GetValue()}";
        tradeCount = 0;
        totalCount = lootDatas.Count;
        SetCountText();
    }
    public int LootValue() {
        if (lootDatas == null || lootDatas.Count == 0) return 0;
        LootData lootData = lootDatas[0];
        return lootData.GetValue();
    }
    void SetCountText() {
        playerCount.text = $"{totalCount - tradeCount}";
        tradeCountText.text = $"{tradeCount}";
    }

    public void OnPlusClick() {
        if (tradeCount < totalCount && tradeCount < dealData.priceCount) {
            tradeCount++;
            SetCountText();
            dealDialogueController.LootTradeButtonCountCallback(this);
        }
        Toolbox.RandomizeOneShot(audioSource, clickButtonSound);
    }
    public void OnMinusClick() {
        if (tradeCount > 0) {
            tradeCount--;
            SetCountText();
            dealDialogueController.LootTradeButtonCountCallback(this);
        }
        Toolbox.RandomizeOneShot(audioSource, clickButtonSound);
    }
    public void UndoPlusClick() {
        tradeCount--;
        SetCountText();
    }

    public List<LootData> GetLootDataForOffer() {
        return lootDatas.GetRange(0, tradeCount);
    }
}
