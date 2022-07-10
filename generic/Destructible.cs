using UnityEngine;

public class Destructible : Damageable {
    public Gibs gibs;
    public float health;
    private bool doDestruct;
    public float destructionTimer = 5f;
    public GameObject[] destructionFx;
    public AudioClip[] destructSounds;

    override public void TakeDamage(Damage damage) {
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
        Destroy(transform.parent.gameObject, destructionTimer);
        TagSystemData data = Toolbox.GetTagData(gameObject);
        data.targetPriority = -1;

        gibs?.Emit(gameObject, damage, myCollider);
        foreach (GameObject fx in destructionFx) {
            GameObject.Instantiate(fx, transform.position, Quaternion.identity);
        }
        if (destructSounds.Length > 0) {
            Toolbox.AudioSpeaker(transform.position, destructSounds, volume: 2f);
        }
    }
}