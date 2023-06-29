using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GunShopController : MonoBehaviour {
    public GameObject UIEditorCamera;

    public GameObject gunButtonPrefab;
    public GameObject inventoryButtonPrefab;
    public GameObject gunListDividerPrefab;
    public StoreDialogueController dialogueController;
    public RectTransform bottomRect;
    [Header("lists")]
    public Transform leftGunScrollContainer;
    public Transform rightGunScrollContainer;
    [Header("stats")]
    public GunStatHandler gunStatHandler;
    public TextMeshProUGUI creditsPlayerTotal;
    public TextMeshProUGUI creditsCost;
    [Header("compare")]
    public Image compareBoxImage;
    public TextMeshProUGUI compareGunTitle;
    public GameObject clearButtonObject;
    [Header("buttonbar")]
    public GameObject buyModeButton;
    public GameObject sellModeButton;
    [Header("sounds")]
    public AudioSource audioSource;
    public AudioClip[] buySound;
    public AudioClip[] buyFailSound;
    public AudioClip[] selectGunSound;
    public AudioClip[] mouseOverSound;
    public AudioClip[] discloseBottomSound;
    public AudioClip blitSound;
    Coroutine blitTextRoutine;
    List<GunSaleData> gunSaleData = new List<GunSaleData>();
    GunSaleData currentGunForSale;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }
    public void Start() {
        // rightDialogueName.text = GameManager.I.gameData.filename;
        // leftDialogueName.text = "seller";
        dialogueController.Initialize(GameManager.I.gameData.filename, "honest pete");
        gunSaleData = LoadGunSaleData();
        ClearInitialize();
        SetBuyMode();
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
    }


    List<GunSaleData> LoadGunSaleData() => new List<GunSaleData>(){
            new GunSaleData(GunTemplate.Load("p1"), 600),
            new GunSaleData(GunTemplate.Load("p2"), 500),
            new GunSaleData(GunTemplate.Load("p3"), 800),
            new GunSaleData(GunTemplate.Load("p4"), 750),
            new GunSaleData(GunTemplate.Load("s1"), 1750),
            new GunSaleData(GunTemplate.Load("s2"), 2450),
            new GunSaleData(GunTemplate.Load("s3"), 2700),
            new GunSaleData(GunTemplate.Load("r1"), 5620),
            new GunSaleData(GunTemplate.Load("r2"), 8900),
            new GunSaleData(GunTemplate.Load("sh1"), 2000),
            new GunSaleData(GunTemplate.Load("sh2"), 2300),
            new GunSaleData(GunTemplate.Load("sh2"), 1900),
        };

    void ClearInitialize() {
        PopulateStoreInventory();
        PopulatePlayerInventory();
        SetPlayerCredits();
        // set portraits
        dialogueController.SetShopownerDialogue("Please come in to my underground black market weapons shop.");
        creditsCost.text = "";

        // clear compare gun
        ClearCompareCallback();

        // clear stats
        gunStatHandler.ClearStats();
    }
    void SetBuyMode() {
        buyModeButton.SetActive(false);
        sellModeButton.SetActive(true);
    }
    void SetSellMode() {
        buyModeButton.SetActive(true);
        sellModeButton.SetActive(false);
    }
    void PopulateStoreInventory() {
        foreach (Transform child in leftGunScrollContainer) {
            Destroy(child.gameObject);
        }
        CreateShopGunButtons(gunSaleData);
    }

    void CreateShopGunButtons(List<GunSaleData> saleData) {
        saleData.GroupBy(saleData => saleData.template.type).ToList().ForEach(saleDataGroup => {
            string sectionHeader = saleDataGroup.Key switch {
                GunType.pistol => "Pistols",
                GunType.smg => "SMGs",
                GunType.rifle => "Rifles",
                GunType.shotgun => "Shotguns",
                _ => "other",
            };
            GameObject divider = CreateGunListDivider(sectionHeader);
            divider.transform.SetParent(leftGunScrollContainer, false);
            foreach (GunSaleData data in saleDataGroup) {
                GameObject button = CreateStoreGunButton(data);
                button.transform.SetParent(leftGunScrollContainer, false);
            }
        });
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
        foreach (GunState gun in GameManager.I.gameData.playerState.allGuns) {
            if (gun == null) continue;
            GameObject button = CreatePlayerGunButton(gun);
            if (button == null) continue;
            button.transform.SetParent(rightGunScrollContainer, false);
        }
    }
    GameObject CreateGunListDivider(string title) {
        GameObject obj = GameObject.Instantiate(gunListDividerPrefab);
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        // button.Initialize(data, ShopButtonCallback);
        text.text = title;
        return obj;
    }
    GameObject CreateStoreGunButton(GunSaleData data) {
        GameObject obj = GameObject.Instantiate(gunButtonPrefab);
        GunShopButton button = obj.GetComponent<GunShopButton>();
        button.Initialize(data, ShopButtonCallback);
        return obj;
    }
    GameObject CreatePlayerGunButton(GunState state) {
        if (state.template == null) return null;
        GameObject obj = GameObject.Instantiate(inventoryButtonPrefab);
        GunShopButton button = obj.GetComponent<GunShopButton>();
        button.Initialize(state, InventoryButtonCallback);
        return obj;
    }

    public void ShopButtonCallback(GunShopButton button) {
        SetGunForSale(button.saleData);
    }
    public void InventoryButtonCallback(GunShopButton button) {
        SetCompareGun(button.gunState);
    }

    void SetGunForSale(GunSaleData data) {
        gunStatHandler.DisplayGunTemplate(data.template);
        SetSalePrice(data.cost);
        dialogueController.SetShopownerDialogue(data.template.shopDescription);
        currentGunForSale = data;
        Toolbox.RandomizeOneShot(audioSource, selectGunSound, randomPitchWidth: 0.02f);
        // TODO: grey out Buy button if player has insufficient credits
    }
    void ClearGunForSale() {
        gunStatHandler.ClearGunTemplate();
        currentGunForSale = null;
        creditsCost.text = "";
        // TODO: grey out Buy button if player has insufficient credits
    }

    public void DoneButtonCallback() {
        GameManager.I.HideShopMenu();
    }
    public void SellModeCallback() {
        SetSellMode();
    }
    public void BuyModeCallback() {
        SetBuyMode();
    }
    public void BuyCallback() {
        if (currentGunForSale == null) {
            return;
        } else {
            if (GameManager.I.gameData.playerState.credits >= currentGunForSale.cost) {
                Toolbox.RandomizeOneShot(audioSource, buySound, randomPitchWidth: 0.02f);
                BuyGun(currentGunForSale);
            } else {
                Toolbox.RandomizeOneShot(audioSource, buyFailSound, randomPitchWidth: 0.02f);
            }
        }
    }
    public void ClearCompareCallback() {
        ClearCompareGun();
    }

    void ClearCompareGun() {
        clearButtonObject.SetActive(false);
        compareGunTitle.text = "";
        compareBoxImage.enabled = false;
        gunStatHandler.SetCompareGun(null);
    }
    void SetCompareGun(GunState state) {
        compareGunTitle.text = state.template.name;
        clearButtonObject.SetActive(true);
        compareBoxImage.enabled = true;
        gunStatHandler.SetCompareGun(state);
    }


    void BuyGun(GunSaleData data) {
        // remove credits
        GameManager.I.gameData.playerState.credits -= data.cost;

        // remove gun from sale data
        gunSaleData.Remove(data);

        // add instance to player arsenal
        GunState newGun = GunState.Instantiate(data.template);
        GameManager.I.gameData.playerState.allGuns.Add(newGun);

        // clear stat view
        ClearGunForSale();

        PopulateStoreInventory();
        PopulatePlayerInventory();
        SetPlayerCredits();
        // set portraits
        dialogueController.SetShopownerDialogue("Thank you.");
    }
}
