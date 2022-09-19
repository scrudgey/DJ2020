using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public record PlayerState : ISkinState, IGunHandlerState, IItemHandlerState, ICharacterHurtableState {
    public int credits;

    // skin
    public string legSkin { get; set; }
    public string bodySkin { get; set; }

    // gun
    public GunInstance primaryGun { get; set; }
    public GunInstance secondaryGun { get; set; }
    public GunInstance tertiaryGun { get; set; }
    public int activeGun { get; set; }

    // health
    public float health { get; set; }
    public float fullHealthAmount { get; set; }
    public HitState hitState { get; set; }

    // items
    public List<string> items { get; set; }

    // stats
    public int cyberlegsLevel;
    public bool cyberEyesThermal;
    public bool thirdWeaponSlot;
    public bool cyberEyesThermalBuff;
    public Dictionary<GunType, int> gunSkillLevel = new Dictionary<GunType, int>{
        {GunType.pistol, 1},
        {GunType.smg, 1},
        {GunType.rifle, 1},
        {GunType.shotgun, 1},
        {GunType.sword, 1},
    };

    public int maxConcurrentNetworkHacks;
    public float hackSpeedCoefficient;
    public float hackRadius;

    public bool disguise;

    public static PlayerState DefaultGameData() {
        Gun gun1 = Gun.Load("smg");
        Gun gun2 = Gun.Load("pistol");
        Gun gun3 = Gun.Load("shotgun");

        return new PlayerState() {
            legSkin = "Jack",
            bodySkin = "Jack",

            primaryGun = new GunInstance(gun1),
            secondaryGun = new GunInstance(gun2),
            tertiaryGun = new GunInstance(gun3),
            activeGun = 2,

            items = new List<string> { "explosive", "deck", "goggles" },

            cyberlegsLevel = 1,
            maxConcurrentNetworkHacks = 1,
            hackSpeedCoefficient = 1f,
            hackRadius = 1.5f,
            thirdWeaponSlot = false,

            health = 250f,
            fullHealthAmount = 250f,

        };
    }

    public void ApplyState(GameObject playerObject) {
        ((IGunHandlerState)this).ApplyGunState(playerObject);
        ((ISkinState)this).ApplySkinState(playerObject);
        ((IItemHandlerState)this).ApplyItemState(playerObject);
        ((ICharacterHurtableState)this).ApplyHurtableState(playerObject);
        ApplyPlayerState(playerObject);
    }
    public void ApplyPlayerState(GameObject playerObject) {
        foreach (IPlayerStateLoader loader in playerObject.GetComponentsInChildren<IPlayerStateLoader>()) {
            loader.LoadState(this);
        }
    }

    public PlayerState Refresh() {
        GunInstance newPrimary = primaryGun;
        GunInstance newSecondary = secondaryGun;
        GunInstance newTertiary = tertiaryGun;
        if (primaryGun != null && primaryGun.baseGun != null) {
            newPrimary = new GunInstance(primaryGun.baseGun);
        }
        if (secondaryGun != null && secondaryGun.baseGun != null) {
            newSecondary = new GunInstance(secondaryGun.baseGun);
        }
        if (tertiaryGun != null && tertiaryGun.baseGun != null) {
            newTertiary = new GunInstance(tertiaryGun.baseGun);
        }
        return this with {
            health = fullHealthAmount,
            hitState = HitState.normal,
            primaryGun = newPrimary,
            secondaryGun = newSecondary,
            tertiaryGun = newTertiary
        };
    }
}

// legSkin = "generic64",
// // legSkin = "cyber",
// legSkin = "civ_male",
// bodySkin = "civ_female",
