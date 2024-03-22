using Newtonsoft.Json;
using UnityEngine;
public enum WeaponType { none, gun, melee }
public class WeaponState {
    public WeaponType type;
    public GunState gunInstance;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<MeleeWeaponTemplate>))]
    public MeleeWeaponTemplate meleeWeapon;

    public Sprite GetSprite() {
        if (type == WeaponType.gun) {
            return gunInstance.GetSprite();
        } else {
            return meleeWeapon.sprite;
        }
    }
    public string GetName() {
        if (type == WeaponType.gun) {
            return gunInstance.getName();
        } else {
            return meleeWeapon.name;
        }
    }
    public string GetShortName() {
        if (type == WeaponType.gun) {
            return gunInstance.getShortName();
        } else {
            return meleeWeapon.name;
        }
    }
    public void ResetTemporaryState() {
        if (gunInstance != null) {
            gunInstance.delta.chamber = 0;
            gunInstance.ClipIn();
        }
    }
    // public WeaponState() { }
    public WeaponState(GunState gunInstance) {
        this.gunInstance = gunInstance;
        type = WeaponType.gun;
    }
    public WeaponState(MeleeWeaponTemplate meleeWeapon) {
        this.meleeWeapon = meleeWeapon;
        type = WeaponType.melee;
    }
}