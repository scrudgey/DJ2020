using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionPlanLoadoutController : MonoBehaviour {
    public CharacterView characterView;
    public AudioClip[] openPickerSounds;
    public AudioClip[] pickerCallbackSounds;
    public AudioClip[] weaponSelectorSounds;
    public AudioSource audioSource;
    public GunStatHandler gunStatHandler;
    public Transform pickerContainer;
    public GameObject pickerEntryPrefab;
    public GameObject pickerHeaderPrefab;

    [Header("weapon slots")]
    public GameObject primaryHighlight;
    public GameObject secondaryHighlight;
    public GameObject tertiaryHighlight;

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
    Coroutine pickerCoroutine;
    Coroutine statsCoroutine;
    bool initialized;
    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.plan = plan;

        selectedWeaponSlot = 0;

        pickerObject.SetActive(false);
        pickerOpen = false;
        statsOpen = false;
        statsRect.gameObject.SetActive(false);
        gunStatHandler.ClearStats();

        primaryHighlight.SetActive(false);
        secondaryHighlight.SetActive(false);
        tertiaryHighlight.SetActive(false);

        characterView.Initialize(data, plan);

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
            Toolbox.Ease(null, 0.15f, 35f, 446f, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
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
            Toolbox.Ease(null, 0.15f, 446, 35, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                statsRect.sizeDelta = new Vector2(440f, height);
            }, unscaledTime: true),
            Toolbox.CoroutineFunc(() => {
                statsRect.gameObject.SetActive(false);
                gunStatHandler.ClearStats();
                statsCoroutine = null;
            })
        ));
    }

    public void WeaponSlotClicked(LoadoutWeaponButton weaponButton) {
        WeaponState weaponState = weaponButton.gunState;
        int slotIndex = weaponButton.weaponIndex;

        if (weaponState == null) {
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

        if (!pickerOpen) {
            Toolbox.RandomizeOneShot(audioSource, openPickerSounds, randomPitchWidth: 0.05f);
            StartPickerCoroutine(OpenPicker());
        }
    }
    public void ClearWeaponSlot(LoadoutWeaponButton button) {
        int slotIndex = button.weaponIndex;
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
    }

    public void ItemSlotClicked(LoadoutGearSlotButton button) {
        HideStatsView();
        selectedWeaponSlot = 0;

        selectedItemSlot = button.index;
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
        if (!statsOpen) {
            ShowStatsView();
        }
        switch (picker.type) {
            case LoadoutStashPickerButton.PickerType.gun:
                if (picker.gunstate != null && picker.gunstate.type == WeaponType.gun) {
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
                WeaponState gunState = selectedWeaponSlot switch {
                    0 => null,
                    1 => data.playerState.primaryGun,
                    2 => data.playerState.secondaryGun,
                    3 => data.playerState.tertiaryGun
                };
                if (gunState == null) {
                    HideStatsView();
                    break;
                }
                if (gunState.type == WeaponType.gun) {
                    gunStatHandler.DisplayGunState(gunState.gunInstance);
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
        characterView.currentWeaponState = weaponState;

        // view stuff
        characterView.Refresh(data.playerState, plan);
    }
    void ItemPickerCallback(LoadoutStashPickerButton picker) {
        plan.items[selectedItemSlot] = picker.item;
        InitializeItemPicker();

        // view stuff
        characterView.Refresh(data.playerState, plan);
    }
    public void ClearItemSlot() {
        // TODO
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
