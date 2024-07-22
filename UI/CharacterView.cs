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
    public bool animate;
    public GameObject thirdWeaponSlot;
    public Image headImage;
    public Image torsoImage;
    public Image legsImage;
    public TextMeshProUGUI captionText;

    [Header("animation")]
    public CustomAnimator customAnimator;
    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    // public AnimationClip unarmedWalkSlowAnimation;

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

    float movementTypeFactor;
    float baseAngle;
    Direction headDirection;
    Direction baseDirection;
    Octet<Sprite[]> torsoOctet;
    Octet<Sprite[]> legsOctet;
    float headAngle2;
    int _frame;

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

        if (animate) {
            movementTypeFactor = Mathf.Sin((timer / 10f) * (2 * Mathf.PI));    // 2𝛑 = 1 cycle. t -> 2𝛑 / T
            baseAngle -= Time.unscaledDeltaTime * 10f;
        }
        headAngle2 = Mathf.Sin(timer + Mathf.PI * 3f / 4f) * 90;

        if (baseAngle > 180f) baseAngle -= 360f;
        if (baseAngle < -180f) baseAngle += 360f;
        Direction baseDirection = Toolbox.DirectionFromAngle(baseAngle);
        headDirection = Toolbox.DirectionFromAngle(baseAngle + headAngle2);

        // -1, 0, 1
        int movementIndex = movementTypeFactor switch {
            < 0 => 0,
            < 0.5f => 1,
            _ => 2
        };
        if (movementIndex == 0) _frame = 0;
        SetTorsoOctet(movementIndex);
        AnimationClip animationClip = movementIndex == 0 ? idleAnimation : unarmedWalkAnimation;
        SetAnimation(animationClip);


        headImage.sprite = headSkin.headIdle[headDirection][0];
        legsImage.sprite = legsOctet[baseDirection][_frame];
        _frame = Math.Min(_frame, torsoOctet[baseDirection].Length - 1);
        torsoImage.sprite = torsoOctet[baseDirection][_frame];

        if (baseDirection == Direction.left || baseDirection == Direction.leftUp || baseDirection == Direction.leftDown) {
            Vector3 legscale = legsImage.transform.localScale;
            legscale.x = -Math.Abs(legscale.x);
            legsImage.transform.localScale = legscale;
        } else {
            Vector3 legscale = legsImage.transform.localScale;
            legscale.x = Math.Abs(legscale.x);
            legsImage.transform.localScale = legscale;
        }
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

        SetTorsoOctet(0);

        headDirection = Direction.rightDown;
        movementTypeFactor = -1;
        baseAngle = -90;

        headImage.sprite = headSkin.headIdle[headDirection][0];
        legsImage.sprite = legsSkin.legsIdle[Direction.rightDown][0];
        torsoImage.sprite = torsoOctet[Direction.rightDown][0];
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


    void SetTorsoOctet(int index) {
        if (currentWeaponState == null || currentWeaponState.gunInstance == null) {
            torsoOctet = index switch {
                0 => bodySkin.unarmedIdle,
                1 => bodySkin.unarmedWalk,
                2 => bodySkin.unarmedRun
            };
        } else if (currentWeaponState.type == WeaponType.melee) {
            torsoOctet = bodySkin.swordIdle;
        } else {
            torsoOctet = currentWeaponState.gunInstance.template.type switch {
                GunType.pistol => index switch {
                    0 => bodySkin.pistolIdle,
                    1 => bodySkin.pistolIdle,
                    2 => bodySkin.pistolRun
                },
                GunType.smg => index switch {
                    0 => bodySkin.smgIdle,
                    1 => bodySkin.smgIdle,
                    2 => bodySkin.smgRun
                },
                GunType.rifle => index switch {
                    0 => bodySkin.rifleIdle,
                    1 => bodySkin.rifleIdle,
                    2 => bodySkin.smgRun
                },
                GunType.shotgun => index switch {
                    0 => bodySkin.shotgunIdle,
                    1 => bodySkin.shotgunIdle,
                    2 => bodySkin.smgRun
                }
            };
        }
        if (index == 0) {
            legsOctet = legsSkin.legsIdle;
        } else if (index == 1) {
            legsOctet = legsSkin.legsWalk;
        } else if (index == 2) {
            legsOctet = legsSkin.legsRun;
        }
    }
    private void SetAnimation(AnimationClip clip) {
        customAnimator.playbackSpeed = 1f;

        if (customAnimator.clip != clip) {
            customAnimator.Stop();
            // bob = false;
            customAnimator.clip = clip;
            customAnimator.Play();
        }
    }
    public void SetFrame(int frame) {
        _frame = frame;
    }
}
