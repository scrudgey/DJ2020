using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : Destructible {
    Collider myCollider;
    void Awake() {
        myCollider = GetComponent<Collider>();
        if (gibs != null)
            foreach (Gib gib in gibs.gibs) {
                PoolManager.I.RegisterPool(gib.prefab);
            }
        RegisterDamageCallback<ExplosionDamage>(TakeExplosionDamage);
    }

    public DamageResult TakeExplosionDamage(ExplosionDamage damage) {
        return new DamageResult {
            damageAmount = damage.amount
        };
    }
}
