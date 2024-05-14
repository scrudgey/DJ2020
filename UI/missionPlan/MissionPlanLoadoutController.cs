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
    public GameObject itemSlot5;
    [Header("picker")]
    public GameObject pickerObject;
    public RectTransform statsRect;
    public RectTransform pickerRect;
    bool pickerOpen;
    bool statsOpen;

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
    Coroutine statsCoroutine;
    bool initialized;
    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.plan = plan;
        InitializeSkins(data);

        initialTorsoPosition = torsoImage.transform.localPosition;
        initialHeadPosition = headImage.transform.localPosition;

        ApplyGunTemplate(primaryWeaponButton, data.playerState.primaryGun);
        ApplyGunTemplate(secondaryWeaponButton, data.playerState.secondaryGun);
        ApplyGunTemplate(tertiaryWeaponButton, data.playerState.tertiaryGun);

        // hide tertiary box

        InitializeItemSlots(plan);

        selectedWeaponSlot = 0;

        pickerObject.SetActive(false);
        pickerOpen = false;
        statsOpen = false;
        statsRect.gameObject.SetActive(false);
        gunStatHandler.ClearStats();

        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);

        thirdWeaponSlot.SetActive(data.playerState.PerkThirdWeaponSlot());

        itemSlot5.SetActive(GameManager.I.gameData.playerState.PerkNumberOfItemSlots() == 5);

        initialized = true;
    }


    void ShowStatsView() {
        if (statsOpen) return;
        if (statsCoroutine != null) {
            StopCoroutine(statsCoroutine);
        }
        statsOpen = true;
        statsRect.gameObject.SetActive(true);
        statsRect.sizeDelta = new Vector2(440f, 35f);
        statsCoroutine = StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.5f, 35f, 375f, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                statsRect.sizeDelta = new Vector2(440f, height);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => statsCoroutine = null)
        ));
    }

    void HideStatsView() {
        if (!statsOpen) return;
        if (statsCoroutine != null) {
            StopCoroutine(statsCoroutine);
        }
        statsOpen = false;
        statsCoroutine = StartCoroutine(Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.5f, 375, 35, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                statsRect.sizeDelta = new Vector2(440f, height);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                statsRect.gameObject.SetActive(false);
                gunStatHandler.ClearStats();
                statsCoroutine = null;
            })
        ));
    }


    void ApplyGunTemplate(LoadoutWeaponButton button, WeaponState gunstate) {
        if (gunstate != null) {
            button.ApplyGunTemplate(gunstate);
        } else {
            button.WeaponClearCallback();
        }
    }

    void InitializeItemSlots(LevelPlan plan) {
        int numberOfItemSlots = GameManager.I.gameData.playerState.PerkNumberOfItemSlots();
        for (int i = 0; i < numberOfItemSlots; i++) {
            itemSlots[i].Initialize(this, i);
            if (i < plan.items.Count) {
                // BaseItem item = BaseItem.LoadItem(plan.items[i]);
                SetItemSlot(i, plan.items[i]);
            } else {
                SetItemSlot(i, null);
            }
        }
    }
    void SetItemSlot(int index, ItemTemplate item) {
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


    public void WeaponSlotClicked(int slotIndex, WeaponState weaponState, bool clear = false) {
        if (clear) {
            gunStatHandler.ClearGunTemplate();

        } else {
            ShowStatsView();
            if (weaponState.type == WeaponType.gun) {
                gunStatHandler.DisplayGunState(weaponState.gunInstance);
                gunStatHandler.SetCompareGun(weaponState.gunInstance);
            }
        }

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
        InitializeWeaponPicker();
        if (weaponState == null) {
            torsoImage.sprite = bodySkin.unarmedIdle[Direction.rightDown][0];
        }
        if (clear) {
            selectedWeaponSlot = 0;
            primaryHighlight.SetActive(false);
            secondaryHighlight.SetActive(false);
            tertiaryHighlight.SetActive(false);
            StartPickerCoroutine(ClosePicker());
            switch (slotIndex) {
                case 1:
                    data.playerState.primaryGun = null;
                    break;
                case 2:
                    data.playerState.secondaryGun = null;
                    break;
                case 3:
                    data.playerState.tertiaryGun = null;
                    break;
            }
        } else if (!pickerOpen) {
            Toolbox.RandomizeOneShot(audioSource, openPickerSounds, randomPitchWidth: 0.05f);
            StartPickerCoroutine(OpenPicker());
        }

    }
    public void ItemSlotClicked(int slotIndex, LoadoutGearSlotButton button) {
        // Debug.Log($"item slot {slotIndex} {button.item}");
        HideStatsView();
        selectedWeaponSlot = 0;

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
            case LoadoutStashPickerButton.PickerType.gun:
                WeaponPickerCallback(picker.gunstate);
                break;
            case LoadoutStashPickerButton.PickerType.item: ItemPickerCallback(picker); break;
        }
        StartPickerCoroutine(ClosePicker());
        HideStatsView();
        selectedWeaponSlot = 0;
    }
    public void StashPickerMouseOverCallback(LoadoutStashPickerButton picker) {
        switch (picker.type) {
            case LoadoutStashPickerButton.PickerType.gun:
                // gunStatHandler.SetCompareGun(picker.gunstate.template);
                // gunStatHandler.DisplayGunTemplate(picker.gunstate.template);
                if (picker.gunstate.type == WeaponType.gun) {
                    gunStatHandler.DisplayGunState(picker.gunstate.gunInstance);
                } else {
                    gunStatHandler.ClearGunTemplate();
                }
                break;
        }
    }
    public void StashPickerMouseExitCallback(LoadoutStashPickerButton picker) {
        switch (picker.type) {
            case LoadoutStashPickerButton.PickerType.gun:
                // gunStatHandler.DisplayGunTemplate(picker.gunstate.template);
                LoadoutWeaponButton button = selectedWeaponSlot switch {
                    1 => primaryWeaponButton,
                    2 => secondaryWeaponButton,
                    3 => tertiaryWeaponButton
                };
                if (button.gunState.type == WeaponType.gun) {
                    gunStatHandler.DisplayGunState(button.gunState.gunInstance);
                } else {
                    gunStatHandler.ClearGunTemplate();
                }
                break;
        }
    }

    void WeaponPickerCallback(WeaponState weaponState) {
        if (weaponState.type == WeaponType.gun)
            Toolbox.RandomizeOneShot(audioSource, weaponState.gunInstance.template.unholster, randomPitchWidth: 0.05f);

        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);

        LoadoutWeaponButton button = selectedWeaponSlot switch {
            1 => primaryWeaponButton,
            2 => secondaryWeaponButton,
            3 => tertiaryWeaponButton
        };

        // WeaponState weaponState = new WeaponState(gunstate);
        button.ApplyGunTemplate(weaponState);
        // TODO: apply total state, not just template
        // gunStatHandler.DisplayGunTemplate(picker.gunstate.template);
        // TODO: change plan, not player state (requires change to VR mission designer too?)
        switch (selectedWeaponSlot) {
            case 1:
                data.playerState.primaryGun = weaponState;
                break;
            case 2:
                data.playerState.secondaryGun = weaponState;
                break;
            case 3:
                data.playerState.tertiaryGun = weaponState;
                break;
        }
        Octet<Sprite[]> octet;
        if (weaponState.type == WeaponType.melee) {
            octet = bodySkin.swordIdle;
        } else {
            octet = weaponState.gunInstance.template.type switch {
                GunType.pistol => bodySkin.pistolIdle,
                GunType.smg => bodySkin.smgIdle,
                GunType.rifle => bodySkin.rifleIdle,
                GunType.shotgun => bodySkin.shotgunIdle,
            };
        }
        torsoImage.sprite = octet[Direction.rightDown][0];
    }
    void ItemPickerCallback(LoadoutStashPickerButton picker) {
        SetItemSlot(selectedItemSlot, picker.item);
        // plan.items.Add(picker.item.data.shortName);
        plan.items[selectedItemSlot] = picker.item;
        InitializeItemPicker();
    }

    void InitializeItemPicker() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        foreach (ItemTemplate item in data.playerState.allItems) {
            // BaseItem item = BaseItem.LoadItem(itemName);
            if (item == null) continue;
            // if (plan.items.Any(planItem => item.data.shortName == planItem)) continue;
            if (plan.items.Contains(item)) continue;
            LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
            pickerHandler.Initialize(this, item);
        }
    }
    void InitializeWeaponPicker() {
        foreach (Transform child in pickerContainer) {
            Destroy(child.gameObject);
        }
        List<WeaponState> allGuns = GameManager.I.gameData.playerState.allGuns
            .Where(weapon => weapon.type == WeaponType.gun)
            .ToList();
        List<WeaponState> melees = GameManager.I.gameData.playerState.allGuns.Where(weapon => weapon.type == WeaponType.melee).ToList();

        List<WeaponState> pistols = allGuns.Where(gun => gun.gunInstance.template.type == GunType.pistol)
            .Where(gun => gun != data.playerState.primaryGun && gun != data.playerState.secondaryGun && gun != data.playerState.tertiaryGun)
            .ToList();
        List<WeaponState> smgs = allGuns.Where(gun => gun.gunInstance.template.type == GunType.smg)
            .Where(gun => gun != data.playerState.primaryGun && gun != data.playerState.secondaryGun && gun != data.playerState.tertiaryGun)
            .ToList();
        List<WeaponState> rifles = allGuns.Where(gun => gun.gunInstance.template.type == GunType.rifle)
            .Where(gun => gun != data.playerState.primaryGun && gun != data.playerState.secondaryGun && gun != data.playerState.tertiaryGun)
            .ToList();
        List<WeaponState> shotguns = allGuns.Where(gun => gun.gunInstance.template.type == GunType.shotgun)
            .Where(gun => gun != data.playerState.primaryGun && gun != data.playerState.secondaryGun && gun != data.playerState.tertiaryGun)
            .ToList();

        // None button

        if (pistols.Count > 0) {
            // create pistol divider
            instantiateDivider("Pistol");
            pistols.ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });
        }
        if (smgs.Count > 0) {
            // create smg divider
            instantiateDivider("SMG");
            smgs.ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });
        }
        if (rifles.Count > 0) {
            // create rifle divider
            instantiateDivider("Rifle");
            rifles.ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });
        }
        if (shotguns.Count > 0) {
            // create shotgun divider
            instantiateDivider("Shotgun");
            shotguns.ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });
        }
        if (melees.Count > 0) {
            // create shotgun divider
            instantiateDivider("Melee");
            melees.ForEach(gun => {
                LoadoutStashPickerButton pickerHandler = instantiateWeaponPicker();
                pickerHandler.Initialize(this, gun);
            });
        }
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
        if (!initialized) return;
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
        if (button.gunState != null) {
            ShowStatsView();
            if (button.gunState.type == WeaponType.gun) {
                gunStatHandler.DisplayGunState(button.gunState.gunInstance);
            } else {
                gunStatHandler.ClearGunTemplate();
            }
        }
    }
    public void OnWeaponSlotMouseExit(LoadoutWeaponButton button) {
        if (selectedWeaponSlot == 0) {
            HideStatsView();
        }
    }

    void StartPickerCoroutine(IEnumerator coroutine) {
        if (!gameObject.activeInHierarchy) return;
        if (pickerCoroutine != null) {
            StopCoroutine(pickerCoroutine);
        }
        pickerCoroutine = StartCoroutine(coroutine);
    }
    IEnumerator OpenPicker() {
        pickerOpen = true;
        pickerObject.SetActive(true);
        // pickerHeader.SetActive(true);
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
        pickerObject.SetActive(false);
        // pickerHeader.SetActive(false);
        pickerCoroutine = null;
    }

    public void ClosePickerButtonCallback() {
        StartPickerCoroutine(ClosePicker());
        HideStatsView();
        selectedWeaponSlot = 0;
        selectedItemSlot = 0;
        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);
    }

}
