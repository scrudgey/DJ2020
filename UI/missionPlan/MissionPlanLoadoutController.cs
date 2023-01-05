using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionPlanLoadoutController : MonoBehaviour {
    public AudioClip[] openPickerSounds;
    public AudioClip[] pickerCallbackSounds;
    public AudioClip[] weaponSelectorSounds;
    public AudioSource audioSource;

    public GameObject thirdWeaponSlot;
    public Image headImage;
    public Image torsoImage;
    public Image legsImage;
    public GunStatHandler gunStatHandler;
    public Transform pickerContainer;
    public GameObject pickerEntryPrefab;
    public GameObject pickerHeaderPrefab;

    [Header("weapon slots")]
    public LoadoutWeaponButton primaryWeaponButton;
    public LoadoutWeaponButton secondaryWeaponButton;
    public LoadoutWeaponButton tertiaryWeaponButton;

    public GameObject primaryHighlight;
    public GameObject secondaryHighlight;
    public GameObject tertiaryHighlight;

    [Header("item slots")]
    public LoadoutGearSlotButton[] itemSlots;
    [Header("picker")]
    public RectTransform statsRect;
    public RectTransform pickerRect;
    public GameObject pickerHeader;
    bool pickerOpen;

    int selectedWeaponSlot;
    int selectedItemSlot;
    GameData data;
    LevelPlan plan;

    // animation
    Vector3 initialTorsoPosition;
    Vector3 initialHeadPosition;
    float timer;

    Skin legsSkin;
    Skin bodySkin;
    Skin headSkin;

    float headAngle;

    Coroutine pickerCoroutine;
    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.plan = plan;
        InitializeSkins(data);

        initialTorsoPosition = torsoImage.transform.localPosition;
        initialHeadPosition = headImage.transform.localPosition;

        primaryWeaponButton.ApplyGunTemplate(data.playerState.primaryGun.template);
        secondaryWeaponButton.ApplyGunTemplate(data.playerState.secondaryGun.template);
        tertiaryWeaponButton.ApplyGunTemplate(data.playerState.tertiaryGun.template);
        InitializeItemSlots(plan);

        thirdWeaponSlot.SetActive(data.playerState.thirdWeaponSlot);

        // data.playerState.items
        selectedWeaponSlot = 1;
        // gunStatHandler.DisplayGunTemplate(data.playerState.primaryGun.template);


        pickerRect.gameObject.SetActive(false);
        pickerHeader.SetActive(false);
        pickerOpen = false;

        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);
        // InitializeWeaponPicker();
    }

    void InitializeItemSlots(LevelPlan plan) {
        for (int i = 0; i < 4; i++) {
            itemSlots[i].Initialize(this, i);
            if (i < plan.items.Length) {
                BaseItem item = ItemInstance.LoadItem(plan.items[i]);
                SetItemSlot(i, item);
            } else {
                SetItemSlot(i, null);
            }
        }
    }
    void SetItemSlot(int index, BaseItem item) {
        if (item == null) {
            itemSlots[index].Clear();
            return;
        } else {
            itemSlots[index].SetItem(item);
        }
    }

    void InitializeSkins(GameData data) {
        string legSkinName = data.playerState.legSkin;
        string torsoSkinName = data.playerState.bodySkin;
        string headSkinName = data.playerState.headSkin;

        legsSkin = Skin.LoadSkin(legSkinName);
        bodySkin = Skin.LoadSkin(torsoSkinName);
        headSkin = Skin.LoadSkin(headSkinName);

        Direction headDirection = Direction.rightDown;

        headImage.sprite = headSkin.headIdle[headDirection][0];
        torsoImage.sprite = bodySkin.smgIdle[Direction.rightDown][0];
        legsImage.sprite = legsSkin.legsIdle[Direction.rightDown][0];
    }


    public void WeaponSlotClicked(int slotIndex, GunTemplate template, bool clear = false) {
        selectedWeaponSlot = slotIndex;
        switch (slotIndex) {
            case 1:
                primaryHighlight.SetActive(true);
                secondaryHighlight.SetActive(false);
                tertiaryHighlight.SetActive(false);
                break;
            case 2:
                primaryHighlight.SetActive(false);
                secondaryHighlight.SetActive(true);
                tertiaryHighlight.SetActive(false);
                break;
            case 3:
                primaryHighlight.SetActive(false);
                secondaryHighlight.SetActive(false);
                tertiaryHighlight.SetActive(true);
                break;
        }
        gunStatHandler.DisplayGunTemplate(template);
        InitializeWeaponPicker();
        if (template == null) {
            torsoImage.sprite = bodySkin.unarmedIdle[Direction.rightDown][0];
        }
        if (clear) {
            StartPickerCoroutine(ClosePicker());
        } else if (!pickerOpen) {
            Toolbox.RandomizeOneShot(audioSource, openPickerSounds, randomPitchWidth: 0.05f);
            StartPickerCoroutine(OpenPicker());
        }

    }
    public void ItemSlotClicked(int slotIndex, LoadoutGearSlotButton button) {
        Debug.Log($"item slot {slotIndex} {button.item}");
        selectedItemSlot = slotIndex;
        InitializeItemPicker();

        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);

        if (!pickerOpen) {
            Toolbox.RandomizeOneShot(audioSource, openPickerSounds, randomPitchWidth: 0.05f);
            StartPickerCoroutine(OpenPicker());
        }
    }
    public void StashPickerCallback(LoadoutStashPickerButton picker) {
        // Toolbox.RandomizeOneShot(audioSource, pickerCallbackSounds, randomPitchWidth: 0.05f);
        switch (picker.type) {
            case LoadoutStashPickerButton.PickerType.gun: WeaponPickerCallback(picker); break;
            case LoadoutStashPickerButton.PickerType.item: ItemPickerCallback(picker); break;
        }
        StartPickerCoroutine(ClosePicker());

    }

    void WeaponPickerCallback(LoadoutStashPickerButton picker) {
        Toolbox.RandomizeOneShot(audioSource, picker.template.unholster, randomPitchWidth: 0.05f);

        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);

        LoadoutWeaponButton button = selectedWeaponSlot switch {
            1 => primaryWeaponButton,
            2 => secondaryWeaponButton,
            3 => tertiaryWeaponButton
        };
        button.ApplyGunTemplate(picker.template);
        gunStatHandler.DisplayGunTemplate(picker.template);

        // TODO: change plan, not player state (requires change to VR mission designer too?)
        GunState newState = picker.template ? GunState.Instantiate(picker.template) : null;
        switch (selectedWeaponSlot) {
            case 1:
                data.playerState.primaryGun = newState;
                break;
            case 2:
                data.playerState.secondaryGun = newState;
                break;
            case 3:
                data.playerState.tertiaryGun = newState;
                break;
        }

        Octet<Sprite[]> octet = picker.template.type switch {
            GunType.pistol => bodySkin.pistolIdle,
            GunType.smg => bodySkin.smgIdle,
            GunType.rifle => bodySkin.rifleIdle,
            GunType.shotgun => bodySkin.shotgunIdle,
        };
        torsoImage.sprite = octet[Direction.rightDown][0];
    }
    void ItemPickerCallback(LoadoutStashPickerButton picker) {
        SetItemSlot(selectedItemSlot, picker.item);
        // plan.items.Add(picker.item.data.shortName);
        plan.items[selectedItemSlot] = picker.item.data.shortName;
        InitializeItemPicker();
    }

    void InitializeItemPicker() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        foreach (string itemName in data.unlockedItems) {
            BaseItem item = ItemInstance.LoadItem(itemName);
            if (item == null) continue;
            if (plan.items.Any(planItem => item.data.shortName == planItem)) continue;
            LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
            pickerHandler.Initialize(this, item);
        }
    }
    void InitializeWeaponPicker() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        GunTemplate[] allGuns = Resources.LoadAll<GunTemplate>("data/guns");

        // None button

        // create pistol divider
        instantiateDivider("Pistol");
        allGuns.Where(gun => gun.type == GunType.pistol).ToList()
            .ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });

        // create smg divider
        instantiateDivider("SMG");
        allGuns.Where(gun => gun.type == GunType.smg).ToList()
            .ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });

        // create rifle divider
        instantiateDivider("Rifle");
        allGuns.Where(gun => gun.type == GunType.rifle).ToList()
            .ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });

        // create shotgun divider
        instantiateDivider("Shotgun");
        allGuns.Where(gun => gun.type == GunType.shotgun).ToList()
            .ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });
    }

    LoadoutStashPickerButton instantiateWeaponPicker() {
        GameObject gameObject = GameObject.Instantiate(pickerEntryPrefab);
        gameObject.transform.SetParent(pickerContainer, false);
        return gameObject.GetComponent<LoadoutStashPickerButton>();
    }
    TextMeshProUGUI instantiateDivider(string caption) {
        GameObject gameObject = GameObject.Instantiate(pickerHeaderPrefab);
        gameObject.transform.SetParent(pickerContainer, false);
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = caption;
        return text;
    }

    void Update() {
        timer += Time.unscaledDeltaTime;

        Vector3 torsoPosition = new Vector3(initialTorsoPosition.x, initialTorsoPosition.y, initialTorsoPosition.z);
        Vector3 headPosition = new Vector3(initialHeadPosition.x, initialHeadPosition.y, initialHeadPosition.z);

        torsoPosition.y += 0.35f * Mathf.Sin(timer);
        headPosition.y += 0.55f * Mathf.Sin(timer + Mathf.PI / 8);

        torsoImage.transform.localPosition = torsoPosition;
        headImage.transform.localPosition = headPosition;

        torsoImage.transform.localScale = (1 + Mathf.Clamp(Mathf.Sin(timer), 0f, 1f) * 0.02f) * Vector3.one;

        Direction headDirection = Direction.rightDown;
        headAngle = Mathf.Sin(timer + Mathf.PI * 3f / 4f);
        switch (headAngle) {
            case float n when (n < -0.5f):
                headDirection = Direction.leftDown;
                break;
            case float n when (n >= -0.5f && n < 0):
                headDirection = Direction.down;
                break;
            case float n when (n >= 0 && n < 0.5f):
                headDirection = Direction.rightDown;
                break;
            case float n when (n > 0.5f):
                headDirection = Direction.right;
                break;
        }
        headImage.sprite = headSkin.headIdle[headDirection][0];

    }


    public void OnWeaponSlotMouseOver(LoadoutWeaponButton button) {
        gunStatHandler.DisplayGunTemplate(button.gunTemplate);
    }

    void StartPickerCoroutine(IEnumerator coroutine) {
        if (pickerCoroutine != null) {
            StopCoroutine(pickerCoroutine);
        }
        pickerCoroutine = StartCoroutine(coroutine);
    }
    IEnumerator OpenPicker() {
        pickerOpen = true;
        pickerRect.gameObject.SetActive(true);
        pickerHeader.SetActive(true);
        float timer = 0f;
        float duration = 0.15f;
        float width = pickerRect.rect.width;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float height = (float)PennerDoubleAnimation.Linear(timer, 0f, 340f, duration);
            pickerRect.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        pickerCoroutine = null;
    }
    IEnumerator ClosePicker() {
        pickerOpen = false;
        float timer = 0f;
        float duration = 0.15f;
        float width = pickerRect.rect.width;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float height = (float)PennerDoubleAnimation.Linear(timer, 340f, -340f, duration);
            pickerRect.sizeDelta = new Vector2(width, height);
            yield return null;
        }
        pickerRect.gameObject.SetActive(false);
        pickerHeader.SetActive(false);
        pickerCoroutine = null;
    }

}
