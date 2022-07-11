using UnityEngine;

public class Destructible : Damageable {
    public float health;
    private bool doDestruct;
    public float destructionTimer = 5f;
    public GameObject[] destructionFx;
    public AudioClip[] destructSounds;
    override public DamageResult TakeDamage(Damage damage) {
        DamageResult result = base.TakeDamage(damage);
        if (health <= 0) {
            doDestruct = true;
        }
        return result;
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
        Destroy(transform.parent.gameObject, destructionTimer);
        TagSystemData data = Toolbox.GetTagData(gameObject);
        data.targetPriority = -1;

        gibs?.EmitOnDamage(gameObject, damage, myCollider);
        foreach (GameObject fx in destructionFx) {
            GameObject.Instantiate(fx, transform.position, Quaternion.identity);
        }
        if (destructSounds.Length > 0) {
            Toolbox.AudioSpeaker(transform.position, destructSounds, volume: 2f);
        }
    }
}