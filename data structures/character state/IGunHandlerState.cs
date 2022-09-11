using UnityEngine;

public interface IGunHandlerState {

    // gun
    public GunInstance primaryGun { get; set; }
    public GunInstance secondaryGun { get; set; }
    public GunInstance tertiaryGun { get; set; }
    public int activeGun { get; set; }

    public void ApplyGunState(GameObject playerObject) {
        foreach (IGunHandlerStateLoader gunLoader in playerObject.GetComponentsInChildren<IGunHandlerStateLoader>()) {
            gunLoader.LoadGunHandlerState(this);
        }
    }
}