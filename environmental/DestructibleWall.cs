using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : IDestructible {
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
        Vector3 myPosition = transform.position;
        if (myCollider != null) {
            myPosition = myCollider.bounds.center;
        }
        Vector3 force = damage.GetDamageAtPoint(myPosition);
        return new DamageResult {
            damageAmount = force.magnitude
        };
    }
}
