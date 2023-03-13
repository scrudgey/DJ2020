using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootShopController : MonoBehaviour {

    public GameObject UIEditorCamera;
    public GameObject lootButtonPrefab;
    // public GameObject bodyContainer;
    public GameObject lootPreferencePrefab;
    public StoreDialogueController dialogueController;

    [Header("lists")]
    public Transform inventoryContainer;
    public TextMeshProUGUI nothingToSellText;
    [Header("lootpanel")]
    public Image itemImage;
    public LootTypeIcon lootTypeIcon;
    public TextMeshProUGUI lootNameCaption;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI factorText;
    public TextMeshProUGUI totalText;
    public TextMeshProUGUI playerCreditsText;
    public Image[] creditsImages;
    [Header("buyer")]
    public TextMeshProUGUI buyerNameText;
    public TextMeshProUGUI buyerDescriptionText;
    public Transform buyerPreferencesContainer;

    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip[] sellSound;
    public AudioClip[] selectSound;


    List<LootData> currentItemForSale;
    int forSaleTotalPrice;
    LootBuyerData lootBuyerData;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }

    public void Initialize(LootBuyerData lootBuyerData) {
        this.lootBuyerData = lootBuyerData;
        PopulatePlayerInventory();
        SetPlayerCredits();
        ClearItemForSale();
        ShowSellerData(lootBuyerData);
        dialogueController.Initialize(GameManager.I.gameData.filename, lootBuyerData.buyerName);
        // set portraits
        dialogueController.SetShopownerDialogue("Please come in to my underground loot shop.");
    }
    void PopulatePlayerInventory() {
        foreach (Transform child in inventoryContainer) {
            if (child == nothingToSellText.transform) continue;
            Destroy(child.gameObject);
        }
        nothingToSellText.enabled = GameManager.I.gameData.playerState.loots.Count == 0;

        foreach (IGrouping<string, LootData> grouping in GameManager.I.gameData.playerState.loots.GroupBy(lootData => lootData.lootName)) {
            int count = grouping.Count();
            LootData data = grouping.First();
            GameObject button = CreateLootButton(data);
            LootInventoryButton script = button.GetComponent<LootInventoryButton>();
            script.Initialize(this, grouping.ToList(), count);
            Debug.Log($"group: {data.name} {count}");
            button.transform.SetParent(inventoryContainer, false);
        }
    }
    public void DoneButtonCallback() {
        GameManager.I.HideShopMenu();
    }
    void SetPlayerCredits() {
        playerCreditsText.text = GameManager.I.gameData.playerState.credits.ToString();
    }
    void SetSaleData(List<LootData> datas) {
        LootData data = datas[0];
        valueText.text = data.value.ToString();

        itemImage.enabled = true;
        itemImage.sprite = data.portrait;
        lootNameCaption.text = data.lootName;
        lootTypeIcon.SetLootCategory(data.category);
        lootTypeIcon.Show();
        foreach (Image image in creditsImages) {
            image.enabled = true;
        }
        float bonus = lootBuyerData.CalculateBonusFactor(data);
        int count = datas.Count();
        forSaleTotalPrice = (int)(data.value * count * bonus);
        totalText.text = $"{forSaleTotalPrice}";
        factorText.text = $"{bonus}x";
        countText.text = $"{count}";
    }
    void ShowSellerData(LootBuyerData data) {
        foreach (Transform child in buyerPreferencesContainer) {
            Destroy(child.gameObject);
        }
        buyerNameText.text = data.buyerName.ToLower();
        buyerDescriptionText.text = data.description;
        foreach (LootPreferenceData preference in data.preferences) {
            GameObject gameObject = GameObject.Instantiate(lootPreferencePrefab);
            LootPreferenceController controller = gameObject.GetComponent<LootPreferenceController>();
            controller.Initialize(preference);
            gameObject.transform.SetParent(buyerPreferencesContainer, false);
        }
    }
    void ClearItemForSale() {
        currentItemForSale = null;
        valueText.text = "-";
        factorText.text = "-";
        countText.text = "-";
        totalText.text = "-";
        lootNameCaption.text = "";
        itemImage.enabled = false;
        foreach (Image image in creditsImages) {
            image.enabled = false;
        }
        forSaleTotalPrice = 0;
        lootTypeIcon.Hide();
    }

    GameObject CreateLootButton(LootData data) {
        GameObject obj = GameObject.Instantiate(lootButtonPrefab);
        // ItemShopButton button = obj.GetComponent<ItemShopButton>();
        // button.Initialize(data, ShopButtonCallback);
        return obj;
    }
    public void LootButtonCallback(List<LootData> data) {
        // bodyContainer.SetActive(true);
        // SetItemForSale(button.saleData);
        SetItemForSale(data);
    }
    void SetItemForSale(List<LootData> datas) {
        SetSaleData(datas);
        LootData data = datas.First();
        dialogueController.SetShopownerDialogue(data.lootDescription);
        currentItemForSale = datas;
        Toolbox.RandomizeOneShot(audioSource, selectSound);
        // SetPlayerDialogue(data.lootDescription);
    }

    public void SellCallback() {
        Debug.Log("sell");
        if (currentItemForSale == null) {
            return;
        } else {
            Toolbox.RandomizeOneShot(audioSource, sellSound);
            SellItem(currentItemForSale);
        }
    }
    void SellItem(List<LootData> datas) {
        foreach (LootData data in datas) {
            GameManager.I.gameData.playerState.loots.Remove(data);
        }
        GameManager.I.gameData.playerState.credits += forSaleTotalPrice;
        PopulatePlayerInventory();
        ClearItemForSale();
        SetPlayerCredits();
        dialogueController.SetShopownerDialogue("I'll take it.");
    }

}
