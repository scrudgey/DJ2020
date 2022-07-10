using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : Destructible {
    Collider myCollider;
    override public void Awake() {
        base.Awake();
        myCollider = GetComponent<Collider>();
        RegisterDamageCallback<ExplosionDamage>(TakeExplosionDamage);
    }

    public DamageResult TakeExplosionDamage(ExplosionDamage damage) {
        return new DamageResult {
            damageAmount = damage.amount
        };
    }
}
