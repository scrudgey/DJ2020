using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ItemShopController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public GameObject itemButtonPrefab;
    public GameObject bodyContainer;
    public StoreDialogueController dialogueController;
    public RectTransform bottomRect;

    [Header("lists")]
    public Transform leftGunScrollContainer;
    public Transform rightGunScrollContainer;
    [Header("stats")]
    public TextMeshProUGUI creditsPlayerTotal;
    public TextMeshProUGUI creditsCost;
    public TextMeshProUGUI itemNameTitle;
    public Image itemImage;
    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip[] buySound;
    public AudioClip[] buyFailSound;
    public AudioClip[] selectGunSound;
    public AudioClip blitSound;
    public AudioClip[] discloseBottomSound;
    Coroutine blitTextRoutine;

    List<ItemSaleData> itemSaleData = new List<ItemSaleData>();
    ItemSaleData currentItemForSale;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }
    public void Start() {
        dialogueController.Initialize(GameManager.I.gameData.filename, "seller");
        itemSaleData = LoadItemSaleData();
        ClearInitialize();
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
    }
    List<ItemSaleData> LoadItemSaleData() => new List<ItemSaleData>(){
            new ItemSaleData(ItemInstance.LoadItem("C4"), 600),
            new ItemSaleData(ItemInstance.LoadItem("grenade"), 500),
            new ItemSaleData(ItemInstance.LoadItem("rocket"), 800),
            new ItemSaleData(ItemInstance.LoadItem("goggles"), 750),
        };

    void ClearInitialize() {
        PopulateStoreInventory();
        PopulatePlayerInventory();
        SetPlayerCredits();
        // set portraits
        dialogueController.SetShopownerDialogue("Please come in to my underground black market items shop.");
        creditsCost.text = "";
        ClearItemForSale();
        bodyContainer.SetActive(false);
    }
    public void DoneButtonCallback() {
        GameManager.I.HideShopMenu();
    }

    void PopulateStoreInventory() {
        foreach (Transform child in leftGunScrollContainer) {
            Destroy(child.gameObject);
        }
        CreateShopItemButtons(itemSaleData);
    }
    void SetPlayerCredits() {
        creditsPlayerTotal.text = GameManager.I.gameData.playerState.credits.ToString();
    }
    void SetSalePrice(int cost) {
        creditsCost.text = cost.ToString();
    }
    void PopulatePlayerInventory() {
        foreach (Transform child in rightGunScrollContainer) {
            Destroy(child.gameObject);
        }
        foreach (BaseItem item in GameManager.I.gameData.playerState.allItems) {
            if (item == null) continue;
            GameObject button = CreatePlayerItemButton(item);
            if (button == null) continue;
            button.transform.SetParent(rightGunScrollContainer, false);
        }
    }
    void CreateShopItemButtons(List<ItemSaleData> saleData) {
        saleData.ForEach(saleData => {
            GameObject button = CreateStoreItemButton(saleData);
            button.transform.SetParent(leftGunScrollContainer, false);
        });
    }
    GameObject CreateStoreItemButton(ItemSaleData data) {
        GameObject obj = GameObject.Instantiate(itemButtonPrefab);
        ItemShopButton button = obj.GetComponent<ItemShopButton>();
        button.Initialize(data, ShopButtonCallback);
        return obj;
    }
    public void ShopButtonCallback(ItemShopButton button) {
        bodyContainer.SetActive(true);
        SetItemForSale(button.saleData);
    }
    void SetItemForSale(ItemSaleData data) {
        SetSalePrice(data.cost);
        dialogueController.SetShopownerDialogue(data.item.data.shopDescription);
        currentItemForSale = data;
        Toolbox.RandomizeOneShot(audioSource, selectGunSound);

        itemImage.enabled = true;
        itemImage.sprite = data.item.data.image;
        itemNameTitle.text = data.item.data.name;
    }
    void ClearItemForSale() {
        currentItemForSale = null;
        creditsCost.text = "";

        itemImage.enabled = false;
        itemNameTitle.text = "";
    }
    GameObject CreatePlayerItemButton(BaseItem item) {
        if (item == null) return null;
        GameObject obj = GameObject.Instantiate(itemButtonPrefab);
        ItemShopButton button = obj.GetComponent<ItemShopButton>();
        button.Initialize(item, InventoryButtonCallback);
        return obj;
    }
    public void InventoryButtonCallback(ItemShopButton button) {
        bodyContainer.SetActive(true);
        // SetCompareGun(button.gunState.template);
        ClearItemForSale();
        dialogueController.SetShopownerDialogue(button.item.data.shopDescription);
        currentItemForSale = null;
        Toolbox.RandomizeOneShot(audioSource, selectGunSound);

        itemImage.enabled = true;
        itemImage.sprite = button.item.data.image;
        itemNameTitle.text = button.item.data.name;
    }

    public void BuyCallback() {
        if (currentItemForSale == null) {
            return;
        } else {
            if (GameManager.I.gameData.playerState.credits >= currentItemForSale.cost) {
                Toolbox.RandomizeOneShot(audioSource, buySound);
                BuyItem(currentItemForSale);
            } else {
                Toolbox.RandomizeOneShot(audioSource, buyFailSound);
            }
        }
    }
    void BuyItem(ItemSaleData data) {
        // remove credits
        GameManager.I.gameData.playerState.credits -= data.cost;


        // remove gun from sale data
        itemSaleData.Remove(data);

        // add instance to player arsenal
        // GunState newGun = GunState.Instantiate(data.template);
        GameManager.I.gameData.playerState.allItems.Add(data.item);

        // clear stat view
        ClearItemForSale();
        PopulateStoreInventory();
        PopulatePlayerInventory();
        SetPlayerCredits();
        // set portraits
        dialogueController.SetShopownerDialogue("Thank you.");
    }


}
