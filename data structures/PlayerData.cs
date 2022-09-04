using System.Collections.Generic;

public class PlayerData {
    public int credits;

    // skin
    public string legSkin;
    public string bodySkin;

    // gun
    public GunInstance primaryGun;
    public GunInstance secondaryGun;
    public GunInstance tertiaryGun;
    public int activeGun;

    // items
    public List<string> items = new List<string>();

    // stats
    public int cyberlegsLevel;
    public bool cyberEyesThermal;
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

    public static PlayerData DefaultGameData() {
        Gun gun1 = Gun.Load("smg");
        Gun gun2 = Gun.Load("pistol");
        Gun gun3 = Gun.Load("shotgun");

        // gun1.silencer = true;
        // gun2.silencer = true;
        return new PlayerData() {
            primaryGun = new GunInstance(gun1),
            secondaryGun = new GunInstance(gun2),
            tertiaryGun = new GunInstance(gun3),
            activeGun = 2,

            // legSkin = "generic64",
            // // legSkin = "cyber",
            // bodySkin = "generic64",

            legSkin = "Jack",
            bodySkin = "Jack",

            // legSkin = "civ_male",
            // bodySkin = "civ_male",

            // legSkin = "civ_female",
            // bodySkin = "civ_female",

            cyberlegsLevel = 1,

            items = new List<string> { "explosive", "deck", "goggles" },

            maxConcurrentNetworkHacks = 1,
            hackSpeedCoefficient = 1f,
            hackRadius = 1.5f
        };
    }
}