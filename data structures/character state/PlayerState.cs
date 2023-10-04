using System.Collections.Generic;
using Items;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public record PlayerState : ISkinState, IGunHandlerState, ICharacterHurtableState, PerkIdConstants {
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
    public List<ItemTemplate> allItems;
    public GunState primaryGun { get; set; }
    public GunState secondaryGun { get; set; }
    public GunState tertiaryGun { get; set; }
    public int activeGun { get; set; }
    public int numberOfShellsPerReload {
        get {
            return PerkNumberOfShellsPerReload();
        }
    }

    // health
    public float health { get; set; }
    public float fullHealthAmount {
        get {
            return PerkFullHealthAmount();
        }
    }
    public HitState hitState { get; set; }

    // stats
    public int cyberlegsLevel;
    public bool cyberEyesThermal;
    // public bool thirdWeaponSlot;
    public bool cyberEyesThermalBuff;
    public Dictionary<GunType, int> gunSkillLevel = new Dictionary<GunType, int>{
        {GunType.pistol, 1},
        {GunType.smg, 1},
        {GunType.rifle, 1},
        {GunType.shotgun, 1},
        {GunType.sword, 1},
    };


    // TODO: remove these
    // public int speechSkillLevel;
    public int maxConcurrentNetworkHacks;
    public float hackSpeedCoefficient;
    public float hackRadius;


    public SpeechEtiquette[] etiquettes;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite portrait;

    public HashSet<int> physicalKeys;
    public HashSet<int> keycards;

    public HashSet<string> activePerks;
    public int skillpoints;
    public int bodySkillPoints;
    public int gunSkillPoints;
    public int hackSkillPoints;
    public int speechSkillPoints;

    public static PlayerState DefaultState() {
        GunTemplate gun1 = GunTemplate.Load("p1");
        GunTemplate gun2 = GunTemplate.Load("s1");
        GunTemplate gun3 = GunTemplate.Load("r1");
        // GunTemplate gun4 = GunTemplate.Load("p2");

        GunState gunState1 = GunState.Instantiate(gun1);
        GunState gunState2 = GunState.Instantiate(gun2);
        GunState gunState3 = GunState.Instantiate(gun3);
        // GunState gunState4 = GunState.Instantiate(gun4);

        GunMod silencer = Resources.Load("data/guns/mods/silencer") as GunMod;
        gunState1.delta.activeMods.Add(silencer);

        List<GunState> allGuns = new List<GunState> {
            gunState1,
            gunState2,
            // gunState3,
            // gunState4
        };

        List<ItemTemplate> allItems = new List<ItemTemplate> {
            // BaseItem.LoadItem("C4"),
            // BaseItem.LoadItem("rocket"),
            // BaseItem.LoadItem("goggles"),
        };

        List<LootData> loots = new List<LootData> {
            // Resources.Load("data/loot/drug/rush") as LootData,
            // Resources.Load("data/loot/drug/rush") as LootData,
            // Resources.Load("data/loot/drug/rush") as LootData,
            // Resources.Load("data/loot/drug/vial") as LootData,
            // Resources.Load("data/loot/drug/vial") as LootData,
            // Resources.Load("data/loot/drug/zyme") as LootData,
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
            // secondaryGun = null,
            secondaryGun = gunState2,
            // tertiaryGun = null,
            tertiaryGun = gunState3,
            activeGun = -1,

            // items = new List<string> { "explosive", "deck", "goggles", "tools" },

            cyberlegsLevel = 1,
            // cyberlegsLevel = 0,
            maxConcurrentNetworkHacks = 1,
            hackSpeedCoefficient = 1f,
            hackRadius = 1.5f,
            // thirdWeaponSlot = false,

            health = 150f,
            // fullHealthAmount = 150f,

            // speechSkillLevel = 3,
            etiquettes = new SpeechEtiquette[] { SpeechEtiquette.street },
            portrait = Resources.Load<Sprite>("sprites/portraits/Jack") as Sprite,

            physicalKeys = new HashSet<int>(),
            keycards = new HashSet<int>(),

            payDatas = new List<PayData>(),

            credits = 10000,
            // credits = 600,
            loots = loots,

            activePerks = new HashSet<string>(),
            skillpoints = 20
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
        legSkin = template.legSkin,
        bodySkin = template.bodySkin,
        headSkin = template.headSkin,
        etiquettes = template.etiquettes,
        portrait = template.portrait
    };

    public void ResetTemporaryState() {
        if (primaryGun != null) {
            primaryGun.delta.chamber = 0;
            primaryGun.ClipIn();
        }
        if (secondaryGun != null) {
            secondaryGun.delta.chamber = 0;
            secondaryGun.ClipIn();
        }
        if (tertiaryGun != null) {
            tertiaryGun.delta.chamber = 0;
            tertiaryGun.ClipIn();
        }
    }
    public int PlayerLevel() {
        return bodySkillPoints + gunSkillPoints + hackSkillPoints + speechSkillPoints;
    }
    public bool PerkIsFullyActivated(Perk perk) {
        if (perk.IsMultiStagePerk()) {
            return PerkLevelIsActivated(perk, perk.stages);
        } else {
            return PerkIsActivated(perk.perkId);
        }
    }
    public bool PerkIsActivated(string perkId) {
        return activePerks.Contains(perkId);
    }
    public bool PerkLevelIsActivated(Perk perk, int level) {
        string perkString = perk.PerkIdForLevel(level);
        return PerkIsActivated(perkString);
    }
    public bool PerkLevelIsActivated(string perkid, int level) {
        string perkString = Perk.PerkIdForLevel(perkid, level);
        return PerkIsActivated(perkString);
    }
    public int GetPerkLevel(Perk perk) {
        return GetPerkLevel(perk.perkId);
    }
    public int GetPerkLevel(string perkId) {
        int level = 0;

        bool containsNextLevel = PerkLevelIsActivated(perkId, level + 1);
        while (containsNextLevel) {
            level += 1;
            containsNextLevel = PerkLevelIsActivated(perkId, level + 1);
        }
        return level;
    }
    public void ActivatePerk(Perk perk) {
        skillpoints -= 1;
        switch (perk.category) {
            case PerkCategory.gun:
                gunSkillPoints += 1;
                break;
            case PerkCategory.hack:
                hackSkillPoints += 1;
                break;
            case PerkCategory.body:
                bodySkillPoints += 1;
                break;
            case PerkCategory.speech:
                speechSkillPoints += 1;
                break;
        }
        if (perk.IsMultiStagePerk()) {
            int level = GetPerkLevel(perk);
            activePerks.Add(perk.PerkIdForLevel(level + 1));
        } else {
            activePerks.Add(perk.perkId);
        }
        if (perk.perkId == PerkIdConstants.PERKID_HEALTH_1 || perk.perkId == PerkIdConstants.PERKID_HEALTH_2 || perk.perkId == PerkIdConstants.PERKID_HEALTH_3) {
            health = PerkFullHealthAmount();
        }
    }

    public int PerkScaledLootValue(LootData loot) {
        if (PerkIsActivated(Perk.PerkIdForLevel(PerkIdConstants.PERKID_BARGAIN, 3))) {
            return (int)(loot.value * 1.15f);
        } else if (PerkIsActivated(Perk.PerkIdForLevel(PerkIdConstants.PERKID_BARGAIN, 2))) {
            return (int)(loot.value * 1.1f);
        } else if (PerkIsActivated(Perk.PerkIdForLevel(PerkIdConstants.PERKID_BARGAIN, 1))) {
            return (int)(loot.value * 1.05f);
        } else return loot.value;
    }

    public int PerkNumberOfDailyDeals() {
        int dealLevels = GetPerkLevel(PerkIdConstants.PERKID_DEALMAKER);
        return 3 + dealLevels;
    }
    public bool PerkBonusMarketForces() {
        return PerkIsActivated(PerkIdConstants.PERKID_MARKET_CHAOS);
    }
    public int PerkNumberOfExplosives() {
        return PerkIsActivated(PerkIdConstants.PERKID_TWO_EXPLOSIVE) ? 2 : 1;
    }
    public int PerkNumberOfShellsPerReload() {
        return PerkIsActivated(PerkIdConstants.PERKID_TWO_SHELL) ? 2 : 1;
    }
    public int PerkBetterMarketConditionsChances() {
        return GetPerkLevel(PerkIdConstants.PERKID_MARKET);
    }
    public float PerkFullHealthAmount() {
        int totalLevel = 0;
        totalLevel += GetPerkLevel(PerkIdConstants.PERKID_HEALTH_1);
        totalLevel += GetPerkLevel(PerkIdConstants.PERKID_HEALTH_2);
        totalLevel += GetPerkLevel(PerkIdConstants.PERKID_HEALTH_3);
        return 150f + (totalLevel * 50f);
    }
    public int PerkSpeechlevel() {
        return GetPerkLevel(PerkIdConstants.PERKID_SPEECH);
    }
    public bool PerkThirdWeaponSlot() {
        return PerkIsActivated(PerkIdConstants.PERKID_THIRD_GUN);
    }
}
