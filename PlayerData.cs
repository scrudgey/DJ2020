public class PlayerData {
    // skin
    public string legSkin;
    public string bodySkin;

    // gun
    public GunInstance primaryGun;
    public GunInstance secondaryGun;
    public GunInstance tertiaryGun;
    public int activeGun;

    // stats
    public int cyberlegsLevel;

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

            cyberlegsLevel = 1
        };
    }
}