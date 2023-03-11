using System.Collections.Generic;
using Items;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public record PlayerState : ISkinState, IGunHandlerState, ICharacterHurtableState {
    public int credits;

    [JsonConverter(typeof(ObjectListJsonConverter<LootData>))]
    public List<LootData> loots;

    [JsonConverter(typeof(ObjectListJsonConverter<PayData>))]
    public List<PayData> payDatas;

    // skin
    public string legSkin { get; set; }
    public string bodySkin { get; set; }
    public string headSkin { get; set; }

    // gun
    public List<GunState> allGuns;
    public List<BaseItem> allItems;
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
    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite portrait;

    public HashSet<int> physicalKeys;



    public static PlayerState DefaultState() {
        GunTemplate gun1 = GunTemplate.Load("p1");
        // GunTemplate gun2 = GunTemplate.Load("s1");
        // GunTemplate gun3 = GunTemplate.Load("r1");
        // GunTemplate gun4 = GunTemplate.Load("p2");

        GunState gunState1 = GunState.Instantiate(gun1);
        // GunState gunState2 = GunState.Instantiate(gun2);
        // GunState gunState3 = GunState.Instantiate(gun3);
        // GunState gunState4 = GunState.Instantiate(gun4);

        List<GunState> allGuns = new List<GunState>{
            gunState1,
            // gunState2,
            // gunState3,
            // gunState4
        };

        List<BaseItem> allItems = new List<BaseItem> {
            ItemInstance.LoadItem("deck"),
            ItemInstance.LoadItem("tools"),
            // ItemInstance.LoadItem("C4")
        };

        return new PlayerState() {
            legSkin = "Jack",
            bodySkin = "Jack",
            headSkin = "Jack",

            // legSkin = "swat",
            // bodySkin = "swat",
            // headSkin = "swat",

            // legSkin = "security",
            // bodySkin = "security",
            // headSkin = "security",
            allGuns = allGuns,
            allItems = allItems,
            primaryGun = gunState1,
            secondaryGun = null,
            tertiaryGun = null,
            activeGun = -1,

            // items = new List<string> { "explosive", "deck", "goggles", "tools" },

            cyberlegsLevel = 1,
            maxConcurrentNetworkHacks = 1,
            hackSpeedCoefficient = 1f,
            hackRadius = 1.5f,
            thirdWeaponSlot = false,

            health = 150f,
            fullHealthAmount = 150f,

            speechSkillLevel = 3,
            etiquettes = new SpeechEtiquette[] { SpeechEtiquette.street },
            portrait = Resources.Load<Sprite>("sprites/portraits/Jack") as Sprite,

            physicalKeys = new HashSet<int>(),

            payDatas = new List<PayData>(),

            credits = 10000,
            loots = new List<LootData>()
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
