using System.Collections.Generic;
using System.Linq;
using Items;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public record PlayerState : ISkinState, ICharacterHurtableState, PerkIdConstants { //IGunHandlerState
    public int credits;
    public int favors;

    [JsonConverter(typeof(ObjectListJsonConverter<LootData>))]
    public List<LootData> loots;
    public List<PayData> payDatas;
    [JsonConverter(typeof(ObjectListJsonConverter<Tactic>))]
    public List<Tactic> unlockedTactics;

    // skin
    public string legSkin { get; set; }
    public string bodySkin { get; set; }
    public string headSkin { get; set; }

    // gun
    public List<WeaponState> allGuns;
    [JsonConverter(typeof(ObjectListJsonConverter<ItemTemplate>))]
    public List<ItemTemplate> allItems;
    public WeaponState primaryGun { get; set; }
    public WeaponState secondaryGun { get; set; }
    public WeaponState tertiaryGun { get; set; }
    public int activeGun { get; set; }
    public int numberOfShellsPerReload() {
        return PerkNumberOfShellsPerReload();
    }

    // health
    public float health { get; set; }
    public float fullHealthAmount() {
        return PerkFullHealthAmount();
    }
    public HitState hitState { get; set; }
    public int armorLevel { get; set; }


    // stats
    public int cyberlegsLevel;
    public bool cyberEyesThermal;
    public bool cyberEyesThermalBuff;
    public int maxConcurrentNetworkHacks;
    public float hackSpeedCoefficient;
    public float hackRadius;

    public SpeechEtiquette[] etiquettes;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite portrait;


    public List<string> activePerks;
    public int skillpoints;
    public int bodySkillPoints;
    public int gunSkillPoints;
    public int hackSkillPoints;
    public int speechSkillPoints;

    public List<SoftwareTemplate> softwareTemplates;
    public List<SoftwareState> softwareStates;

    public static PlayerState DefaultState() {
        GunTemplate gun1 = GunTemplate.Load("p1");
        GunTemplate gun2 = GunTemplate.Load("s1");
        GunTemplate gun3 = GunTemplate.Load("r1");
        GunTemplate gun4 = GunTemplate.Load("sh1");
        GunTemplate gun5 = GunTemplate.Load("s3");
        GunTemplate gun6 = GunTemplate.Load("p3");
        GunTemplate gun7 = GunTemplate.Load("p4");
        GunTemplate gun8 = GunTemplate.Load("r2");

        MeleeWeaponTemplate swordTemplate = MeleeWeaponTemplate.Load("sword");

        GunState gunState1 = GunState.Instantiate(gun1);
        GunState gunState2 = GunState.Instantiate(gun2);
        GunState gunState3 = GunState.Instantiate(gun3);
        GunState gunState4 = GunState.Instantiate(gun4);
        GunState gunState5 = GunState.Instantiate(gun5);
        GunState gunState6 = GunState.Instantiate(gun6);
        GunState gunState7 = GunState.Instantiate(gun7);
        GunState gunState8 = GunState.Instantiate(gun8);

        GunMod silencer = Resources.Load("data/guns/mods/silencer") as GunMod;
        gunState1.delta.activeMods.Add(silencer);

        WeaponState sword = new WeaponState(swordTemplate);

        List<WeaponState> allGuns = new List<WeaponState> {
            new WeaponState( gunState1),
            new WeaponState( gunState2),
            new WeaponState( gunState3),
            new WeaponState( gunState4),
            new WeaponState( gunState5),
            new WeaponState( gunState6),
            new WeaponState( gunState7),
            new WeaponState( gunState8),
            sword
        };


        List<ItemTemplate> allItems = new List<ItemTemplate> {
            // ItemTemplate.LoadItem("C4"),
            // ItemTemplate.LoadItem("rocket"),
            // ItemTemplate.LoadItem("goggles"),
            ItemTemplate.LoadItem("grenade"),
            ItemTemplate.LoadItem("deck"),
        };

        // List<LootData> loots = new List<LootData> {
        //     Resources.Load("data/loot/drug/rush") as LootData,
        //     Resources.Load("data/loot/drug/rush") as LootData,
        //     Resources.Load("data/loot/drug/rush") as LootData,
        //     Resources.Load("data/loot/drug/vial") as LootData,
        //     Resources.Load("data/loot/drug/vial") as LootData,
        //     Resources.Load("data/loot/drug/zyme") as LootData,
        //     Resources.Load("data/loot/industrial/deuteriumOxide") as LootData,
        //     Resources.Load("data/loot/industrial/tungstenRod") as LootData,
        //     Resources.Load("data/loot/industrial/flask") as LootData,
        // };

        List<LootData> loots = new List<LootData>();
        // List<string> perks = new List<string>{
        //     "p1_1",
        //     "p1_2",
        //     // "p1_3",
        //     "sh1_1",
        //     "sh1_2",
        //     // "sh1_3",
        //     "p2_1",
        //     "p2_2",
        //     // "p2_3",
        //     "sh2_1",
        //     "sh2_2",
        //     // "sh2_3",
        //     "rifle1_1",
        //     "rifle1_2",
        //     // "rifle1_3",
        //     "smg1_1",
        //     "smg1_2",
        //     // "smg1_3",
        //     "rifle2_1",
        //     "stall",
        //     "ease"
        //     // "smg2"
        // };
        List<string> perks = new List<string>();

        List<SoftwareScriptableTemplate> softwareScriptableTemplates = new List<SoftwareScriptableTemplate>{
            SoftwareScriptableTemplate.Load("scan"),
            SoftwareScriptableTemplate.Load("crack"),
            SoftwareScriptableTemplate.Load("exploit"),
            SoftwareScriptableTemplate.Load("scanData"),
            SoftwareScriptableTemplate.Load("scanEdges"),
            SoftwareScriptableTemplate.Load("scanNode"),
        };
        List<SoftwareTemplate> softwareTemplates = softwareScriptableTemplates.Select(template => template.ToTemplate()).ToList();

        List<SoftwareState> softwareStates = softwareTemplates.Select(template => new SoftwareState(template)).ToList();

        List<Tactic> tactis = new List<Tactic>{
            Resources.Load("data/tactics/cyberattack") as Tactic
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
            // primaryGun = sword,
            primaryGun = new WeaponState(gunState1),
            // secondaryGun = null,
            // secondaryGun = new WeaponState(gunState2),
            secondaryGun = sword,
            // tertiaryGun = null,
            tertiaryGun = new WeaponState(gunState3),
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

            // physicalKeys = new HashSet<int>(),
            // keycards = new HashSet<int>(),

            payDatas = new List<PayData>(),

            credits = 9650,
            favors = 7,
            // credits = 600,
            loots = loots,

            // activePerks = new List<string>(),
            activePerks = perks,
            skillpoints = 0,

            softwareTemplates = softwareTemplates,
            softwareStates = softwareStates,
            unlockedTactics = tactis
        };
    }

    public void ApplyState(GameObject playerObject) {
        // ((IGunHandlerState)this).ApplyGunState(playerObject);
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
        primaryGun = new WeaponState(GunState.Instantiate(template.primaryGun)),
        secondaryGun = new WeaponState(GunState.Instantiate(template.secondaryGun)),
        tertiaryGun = new WeaponState(GunState.Instantiate(template.tertiaryGun)),
        health = template.fullHealthAmount,
        cyberEyesThermal = template.cyberEyesThermal,
        cyberlegsLevel = template.cyberlegsLevel,
        legSkin = template.legSkin,
        bodySkin = template.bodySkin,
        headSkin = template.headSkin,
        etiquettes = template.etiquettes,
        portrait = template.portrait,
        armorLevel = template.armorLevel
    };

    public void ResetTemporaryState(LevelPlan plan) {
        primaryGun?.ResetTemporaryState();
        secondaryGun?.ResetTemporaryState();
        tertiaryGun?.ResetTemporaryState();

        // softwareStates = softwareTemplates.Select(template => new SoftwareState(template)).ToList();
        softwareStates = plan.softwareTemplates.Select(template => new SoftwareState(template)).ToList();
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
    public int PerkGunAccuracyLevel(GunType gunType) {
        return gunType switch {
            GunType.pistol => GetPerkLevel(PerkIdConstants.PERKID_PISTOL_ACC),
            GunType.smg => GetPerkLevel(PerkIdConstants.PERKID_SMG_ACC),
            GunType.shotgun => GetPerkLevel(PerkIdConstants.PERKID_SHOTGUN_ACC),
            GunType.rifle => GetPerkLevel(PerkIdConstants.PERKID_RIFLE_ACC)
        };
    }
    public int PerkGunControlLevel(GunType gunType) {
        return gunType switch {
            GunType.pistol => GetPerkLevel(PerkIdConstants.PERKID_PISTOL_CONTROL),
            GunType.smg => GetPerkLevel(PerkIdConstants.PERKID_SMG_CONTROL),
            GunType.shotgun => GetPerkLevel(PerkIdConstants.PERKID_SHOTGUN_CONTROL),
            GunType.rifle => GetPerkLevel(PerkIdConstants.PERKID_RIFLE_CONTROL)
        };
    }
    public int PerkSpeechlevel() {
        return GetPerkLevel(PerkIdConstants.PERKID_SPEECH);
    }
    public bool PerkThirdWeaponSlot() {
        return PerkIsActivated(PerkIdConstants.PERKID_THIRD_GUN);
    }
    public bool PerkTargetLockOnHead() {
        return PerkIsActivated(PerkIdConstants.PERKID_PISTOL_LOCK_HEAD);
    }
    public int PerkNumberOfItemSlots() {
        if (PerkIsActivated(PerkIdConstants.PERKID_ITEM_SLOT)) {
            return 5;
        } else {
            return 4;
        }
    }
    public int PerkLockpickLevel() {
        return GetPerkLevel(PerkIdConstants.PERKID_LOCKPICK);
    }

    public DialogueCard NewDialogueCard() {
        List<DialogueTacticType> tacticTypes = new List<DialogueTacticType>() { DialogueTacticType.lie, DialogueTacticType.deny };

        // check for unlocked perks
        if (PerkIsActivated(PerkIdConstants.PERKID_SPEECH_CHALLENGE)) {
            tacticTypes.Add(DialogueTacticType.challenge);
        }
        if (PerkIsActivated(PerkIdConstants.PERKID_SPEECH_REDIRECT)) {
            tacticTypes.Add(DialogueTacticType.redirect);
        }

        DialogueTacticType tacticType = Toolbox.RandomFromList(tacticTypes);
        // int baseValue = (int)Toolbox.RandomGaussian(minValue: 5, maxValue: 50);
        int baseValue = Random.Range(5, 50);
        DialogueCard newCard = new DialogueCard() {
            type = tacticType,
            baseValue = baseValue
        };
        return newCard;
    }

    public List<Tactic> TacticsAvailableToUnlock() {
        Tactic[] allTactics = Resources.LoadAll<Tactic>("data/tactics") as Tactic[];
        return allTactics.Where(tactic => !unlockedTactics.Contains(tactic)).ToList();
    }
}
