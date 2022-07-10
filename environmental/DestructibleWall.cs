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
        RegisterDamageCallback<ExplosionDamage>(DoTakeDamage);
    }

    public DamageResult DoTakeDamage(ExplosionDamage damage) {
        // Vector3 myPosition = transform.position;
        // if (myCollider != null) {
        //     myPosition = myCollider.bounds.center;
        // }
        // Vector3 force = damage.force;
        return new DamageResult {
            damageAmount = damage.amount
        };
    }
}
