using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootShopController : MonoBehaviour {

    public GameObject UIEditorCamera;
    public GameObject lootButtonPrefab;
    // public GameObject bodyContainer;
    public GameObject lootPreferencePrefab;

    [Header("lists")]
    public Transform inventoryContainer;
    public TextMeshProUGUI nothingToSellText;
    [Header("lootpanel")]
    public Image itemImage;
    public Image lootType;
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

    [Header("dialogue")]
    public Image leftImage;
    public Image rightImage;
    public TextMeshProUGUI dialogueText;
    public LayoutElement dialogueLeftSpacer;
    public LayoutElement dialogueRightSpacer;
    public TextMeshProUGUI leftDialogueName;
    public TextMeshProUGUI rightDialogueName;
    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip[] sellSound;
    public AudioClip[] selectSound;
    public AudioClip blitSound;
    Coroutine blitTextRoutine;

    LootData currentItemForSale;
    int forSaleTotalPrice;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Start() {
        rightDialogueName.text = GameManager.I.gameData.filename;
        leftDialogueName.text = "seller";
        // ClearInitialize();
    }

    public void Initialize(LootBuyerData lootBuyerData) {
        PopulatePlayerInventory();
        SetPlayerCredits();
        ClearItemForSale();
        ShowSellerData(lootBuyerData);

        // set portraits
        SetShopownerDialogue("Please come in to my underground loot shop.");
    }
    public void DoneButtonCallback() {
        GameManager.I.HideShopMenu();
    }
    void SetPlayerCredits() {
        playerCreditsText.text = GameManager.I.gameData.playerState.credits.ToString();
    }
    void SetSaleData(LootData data, int count) {
        valueText.text = data.value.ToString();

        itemImage.enabled = true;
        itemImage.sprite = data.portrait;
        lootNameCaption.text = data.lootName;
        lootTypeIcon.SetLootCategory(data.category);
        lootTypeIcon.Show();
        foreach (Image image in creditsImages) {
            image.enabled = true;
        }
        float bonus = 1.4f;
        forSaleTotalPrice = (int)(data.value * count * bonus);
        totalText.text = $"{forSaleTotalPrice}";
        factorText.text = $"{bonus}x";
        countText.text = $"{count}";
    }
    void ShowSellerData(LootBuyerData data) {
        foreach (Transform child in buyerPreferencesContainer) {
            Destroy(child.gameObject);
        }
        buyerNameText.text = data.buyerName;
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
    void PopulatePlayerInventory() {
        foreach (Transform child in inventoryContainer) {
            if (child == nothingToSellText.transform) continue;
            Destroy(child.gameObject);
        }
        nothingToSellText.enabled = GameManager.I.gameData.playerState.loots.Count == 0;
        foreach (LootData data in GameManager.I.gameData.playerState.loots) {
            GameObject button = CreateLootButton(data);
            LootInventoryButton script = button.GetComponent<LootInventoryButton>();
            script.Initialize(this, data, 1);
            button.transform.SetParent(inventoryContainer, false);
        }
    }
    void BlitDialogue(string content) {
        if (blitTextRoutine != null) {
            StopCoroutine(blitTextRoutine);
        }
        blitTextRoutine = StartCoroutine(BlitDialogueText(content));
    }
    void SetShopownerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 20f;
        dialogueRightSpacer.minWidth = 150f;
        // dialogueText.text = dialogue;
        BlitDialogue(dialogue);
    }
    void SetPlayerDialogue(string dialogue) {
        dialogueLeftSpacer.minWidth = 150f;
        dialogueRightSpacer.minWidth = 20f;
        // dialogueText.text = dialogue;
        BlitDialogue(dialogue);

    }
    GameObject CreateLootButton(LootData data) {
        GameObject obj = GameObject.Instantiate(lootButtonPrefab);
        // ItemShopButton button = obj.GetComponent<ItemShopButton>();
        // button.Initialize(data, ShopButtonCallback);
        return obj;
    }
    public void LootButtonCallback(LootData data, int count) {
        // bodyContainer.SetActive(true);
        // SetItemForSale(button.saleData);
        SetItemForSale(data, count);
    }
    void SetItemForSale(LootData data, int count) {
        SetSaleData(data, count);
        SetShopownerDialogue(data.lootDescription);
        currentItemForSale = data;
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
    void SellItem(LootData data) {
        GameManager.I.gameData.playerState.loots.Remove(data);
        GameManager.I.gameData.playerState.credits += forSaleTotalPrice;
        PopulatePlayerInventory();
        ClearItemForSale();
        SetPlayerCredits();
        SetShopownerDialogue("I'll take it.");
    }


    public IEnumerator BlitDialogueText(string content) {
        dialogueText.text = "";
        int index = 0;
        float timer = 0f;
        float blitInterval = 0.025f;
        audioSource.clip = blitSound;
        audioSource.Play();
        while (timer < blitInterval && index < content.Length) {
            timer += Time.unscaledDeltaTime;
            if (timer >= blitInterval) {
                index += 1;
                timer -= blitInterval;
                dialogueText.text = content.Substring(0, index);
            }
            yield return null;
        }
        audioSource.Stop();
        dialogueText.text = content;
        blitTextRoutine = null;
    }
}
