using System.Collections.Generic;

public class PlayerData {
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
    public Dictionary<GunType, int> gunSkillLevel = new Dictionary<GunType, int>{
        {GunType.pistol, 1},
        {GunType.smg, 1},
        {GunType.rifle, 1},
        {GunType.shotgun, 1},
        {GunType.sword, 1},
    };

    public static PlayerData TestInitialData() {
        return new PlayerData() {

            primaryGun = new GunInstance(Gun.Load("rifle")),
            // primaryGun = new GunInstance(Gun.Load("smg")),
            secondaryGun = new GunInstance(Gun.Load("pistol")),
            tertiaryGun = new GunInstance(Gun.Load("shotgun")),
            activeGun = 2,

            legSkin = "generic",
            // legSkin = "cyber",
            bodySkin = "generic",

            cyberlegsLevel = 1,

            items = new List<string> { "deck", "explosive" }
        };
    }
}