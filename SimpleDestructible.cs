using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDestructible : Destructible {
    public float bulletDamageModifier = 1f;
    public float explosionDamageModifier = 1f;
    public DamageResult TakeBulletDamage(BulletDamage impact) {
        return new DamageResult {
            damageAmount = impact.bullet.damage * bulletDamageModifier,
            damage = impact
        };
    }
    public DamageResult TakeExplosiveDamage(ExplosionDamage explosion) {
        return new DamageResult {
            damageAmount = explosion.amount * explosionDamageModifier,
            damage = explosion
        };
    }
    public override void Awake() {
        base.Awake();
        RegisterDamageCallback<BulletDamage>(TakeBulletDamage);
        RegisterDamageCallback<ExplosionDamage>(TakeExplosiveDamage);
    }

    override protected void DoDestruct(Damage damage) {
        base.DoDestruct(damage);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) {
            Vector3 force = 50f * Random.onUnitSphere;
            force.y = Mathf.Abs(force.y);
            rigidbody.AddForce(force, ForceMode.Impulse);
        }
        TagSystemData data = Toolbox.GetTagData(gameObject);
        // data.dontHideAbove = true;
        Destroy(this);
    }
}
