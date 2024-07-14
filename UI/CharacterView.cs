using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MonoBehaviour {

    public GameObject thirdWeaponSlot;
    public Image headImage;
    public Image torsoImage;
    public Image legsImage;
    public TextMeshProUGUI captionText;

    [Header("weapon slots")]
    public LoadoutWeaponButton primaryWeaponButton;
    public LoadoutWeaponButton secondaryWeaponButton;
    public LoadoutWeaponButton tertiaryWeaponButton;
    [Header("item slots")]
    public LoadoutGearSlotButton[] itemSlots;
    public GameObject itemSlot5;
    [Header("buttons")]
    public Button[] allButtons;
    public GameObject itemsContainer;
    public GameObject[] clearButtons;
    Vector3 initialTorsoPosition;
    Vector3 initialHeadPosition;
    float timer;
    Skin legsSkin;
    Skin bodySkin;
    Skin headSkin;
    float headAngle;
    bool initialized;
    PlayerState playerState;
    LevelPlan plan;
    public WeaponState currentWeaponState;
    public MissionPlanLoadoutController loadoutController;
    public void Initialize(GameData data, LevelPlan plan) {
        this.playerState = data.playerState;
        this.plan = plan;

        initialTorsoPosition = torsoImage.transform.localPosition;
        initialHeadPosition = headImage.transform.localPosition;
        Refresh(data.playerState, plan);

        if (loadoutController == null) {
            foreach (Button button in allButtons) button.interactable = false;
            foreach (GameObject button in clearButtons) button.SetActive(false);
            itemsContainer.SetActive(false);
        }

        initialized = true;
    }

    public void Refresh(PlayerState playerState, LevelPlan plan) {
        RefreshItemSlots(playerState, plan);
        primaryWeaponButton.ApplyGunTemplate(playerState.primaryGun);
        secondaryWeaponButton.ApplyGunTemplate(playerState.secondaryGun);
        tertiaryWeaponButton.ApplyGunTemplate(playerState.tertiaryGun);

        InitializeSkins(playerState);
        SetCaptionText(playerState);

        thirdWeaponSlot.SetActive(playerState.PerkThirdWeaponSlot());
        itemSlot5.SetActive(playerState.PerkNumberOfItemSlots() == 5);
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


    void SetCaptionText(PlayerState playerState) {
        string caption = $"HP: {playerState.health}/{playerState.fullHealthAmount()}\nDEF: {playerState.armorLevel}";
        captionText.text = caption;
    }
    void InitializeSkins(PlayerState playerState) {
        string legSkinName = playerState.legSkin;
        string torsoSkinName = playerState.bodySkin;
        string headSkinName = playerState.headSkin;

        legsSkin = Skin.LoadSkin(legSkinName);
        bodySkin = Skin.LoadSkin(torsoSkinName);
        headSkin = Skin.LoadSkin(headSkinName);

        Direction headDirection = Direction.rightDown;

        headImage.sprite = headSkin.headIdle[headDirection][0];
        // torsoImage.sprite = bodySkin.smgIdle[Direction.rightDown][0];
        legsImage.sprite = legsSkin.legsIdle[Direction.rightDown][0];

        Octet<Sprite[]> octet;
        if (currentWeaponState == null || currentWeaponState.gunInstance == null) {
            octet = bodySkin.unarmedIdle;
        } else if (currentWeaponState.type == WeaponType.melee) {
            octet = bodySkin.swordIdle;
        } else {
            octet = currentWeaponState.gunInstance.template.type switch {
                GunType.pistol => bodySkin.pistolIdle,
                GunType.smg => bodySkin.smgIdle,
                GunType.rifle => bodySkin.rifleIdle,
                GunType.shotgun => bodySkin.shotgunIdle,
            };
        }
        torsoImage.sprite = octet[Direction.rightDown][0];
    }
    void RefreshItemSlots(PlayerState playerState, LevelPlan plan) {
        if (plan == null) {

        } else {
            int numberOfItemSlots = playerState.PerkNumberOfItemSlots();
            for (int i = 0; i < numberOfItemSlots; i++) {
                LoadoutGearSlotButton button = itemSlots[i];
                button.Initialize(this, i);
                if (i < plan.items.Count) {
                    SetItemSlot(button, plan.items[i]);
                } else {
                    SetItemSlot(button, null);
                }
            }
        }

    }
    void SetItemSlot(LoadoutGearSlotButton button, ItemTemplate item) {
        if (item == null) {
            button.Clear();
            return;
        } else {
            button.SetItem(item);
        }
    }

    public void WeaponButtonClicked(LoadoutWeaponButton button) {
        currentWeaponState = button.gunState;
        if (loadoutController != null) {
            loadoutController.WeaponSlotClicked(button);
            Refresh(playerState, plan);
        }
    }
    public void ItemButtonClicked(LoadoutGearSlotButton button) {
        if (loadoutController != null) {
            loadoutController.ItemSlotClicked(button);
            Refresh(playerState, plan);
        }
    }
    public void ItemButtonCleared(LoadoutGearSlotButton button) {
        if (loadoutController != null) {
            loadoutController.ClearItemSlot();
            Refresh(playerState, plan);
        }
    }
    public void WeaponButtonCleared(LoadoutWeaponButton button) {
        if (loadoutController != null) {
            loadoutController.ClearWeaponSlot(button);
            Refresh(playerState, plan);
        }
    }
}
