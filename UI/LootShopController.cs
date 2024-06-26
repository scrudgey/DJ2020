using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Items;
using Nimrod;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootShopController : MonoBehaviour {
    public Canvas myCanvas;
    public GameObject UIEditorCamera;
    public GameObject lootButtonPrefab;
    // public GameObject bodyContainer;
    public GameObject lootPreferencePrefab;
    public StoreDialogueController dialogueController;
    public RectTransform bottomRect;


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
    public Image buyerPortrait;
    public GameObject staticObject;
    public RectTransform leftPortraitRectTransform;


    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip[] sellSound;
    public AudioClip[] selectSound;
    public AudioClip[] discloseBottomSound;
    public AudioClip[] staticSound;
    public AudioClip[] openDialogueSound;
    public AudioClip[] closeSounds;
    public GameObject continueButton;
    Action continueCallback;

    Grammar grammar;
    List<LootData> currentItemForSale;
    int forSaleTotalPrice;
    LootBuyerData lootBuyerData;
    bool remoteCall;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1290f, 0f);
    }

    public void Initialize(LootBuyerData lootBuyerData) {
        this.lootBuyerData = lootBuyerData;
        continueButton.SetActive(false);
        grammar = new Grammar();
        grammar.Load("loot");
        grammar.AddSymbol("name", GameManager.I.gameData.filename);
        dialogueController.Clear();

        PopulatePlayerInventory();
        SetPlayerCredits();
        ClearItemForSale();
        ShowSellerData(lootBuyerData);
        dialogueController.Initialize(GameManager.I.gameData.filename, lootBuyerData.buyerName);
        bottomRect.sizeDelta = new Vector2(1290f, 0f);

        remoteCall = lootBuyerData.isRemote;

        if (GameManager.I.gameData.playerState.loots.Count == 0) { // player has no loot
            continueCallback = ContinueFromNoLootPrompt;
            if (lootBuyerData.isRemote) {
                leftPortraitRectTransform.sizeDelta = new Vector2(220f, 1f);
                Toolbox.RandomizeOneShot(audioSource, openDialogueSound);
                StartCoroutine(Toolbox.ChainCoroutines(
                        Toolbox.CoroutineFunc(() => Toolbox.RandomizeOneShot(audioSource, staticSound)),
                        Toolbox.Ease(null, 0.75f, 1f, 220f, PennerDoubleAnimation.QuintEaseOut, (float height) => {
                            leftPortraitRectTransform.sizeDelta = new Vector2(220f, height);
                        }, unscaledTime: true),
                        new WaitForSecondsRealtime(0.5f),
                        Toolbox.CoroutineFunc(() => HideStatic(lootBuyerData)),
                        new WaitForSecondsRealtime(0.5f),
                        Toolbox.CoroutineFunc(() => dialogueController.SetShopownerDialogue(grammar.Parse("{greet-remote-normal}"))),
                        new WaitForSecondsRealtime(1f),
                        Toolbox.CoroutineFunc(() => continueButton.SetActive(true))
                    ));
            } else {
                staticObject.SetActive(false);
                StartCoroutine(Toolbox.ChainCoroutines(
                    Toolbox.CoroutineFunc(() => dialogueController.SetShopownerDialogue(grammar.Parse("{greet-street-normal}"))),
                    new WaitForSecondsRealtime(1f),
                    Toolbox.CoroutineFunc(() => continueButton.SetActive(true))
                ));
            }
        } else { // player has loot
            if (lootBuyerData.isRemote) {
                leftPortraitRectTransform.sizeDelta = new Vector2(220f, 1f);
                Toolbox.RandomizeOneShot(audioSource, openDialogueSound);
                StartCoroutine(Toolbox.ChainCoroutines(
                        Toolbox.CoroutineFunc(() => Toolbox.RandomizeOneShot(audioSource, staticSound)),
                        Toolbox.Ease(null, 0.75f, 1f, 220f, PennerDoubleAnimation.QuintEaseOut, (float height) => {
                            leftPortraitRectTransform.sizeDelta = new Vector2(220f, height);
                        }, unscaledTime: true),
                        new WaitForSecondsRealtime(0.5f),
                        Toolbox.CoroutineFunc(() => HideStatic(lootBuyerData)),
                        new WaitForSecondsRealtime(0.5f),
                        Toolbox.CoroutineFunc(() => dialogueController.SetShopownerDialogue(grammar.Parse("{greet-remote-normal}"))),
                        new WaitForSecondsRealtime(1f),
                        // Toolbox.Ease(null, 0.75f, 331f, 680f, PennerDoubleAnimation.ExpoEaseIn, (float height) => {
                        //     dialogueRectTransform.sizeDelta = new Vector2(1290f, height);
                        // }, unscaledTime: true),
                        Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound)
                    ));
            } else {
                staticObject.SetActive(false);
                dialogueController.SetShopownerDialogue(grammar.Parse("{greet-street-normal}"));
                StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
            }
        }
        myCanvas.enabled = true;
    }

    void ContinueFromNoLootPrompt() {
        continueButton.SetActive(false);
        continueCallback = DoneButtonCallback;
        StartCoroutine(Toolbox.ChainCoroutines(
                                Toolbox.CoroutineFunc(() => dialogueController.SetShopownerDialogue(grammar.Parse("{no-loot}"))),
                                new WaitForSecondsRealtime(1f),
                                Toolbox.CoroutineFunc(() => continueButton.SetActive(true))
                            ));
    }

    public void ContinueClicked() {
        continueCallback?.Invoke();
    }

    void HideStatic(LootBuyerData data) {
        // dialogueController.SetImages(tactic.vendorSprite);
        buyerPortrait.sprite = data.isRemote ? data.phonePortrait : data.portrait;
        staticObject.SetActive(false);
    }

    void PopulatePlayerInventory() {
        foreach (Transform child in inventoryContainer) {
            if (child == nothingToSellText.transform) continue;
            Destroy(child.gameObject);
        }
        nothingToSellText.enabled = GameManager.I.gameData.playerState.loots.Count == 0;

        int numberOfitems = 0;
        foreach (IGrouping<string, LootData> grouping in GameManager.I.gameData.playerState.loots.GroupBy(lootData => lootData.lootName)) {
            int count = grouping.Count();
            LootData data = grouping.First();
            GameObject button = CreateLootButton(data);
            LootInventoryButton script = button.GetComponent<LootInventoryButton>();
            script.Initialize(LootButtonCallback, grouping.ToList(), count);
            Debug.Log($"group: {data.name} {count}");
            button.transform.SetParent(inventoryContainer, false);
            numberOfitems += 1;
        }
    }
    public void DoneButtonCallback() {
        GameManager.I.PlayUISound(closeSounds);
        StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.CloseMenu(bottomRect),
            Toolbox.CoroutineFunc(() => {
                if (remoteCall) {
                    GameManager.I.ShowMenu(MenuType.phoneMenu);
                }
            }
            )
        ));
    }
    void SetPlayerCredits() {
        playerCreditsText.text = GameManager.I.gameData.playerState.credits.ToString();
    }
    void SetSaleData(List<LootData> datas) {
        LootData data = datas[0];
        valueText.text = data.GetValue().ToString();

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
        forSaleTotalPrice = (int)(data.GetValue() * count * bonus);
        totalText.text = $"{forSaleTotalPrice}";
        factorText.text = $"{bonus}x";
        countText.text = $"{count}x";
    }
    void ShowSellerData(LootBuyerData data) {
        foreach (Transform child in buyerPreferencesContainer) {
            Destroy(child.gameObject);
        }
        buyerNameText.text = data.buyerName.ToLower();
        buyerDescriptionText.text = data.description;

        // set portrait
        buyerPortrait.sprite = data.isRemote ? data.phonePortrait : data.portrait;

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
        dialogueController.SetShopownerDialogue(grammar.Parse("{accept}"));
    }

}
