using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public record PlayerTemplate : ISkinState, IGunHandlerTemplate, IItemHandlerState, ICharacterHurtableState {
    public int credits;

    // skin
    public string legSkin { get; set; }
    public string bodySkin { get; set; }

    // gun
    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate primaryGun { get; set; }

    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate secondaryGun { get; set; }

    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate tertiaryGun { get; set; }
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

    public SpeechEtiquette[] etiquettes;

    public static PlayerTemplate Default() => new PlayerTemplate() {
        legSkin = "Jack",
        bodySkin = "Jack",

        primaryGun = GunTemplate.Load("s1"),
        secondaryGun = GunTemplate.Load("p1"),
        tertiaryGun = GunTemplate.Load("sh1"),
        activeGun = 2,

        items = new List<string> { "explosive", "deck", "goggles" },

        cyberlegsLevel = 1,
        maxConcurrentNetworkHacks = 1,
        hackSpeedCoefficient = 1f,
        hackRadius = 1.5f,
        thirdWeaponSlot = false,

        health = 250f,
        fullHealthAmount = 250f,

        etiquettes = new SpeechEtiquette[] { SpeechEtiquette.street }
    };
}
