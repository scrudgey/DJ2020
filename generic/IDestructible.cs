using UnityEngine;

public class IDestructible : Damageable {
    public Gibs gibs;
    public float health;
    private bool doDestruct;
    override public void TakeDamage<T>(T damage) {
        base.TakeDamage(damage);
        if (health <= 0) {
            doDestruct = true;
        }
    }
    protected override void ApplyDamageResult(DamageResult result) {
        base.ApplyDamageResult(result);
        health -= result.damageAmount;
    }

    void FixedUpdate() {
        if (doDestruct) {
            doDestruct = false;
            Destruct(lastDamage);
        }
    }

    virtual protected void Destruct(Damage damage) {
        Collider myCollider = GetComponentInChildren<Collider>();
        gibs?.Emit(gameObject, damage, myCollider);
        Destroy(transform.parent.gameObject);
        TagSystemData data = Toolbox.GetTagData(gameObject);
        data.targetPriority = -1;
    }
}