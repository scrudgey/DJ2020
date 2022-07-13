using System;
using UnityEngine;

[System.Serializable]
public class DamageResultConditional {
    public DamageConditional.DamageType type;

    public bool ConditionIsMet(DamageResult result) {
        switch (type) {
            default:
            case DamageConditional.DamageType.any:
                return true;
            case DamageConditional.DamageType.none:
                return false;
            case DamageConditional.DamageType.bullet:
                return result.damage is BulletDamage;
            case DamageConditional.DamageType.explosion:
                return result.damage is ExplosionDamage;
        }
    }
}