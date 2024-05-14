using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GunShopController : MonoBehaviour {
    public Canvas myCanvas;

    public GameObject UIEditorCamera;

    public GameObject gunButtonPrefab;
    public GameObject inventoryButtonPrefab;
    public GameObject gunListDividerPrefab;
    public StoreDialogueController dialogueController;
    public RectTransform bottomRect;
    [Header("empty")]
    public GameObject emptyInventoryIndicator;
    public GameObject emptyForSaleIndicator;
    [Header("lists")]
    public Transform leftGunScrollContainer;
    public Transform rightGunScrollContainer;
    public Transform leftGunListFinalSpacer;
    [Header("stats")]
    public GunStatHandler gunStatHandler;
    public TextMeshProUGUI creditsPlayerTotal;
    public TextMeshProUGUI creditsCost;
    public Button buyButton;
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
    public AudioClip[] closeSounds;

    public AudioClip blitSound;
    Coroutine blitTextRoutine;
    // List<GunSaleData> gunSaleData = new List<GunSaleData>();
    GunState currentGunForSale;
    List<GunState> gunsForSale;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }
    public void Initialize(GameData gameData) {
        dialogueController.Initialize(gameData.filename, "Zed");
        SetBuyMode();
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        myCanvas.enabled = true;

        this.gunsForSale = gameData.gunsForSale;
        PopulateStoreInventory(gameData.gunsForSale);
        PopulatePlayerInventory();
        SetPlayerCredits();

        // set portraits
        dialogueController.SetShopownerDialogue("Check back with me tomorrow- I have a shipment coming in from Tangier.");
        creditsCost.text = "";

        // clear compare gun
        ClearCompareCallback();

        // clear stats
        gunStatHandler.ClearStats();
    }
    void SetBuyMode() {
        buyModeButton.SetActive(false);
        sellModeButton.SetActive(true);
        ClearGunForSale();
    }
    void SetSellMode() {
        buyModeButton.SetActive(true);
        sellModeButton.SetActive(false);
    }
    void PopulateStoreInventory(List<GunState> gunsForSale) {
        foreach (Transform child in leftGunScrollContainer) {
            if (child.name == "empty") continue;
            Destroy(child.gameObject);
        }
        CreateShopGunButtons(gunsForSale);
    }

    void CreateShopGunButtons(List<GunState> saleData) {
        int numberGuns = 0;
        saleData.GroupBy(saleData => saleData.template.type).OrderBy(group => group.Key).ToList().ForEach(saleDataGroup => {
            string sectionHeader = saleDataGroup.Key switch {
                GunType.pistol => "Pistols",
                GunType.smg => "SMGs",
                GunType.rifle => "Rifles",
                GunType.shotgun => "Shotguns",
                _ => "other",
            };
            GameObject divider = CreateGunListDivider(sectionHeader);
            divider.transform.SetParent(leftGunScrollContainer, false);
            foreach (GunState data in saleDataGroup) {
                GameObject button = CreateStoreGunButton(data);
                button.transform.SetParent(leftGunScrollContainer, false);
                numberGuns += 1;
            }
        });
        emptyForSaleIndicator.SetActive(numberGuns == 0);
        leftGunListFinalSpacer.SetAsLastSibling();
    }
    void SetPlayerCredits() {
        creditsPlayerTotal.text = GameManager.I.gameData.playerState.credits.ToString();
    }
    void SetSalePrice(int cost) {
        creditsCost.text = cost.ToString();
    }
    void PopulatePlayerInventory() {
        foreach (Transform child in rightGunScrollContainer) {
            if (child.name == "empty") continue;
            Destroy(child.gameObject);
        }
        emptyInventoryIndicator.SetActive(GameManager.I.gameData.playerState.allGuns.Count == 0);
        foreach (WeaponState weapon in GameManager.I.gameData.playerState.allGuns) {
            if (weapon == null || weapon.type == WeaponType.melee) continue;
            GameObject button = CreatePlayerGunButton(weapon.gunInstance);
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
    GameObject CreateStoreGunButton(GunState data) {
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
        SetGunForSale(button.gunState);
    }
    public void InventoryButtonCallback(GunShopButton button) {
        SetCompareGun(button.gunState);
    }

    void SetGunForSale(GunState data) {
        gunStatHandler.DisplayGunState(data);
        SetSalePrice(data.GetCost());
        dialogueController.SetShopownerDialogue(data.template.shopDescription);
        currentGunForSale = data;
        Toolbox.RandomizeOneShot(audioSource, selectGunSound, randomPitchWidth: 0.02f);
        buyButton.interactable = GameManager.I.gameData.playerState.credits >= data.GetCost();
    }
    void ClearGunForSale() {
        gunStatHandler.ClearGunTemplate();
        currentGunForSale = null;
        creditsCost.text = "";
        buyButton.interactable = false;
    }

    public void DoneButtonCallback() {
        GameManager.I.PlayUISound(closeSounds);
        StartCoroutine(Toolbox.CloseMenu(bottomRect));
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
            if (GameManager.I.gameData.playerState.credits >= currentGunForSale.GetCost()) {
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


    void BuyGun(GunState data) {
        // remove credits
        GameManager.I.gameData.playerState.credits -= data.GetCost();

        // remove gun from sale data
        gunsForSale.Remove(data);

        // add instance to player arsenal
        GunState newGun = GunState.Instantiate(data.template);
        GameManager.I.gameData.playerState.allGuns.Add(new WeaponState(newGun));

        // clear stat view
        ClearGunForSale();

        PopulateStoreInventory(gunsForSale);
        PopulatePlayerInventory();
        SetPlayerCredits();
        // set portraits
        dialogueController.SetShopownerDialogue("Thank you.");
    }
}
