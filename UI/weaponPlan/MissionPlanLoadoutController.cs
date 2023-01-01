using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanLoadoutController : MonoBehaviour {
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

    public LoadoutWeaponButton primaryWeaponButton;
    public LoadoutWeaponButton secondaryWeaponButton;
    public LoadoutWeaponButton tertiaryWeaponButton;

    int activeWeaponSlot;
    GameData data;

    public void Initialize(GameData data, LevelTemplate template) {
        this.data = data;

        primaryWeaponButton.ApplyGunTemplate(data.playerState.primaryGun.template);
        secondaryWeaponButton.ApplyGunTemplate(data.playerState.secondaryGun.template);
        tertiaryWeaponButton.ApplyGunTemplate(data.playerState.tertiaryGun.template);

        thirdWeaponSlot.SetActive(data.playerState.thirdWeaponSlot);

        // data.playerState.items
        activeWeaponSlot = 1;
        gunStatHandler.DisplayGunTemplate(data.playerState.primaryGun.template);

        InitializeWeaponPicker();
    }

    public void WeaponSlotClicked(int slotIndex, GunTemplate template) {
        Toolbox.RandomizeOneShot(audioSource, weaponSelectorSounds);
        activeWeaponSlot = slotIndex;
        gunStatHandler.DisplayGunTemplate(template);
        InitializeWeaponPicker();
    }
    public void StashPickerCallback(LoadoutStashPickerButton picker) {
        Toolbox.RandomizeOneShot(audioSource, pickerCallbackSounds);

        LoadoutWeaponButton button = activeWeaponSlot switch {
            1 => primaryWeaponButton,
            2 => secondaryWeaponButton,
            3 => tertiaryWeaponButton
        };
        button.ApplyGunTemplate(picker.template);
        gunStatHandler.DisplayGunTemplate(picker.template);
        GunState newState = picker.template ? GunState.Instantiate(picker.template) : null;
        switch (activeWeaponSlot) {
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


}
