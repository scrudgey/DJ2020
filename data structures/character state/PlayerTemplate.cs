using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public record PlayerTemplate : ISkinState, IGunHandlerTemplate, ICharacterHurtableState {
    public int credits;

    // skin
    [field: SerializeField]
    public string legSkin { get; set; }
    [field: SerializeField]
    public string bodySkin { get; set; }
    [field: SerializeField]
    public string headSkin { get; set; }

    // gun
    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate primaryGun { get; set; }

    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate secondaryGun { get; set; }

    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate tertiaryGun { get; set; }
    [field: SerializeField]
    public int activeGun { get; set; }

    // health
    [field: SerializeField]
    public float health { get; set; }
    [field: SerializeField]
    public float fullHealthAmount { get; set; }
    [field: SerializeField]
    public HitState hitState { get; set; }

    // items
    // public List<string> items { get; set; }

    // stats
    public int cyberlegsLevel;
    public bool cyberEyesThermal;
    public bool thirdWeaponSlot;
    public bool cyberEyesThermalBuff;
    public SerializableDictionary<GunType, int> gunSkillLevel = new SerializableDictionary<GunType, int>{
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

    // speech
    [field: SerializeField]
    public SpeechEtiquette[] etiquettes;

    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite portrait;

    public static PlayerTemplate Default() {
        Sprite jackPortrait = Resources.Load<Sprite>("sprites/portraits/Jack") as Sprite;

        return new PlayerTemplate() {
            legSkin = "Jack",
            bodySkin = "Jack",
            headSkin = "Jack",

            primaryGun = GunTemplate.Load("s1"),
            secondaryGun = GunTemplate.Load("p1"),
            tertiaryGun = GunTemplate.Load("sh1"),
            activeGun = 2,

            // items = new List<string> { "explosive", "deck", "goggles", "ID", "tools" },

            cyberlegsLevel = 1,
            maxConcurrentNetworkHacks = 1,
            hackSpeedCoefficient = 1f,
            hackRadius = 1.5f,
            thirdWeaponSlot = false,

            health = 250f,
            fullHealthAmount = 250f,

            etiquettes = new SpeechEtiquette[] { SpeechEtiquette.street },

            portrait = jackPortrait
        };
    }
}
