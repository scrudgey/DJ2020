using UnityEngine;

public interface IGunHandlerTemplate {
    // gun
    public GunTemplate primaryGun { get; set; }
    public GunTemplate secondaryGun { get; set; }
    public GunTemplate tertiaryGun { get; set; }
    // public void ApplyGunState(GameObject playerObject) {
    //     foreach (IGunHandlerStateLoader gunLoader in playerObject.GetComponentsInChildren<IGunHandlerStateLoader>()) {
    //         gunLoader.LoadGunHandlerState(this);
    //     }
    // }
}