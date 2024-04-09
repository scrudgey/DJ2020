using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GunModShopController : MonoBehaviour {
    public Canvas myCanvas;
    public GameObject UIEditorCamera;
    public StoreDialogueController storeDialogueController;
    public RectTransform bottomRect;
    public AudioSource audioSource;

    [Header("empty")]
    public GameObject emptyGunsIndicator;
    public GameObject mainEmptyIndicator;
    public GameObject modlist;
    public GameObject statsList;
    public GameObject emptyModlist;

    [Header("mod buttons")]
    public Transform gunModButtonContainer;
    public GameObject gunModButtonPrefab;
    public TextMeshProUGUI modDescriptionTitle;
    public TextMeshProUGUI modDescriptionText;
    public TextMeshProUGUI modSummaryText;
    public TextMeshProUGUI modCostText;
    public TextMeshProUGUI playerCreditsText;
    [Header("inventory")]
    public GunStatHandler gunStatHandler;
    public Transform rightGunScrollContainer;
    public GameObject inventoryButtonPrefab;
    public Button buyButton;
    public GunShopButton selectedGunButton;
    private GunModButton currentGunModButton;
    [Header("infoDisplay")]
    public GameObject activeModIndicatorPrefab;
    public GameObject detailsObject;
    public GameObject activeModsObject;
    [Header("sprites")]

    public Sprite silencerModSprite;
    public Sprite clipModSprite;
    public Sprite damageModSprite;
    public Sprite fireRateSprite;
    [Header("sounds")]
    public AudioClip[] gunSelectSound;
    public AudioClip[] modSelectSound;
    public AudioClip[] buySound;
    public AudioClip[] discloseBottomSound;
    public AudioClip[] closeSounds;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }
    public void Initialize() {
        selectedGunButton = null;
        currentGunModButton = null;

        ClearGunForSale();
        ClearModInformation();
        ClearModButtons();
        ClearActiveModDisplay();
        ClearPlayerInventory();
        PopulatePlayerInventory();
        SetPlayerCredits();
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        activeModsObject.SetActive(false);
        storeDialogueController.Initialize(GameManager.I.gameData.filename, "Shin");
        storeDialogueController.SetShopownerDialogue("You need something with a little more punch, huh? I got you covered.");
        SetNoGunsIndicator(GameManager.I.gameData.playerState.allGuns.Count == 0);
        myCanvas.enabled = true;
    }

    void SetNoGunsIndicator(bool value) {
        emptyGunsIndicator.SetActive(value);
        mainEmptyIndicator.SetActive(value);
        modlist.SetActive(!value);
        statsList.SetActive(!value);
    }
    void ShowEmptyModsIndicator() {
        activeModsObject.SetActive(false);
        detailsObject.SetActive(false);
        emptyModlist.SetActive(true);
        gunModButtonContainer.gameObject.SetActive(false);
    }
    void SetPlayerCredits() {
        playerCreditsText.text = $"{GameManager.I.gameData.playerState.credits}";
    }
    void ClearModInformation() {
        modDescriptionTitle.text = "";
        modDescriptionText.text = "";
        modSummaryText.text = "";
        modCostText.text = $"-";
        buyButton.interactable = false;
        gunStatHandler.SetCompareGun(null);
    }
    public void OnModButtonClicked(GunModButton button) {
        Toolbox.RandomizeOneShot(audioSource, modSelectSound);

        ShowModDetails();
        currentGunModButton = button;
        GunMod mod = button.gunMod;

        modDescriptionTitle.text = mod.title;
        modDescriptionText.text = mod.description;

        modCostText.text = $"{mod.cost}";

        buyButton.interactable = GameManager.I.gameData.playerState.credits >= mod.cost;

        modSummaryText.text = ModSummary(mod);

        GunState compareState = selectedGunButton.gunState.Copy();
        compareState.delta.activeMods.Add(button.gunMod);
        gunStatHandler.SetCompareGun(compareState);

    }
    public static string ModSummary(GunMod mod) => mod.type switch {
        GunModType.silencer => "+silencer",
        GunModType.damage => $"+{mod.baseDamage.low}-{mod.baseDamage.high} DAM",
        GunModType.fireRate => $"+{Mathf.Abs(mod.shootInterval)} RATE",
        GunModType.clipSize => $"+{mod.clipSize} CLIP",
        _ => ""
    };

    Sprite GunModSprite(GunMod gunMod) {
        return gunMod.type switch {
            GunModType.silencer => silencerModSprite,
            GunModType.clipSize => clipModSprite,
            GunModType.damage => damageModSprite,
            GunModType.fireRate => fireRateSprite,
            _ => silencerModSprite
        };
    }

    void ClearPlayerInventory() {
        foreach (Transform child in rightGunScrollContainer) {
            if (child.name == "empty") continue;
            Destroy(child.gameObject);
        }
    }
    void ClearModButtons() {
        foreach (Transform child in gunModButtonContainer) {
            Destroy(child.gameObject);
        }
        gunStatHandler.SetCompareGun(null);
    }
    void ClearActiveModDisplay() {
        foreach (Transform child in activeModsObject.transform) {
            if (child.name == "title") continue;
            Destroy(child.gameObject);
        }
    }


    void PopulatePlayerInventory() {
        foreach (WeaponState weapon in GameManager.I.gameData.playerState.allGuns) {
            if (weapon == null || weapon.type == WeaponType.melee) continue;
            CreatePlayerGunButton(weapon.gunInstance);
        }
    }

    GameObject CreatePlayerGunButton(GunState state) {
        if (state.template == null) return null;
        GameObject obj = GameObject.Instantiate(inventoryButtonPrefab);
        obj.transform.SetParent(rightGunScrollContainer, false);

        GunShopButton button = obj.GetComponent<GunShopButton>();
        button.Initialize(state, InventoryButtonCallback);
        return obj;
    }
    Button CreateModButton(GunMod mod) {
        GameObject obj = GameObject.Instantiate(gunModButtonPrefab);
        obj.transform.SetParent(gunModButtonContainer, false);

        GunModButton button = obj.GetComponent<GunModButton>();
        button.Initialize(this, mod, GunModSprite(mod));

        return obj.GetComponent<Button>();
    }
    ActiveModIndicator CreateModIndicator(GunMod mod) {
        GameObject obj = GameObject.Instantiate(activeModIndicatorPrefab);
        obj.transform.SetParent(activeModsObject.transform, false);
        ActiveModIndicator indicator = obj.GetComponent<ActiveModIndicator>();
        indicator.Initialize(mod, GunModSprite(mod));
        return indicator;
    }


    public void InventoryButtonCallback(GunShopButton button) {
        // if (selectedGunButton.gunState == button.gunState) return;
        Toolbox.RandomizeOneShot(audioSource, gunSelectSound);
        gunStatHandler.DisplayGunTemplate(button.gunState);
        selectedGunButton = button;
        ClearModButtons();
        ClearModInformation();
        PopulateModButtons(button.gunState);
        ShowActiveMods();
    }

    void ClearGunForSale() {
        gunStatHandler.ClearGunTemplate();
        ClearModButtons();
        ClearModInformation();
    }
    void PopulateModButtons(GunState state) {
        if (state.template.availableMods.Count == 0) {
            ShowEmptyModsIndicator();
        } else {
            ShowModDetails();
            foreach (GunMod mod in state.template.availableMods) {
                Button button = CreateModButton(mod);
                if (state.delta.activeMods.Contains(mod)) {
                    button.interactable = false;
                    GunModButton gunModButton = button.GetComponent<GunModButton>();
                    gunModButton.SetEnabledColors();
                    // button.SetEnabledColors();
                }
            }
        }
    }
    void ShowActiveMods() {
        ClearActiveModDisplay();
        PopulateActiveMods();
        detailsObject.SetActive(false);
        activeModsObject.SetActive(true);
    }
    void ShowModDetails() {
        emptyModlist.SetActive(false);
        gunModButtonContainer.gameObject.SetActive(true);
        detailsObject.SetActive(true);
        activeModsObject.SetActive(false);
    }

    void PopulateActiveMods() {
        foreach (GunMod mod in selectedGunButton.gunState.delta.activeMods) {
            CreateModIndicator(mod);
        }
    }

    public void BuyButtonCallback() {
        if (currentGunModButton == null) return;
        Toolbox.RandomizeOneShot(audioSource, buySound);
        GameManager.I.gameData.playerState.credits -= currentGunModButton.gunMod.cost;
        selectedGunButton.gunState.delta.activeMods.Add(currentGunModButton.gunMod);
        InventoryButtonCallback(selectedGunButton);
        gunStatHandler.SetCompareGun(null);
        gunStatHandler.DisplayGunTemplate(selectedGunButton.gunState);

        ClearPlayerInventory();
        SetPlayerCredits();
        PopulatePlayerInventory();

        storeDialogueController.SetShopownerDialogue("I'll get that fixed right up for you.");
    }

    public void DoneButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.PlayUISound(closeSounds);
    }
}
