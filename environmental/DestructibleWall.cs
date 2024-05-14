using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class DestructibleWall : Destructible {
    override public void Awake() {
        base.Awake();
        RegisterDamageCallback<ExplosionDamage>(TakeExplosionDamage);
    }

    public DamageResult TakeExplosionDamage(ExplosionDamage damage) {
        return new DamageResult {
            damageAmount = damage.amount,
            damage = damage
        };
    }

    protected override void DoDestruct(Damage damage) {
        base.DoDestruct(damage);
        GameManager.I.RebuildNavMeshAsync();
    }
}
