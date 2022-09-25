using UnityEngine;

public interface IGunHandlerState {

    // gun
    public GunState primaryGun { get; set; }
    public GunState secondaryGun { get; set; }
    public GunState tertiaryGun { get; set; }
    public int activeGun { get; set; }

    public void ApplyGunState(GameObject playerObject) {
        foreach (IGunHandlerStateLoader gunLoader in playerObject.GetComponentsInChildren<IGunHandlerStateLoader>()) {
            gunLoader.LoadGunHandlerState(this);
        }
    }
}