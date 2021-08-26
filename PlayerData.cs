public class PlayerData {
    // skin
    public string legSkin;
    public string bodySkin;

    // gun
    public GunInstance primaryGun;
    public GunInstance secondaryGun;
    public GunInstance tertiaryGun;
    public int activeGun;



    public static PlayerData TestInitialData() {
        return new PlayerData() {

            // gunHandler.primary = new GunInstance(Gun.Load("rifle"));
            // gunHandler.third = new GunInstance(Gun.Load("smg"));

            primaryGun = new GunInstance(Gun.Load("smg")),
            secondaryGun = new GunInstance(Gun.Load("pistol")),
            tertiaryGun = new GunInstance(Gun.Load("shotgun")),
            activeGun = 2,

            legSkin = "generic",
            bodySkin = "generic"
        };
    }
}