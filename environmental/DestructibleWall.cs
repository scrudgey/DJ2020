using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : IDamageable {
    Collider myCollider;
    void Awake() {
        myCollider = GetComponent<Collider>();
        if (gibs != null)
            foreach (Gib gib in gibs.gibs) {
                PoolManager.I.RegisterPool(gib.prefab);
            }

        // damageHandlers.Add(DoTakeDamage);
        RegisterDamageCallback<ExplosionDamage>(DoTakeDamage);
        // handlers += DoTakeDamage;
    }

    public DamageResult DoTakeDamage(ExplosionDamage damage) {
        Vector3 myPosition = transform.position;
        if (myCollider != null) {
            myPosition = myCollider.bounds.center;
        }
        Vector3 force = damage.GetDamageAtPoint(myPosition);
        // health -= force.magnitude;
        return new DamageResult {
            damageAmount = force.magnitude
        };
    }

    // public void Destruct(Explosion explosion) {
    //     Destroy(transform.parent.gameObject);
    //     Collider myCollider = GetComponentInChildren<Collider>();
    //     if (gibs != null) {
    //         foreach (Gib gib in gibs.gibs) {
    //             gib.Emit(explosion, myCollider);
    //         }
    //     }
    // }
}
