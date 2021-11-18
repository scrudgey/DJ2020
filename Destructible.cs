using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : IDestructible {
    public GameObject[] destructionFx;
    public AudioClip[] destructSounds;
    public DamageResult TakeBulletDamage(BulletDamage impact) {
        return new DamageResult {
            damageAmount = impact.bullet.damage
        };
    }

    public DamageResult TakeExplosiveDamage(ExplosionDamage explosion) {
        return new DamageResult {
            damageAmount = explosion.amount
        };
    }
    void Awake() {
        RegisterDamageCallback<BulletDamage>(TakeBulletDamage);
        RegisterDamageCallback<ExplosionDamage>(TakeExplosiveDamage);
    }

    override protected void Destruct(Damage damage) {
        // TODO: don't duplicate this code.
        Collider myCollider = GetComponentInChildren<Collider>();
        gibs?.Emit(gameObject, damage, myCollider);

        Destroy(transform.parent.gameObject, 5f);
        foreach (GameObject fx in destructionFx) {
            GameObject.Instantiate(fx, transform.position, Quaternion.identity);
        }
        if (destructSounds.Length > 0) {
            Toolbox.AudioSpeaker(transform.position, destructSounds);
        }

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) {
            Vector3 force = 50f * Random.onUnitSphere;
            force.y = Mathf.Abs(force.y);
            rigidbody.AddForce(force, ForceMode.Impulse);
        }

        TagSystemData data = Toolbox.GetTagData(gameObject);
        data.dontHideAbove = true;

        Destroy(this);
        // Debug.Break();
    }
}
