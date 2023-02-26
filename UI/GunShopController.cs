using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunShopController : MonoBehaviour {
    public GameObject UIEditorCamera;

    public GameObject gunButtonPrefab;
    public GameObject inventoryButtonPrefab;
    public GameObject gunListDividerPrefab;
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

    [Header("dialogue")]
    public Image leftImage;
    public Image rightImage;
    public TextMeshProUGUI dialogueText;
    public LayoutElement dialogueLeftSpacer;
    public LayoutElement dialogueRightSpacer;
    [Header("buttonbar")]
    public GameObject buyModeButton;
    public GameObject sellModeButton;

    Coroutine blitTextRoutine;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Start() {
        ClearInitialize();
        SetBuyMode();
    }
    void ClearInitialize() {
        PopulateStoreInventory();
        PopulatePlayerInventory();
        SetPlayerCredits();
        // set portraits
        SetShopownerDialogue("Please come in to my underground black market weapons shop.");
        creditsCost.text = "-";

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

        GunTemplate[] templates = new GunTemplate[4]{
            GunTemplate.Load("p1"),
            GunTemplate.Load("p2"),
            GunTemplate.Load("p3"),
            GunTemplate.Load("p4"),
        };
        CreateShopGunButtons("Pistols", templates, 1525);

        templates = new GunTemplate[2]{
            GunTemplate.Load("s1"),
            GunTemplate.Load("s2")
        };
        CreateShopGunButtons("SMGs", templates, 2460);

        templates = new GunTemplate[2]{
            GunTemplate.Load("r1"),
            GunTemplate.Load("r2")
        };
        CreateShopGunButtons("Rifles", templates, 7500);

        templates = new GunTemplate[3]{
            GunTemplate.Load("sh1"),
            GunTemplate.Load("sh2"),
            GunTemplate.Load("sh3")
        };
        CreateShopGunButtons("Shotguns", templates, 6500);
    }
    void CreateShopGunButtons(string title, GunTemplate[] templates, int cost) {
        GameObject divider = CreateGunListDivider(title);
        divider.transform.SetParent(leftGunScrollContainer, false);
        foreach (GunTemplate template in templates) {
            GameObject button = CreateStoreGunButton(new GunSaleData(template, cost));
            button.transform.SetParent(leftGunScrollContainer, false);
        }
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
        GunState[] playerGuns = new GunState[3]{
            GameManager.I.gameData.playerState.primaryGun,
            GameManager.I.gameData.playerState.secondaryGun,
            GameManager.I.gameData.playerState.tertiaryGun
        };
        foreach (GunState gun in playerGuns) {
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
        SetCompareGun(button.gunState.template);
    }

    void SetGunForSale(GunSaleData data) {
        gunStatHandler.DisplayGunTemplate(data.template);
        SetSalePrice(data.cost);
        SetShopownerDialogue(data.sellerDescription);
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
    void BlitDialogue(string content) {
        if (blitTextRoutine != null) {
            StopCoroutine(blitTextRoutine);
        }
        blitTextRoutine = StartCoroutine(BlitDialogueText(content));
    }
    public void DoneButtonCallback() {
        GameManager.I.HideGunShopMenu();
    }
    public void SellModeCallback() {
        SetSellMode();
    }
    public void BuyModeCallback() {
        SetBuyMode();
    }
    public void BuyCallback() {
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
    void SetCompareGun(GunTemplate template) {
        compareGunTitle.text = template.name;
        clearButtonObject.SetActive(true);
        compareBoxImage.enabled = true;
        gunStatHandler.SetCompareGun(template);
    }

    public IEnumerator BlitDialogueText(string content) {
        dialogueText.text = "";
        int index = 0;
        float timer = 0f;
        float blitInterval = 0.025f;
        while (timer < blitInterval && index < content.Length) {
            timer += Time.unscaledDeltaTime;
            if (timer >= blitInterval) {
                index += 1;
                timer -= blitInterval;
                dialogueText.text = content.Substring(0, index);
            }
            yield return null;
        }
        dialogueText.text = content;
        blitTextRoutine = null;
    }
}
