using UnityEngine;

public class IDestructible : IDamageable {
    public Gibs gibs;
    public float health;

    override public void TakeDamage<T>(T damage) {
        base.TakeDamage(damage);
        if (health <= 0) {
            Destruct(damage);
        }
    }
    protected override void ApplyDamageResult(DamageResult result) {
        base.ApplyDamageResult(result);
        health -= result.damageAmount;
    }

    virtual protected void Destruct(Damage damage) {
        Collider myCollider = GetComponentInChildren<Collider>();
        if (gibs != null) {
            foreach (Gib gib in gibs.gibs) {
                gib.Emit(damage, myCollider);
            }
        }
        Destroy(transform.parent.gameObject);
    }
}