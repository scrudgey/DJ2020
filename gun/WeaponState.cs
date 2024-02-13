using UnityEngine;
public enum WeaponType { none, gun, melee }
public class WeaponState {
    public WeaponType type;
    public GunState gunInstance;

    public Sprite GetSprite() {
        if (type == WeaponType.gun) {
            return gunInstance.GetSprite();
        } else {
            return null; // TODO:
        }
    }
    public string GetName() {
        if (type == WeaponType.gun) {
            return gunInstance.getName();
        } else {
            return "sword"; // TODO:
        }
    }
    public string GetShortName() {
        if (type == WeaponType.gun) {
            return gunInstance.getShortName();
        } else {
            return "sword"; // TODO:
        }
    }
    public void ResetTemporaryState() {
        if (gunInstance != null) {
            gunInstance.delta.chamber = 0;
            gunInstance.ClipIn();
        }
    }
    public WeaponState() { }
    public WeaponState(GunState gunInstance) {
        this.gunInstance = gunInstance;
        type = WeaponType.gun;
    }
}