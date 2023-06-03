using System;
using UnityEngine;

[System.Serializable]
public class DamageConditional {
    public enum DamageType { any, none, bullet, explosion, electric }
    public DamageType type;

    public bool ConditionIsMet(Damage damage) {
        switch (type) {
            default:
            case DamageType.any:
                return true;
            case DamageType.none:
                return false;
            case DamageType.bullet:
                return damage is BulletDamage;
            case DamageType.explosion:
                return damage is ExplosionDamage;
            case DamageType.electric:
                return damage is ElectricalDamage;
        }
    }
}