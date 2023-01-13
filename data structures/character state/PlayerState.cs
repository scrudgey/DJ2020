using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public record PlayerState : ISkinState, IGunHandlerState, ICharacterHurtableState {
    public int credits;

    // skin
    public string legSkin { get; set; }
    public string bodySkin { get; set; }
    public string headSkin { get; set; }

    // gun
    public GunState primaryGun { get; set; }
    public GunState secondaryGun { get; set; }
    public GunState tertiaryGun { get; set; }
    public int activeGun { get; set; }

    // health
    public float health { get; set; }
    public float fullHealthAmount { get; set; }
    public HitState hitState { get; set; }

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

    public int speechSkillLevel;
    public int maxConcurrentNetworkHacks;
    public float hackSpeedCoefficient;
    public float hackRadius;


    public SpeechEtiquette[] etiquettes;
    public Sprite portrait;

    public HashSet<int> physicalKeys;

    public List<PayData> payDatas;


    public static PlayerState DefaultState() {
        GunTemplate gun1 = GunTemplate.Load("s1");
        GunTemplate gun2 = GunTemplate.Load("p1");
        GunTemplate gun3 = GunTemplate.Load("sh1");

        return new PlayerState() {
            legSkin = "Jack",
            bodySkin = "Jack",
            headSkin = "Jack",

            primaryGun = GunState.Instantiate(gun1),
            secondaryGun = GunState.Instantiate(gun2),
            tertiaryGun = GunState.Instantiate(gun3),
            activeGun = -1,

            // items = new List<string> { "explosive", "deck", "goggles", "tools" },

            cyberlegsLevel = 1,
            maxConcurrentNetworkHacks = 1,
            hackSpeedCoefficient = 1f,
            hackRadius = 1.5f,
            thirdWeaponSlot = false,

            health = 100f,
            fullHealthAmount = 100f,

            speechSkillLevel = 3,
            etiquettes = new SpeechEtiquette[] { SpeechEtiquette.street },
            portrait = Resources.Load<Sprite>("sprites/portraits/Jack") as Sprite,

            physicalKeys = new HashSet<int>(),

            payDatas = new List<PayData>(),

            credits = 10000
        };
    }

    public void ApplyState(GameObject playerObject) {
        ((IGunHandlerState)this).ApplyGunState(playerObject);
        ((ISkinState)this).ApplySkinState(playerObject);
        // ((IItemHandlerState)this).ApplyItemState(playerObject);
        ((ICharacterHurtableState)this).ApplyHurtableState(playerObject);
        ApplyPlayerState(playerObject);
    }
    public void ApplyPlayerState(GameObject playerObject) {
        foreach (IPlayerStateLoader loader in playerObject.GetComponentsInChildren<IPlayerStateLoader>()) {
            loader.LoadState(this);
        }
    }

    public static PlayerState Instantiate(PlayerTemplate template) => DefaultState() with {
        primaryGun = GunState.Instantiate(template.primaryGun),
        secondaryGun = GunState.Instantiate(template.secondaryGun),
        tertiaryGun = GunState.Instantiate(template.tertiaryGun),
        health = template.fullHealthAmount,
        cyberEyesThermal = template.cyberEyesThermal,
        cyberlegsLevel = template.cyberlegsLevel,
        thirdWeaponSlot = template.thirdWeaponSlot,
        legSkin = template.legSkin,
        bodySkin = template.bodySkin,
        headSkin = template.headSkin,
        etiquettes = template.etiquettes,
        portrait = template.portrait
    };
}
