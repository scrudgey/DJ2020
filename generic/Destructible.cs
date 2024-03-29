using System;
using UnityEngine;
public class Destructible : Damageable, IPoolable {
    public float health;
    public float fullHealthAmount;
    private bool doDestruct;
    public float destructionTimer = 0f;
    public GameObject[] destructionFx;
    public AudioClip[] destructSounds;
    public Action<Destructible> OnHitStateChanged { get; set; }
    public Action OnDestruct;
    private HitState _hitState;
    public HitState hitState {
        get { return _hitState; }
        set {
            // if value has changed, send a message:
            if (value != _hitState) {
                _hitState = value;
                OnHitStateChanged?.Invoke(this);
            }
        }
    }
    override public DamageResult TakeDamage(Damage damage) {
        DamageResult result = base.TakeDamage(damage);
        if (health <= 0) {
            Destruct(damage);
        }
        return result;
    }
    protected override void ApplyDamageResult(DamageResult result) {
        base.ApplyDamageResult(result);
        health -= result.damageAmount;
    }

    protected void Destruct(Damage damage) {
        TagSystemData data = Toolbox.GetTagData(gameObject);
        data.targetPriority = -1;
        EmitGibs(damage);
        DoDestruct(damage);
        OnDestruct?.Invoke();
    }
    virtual protected void DoDestruct(Damage damage) {
        if (transform.root != null) {
            Destroy(transform.root.gameObject, destructionTimer);
        } else {
            Destroy(gameObject, destructionTimer);
        }
    }
    protected void EmitGibs(Damage damage) {
        gibs?.EmitOnDamage(gameObject, damage, myCollider);
        foreach (GameObject fx in destructionFx) {
            GameObject.Instantiate(fx, transform.position, Quaternion.identity);
        }
        if (destructSounds.Length > 0) {
            Toolbox.AudioSpeaker(transform.position, destructSounds, volume: 2f);
        }
    }

    public virtual void OnPoolActivate() {
        health = fullHealthAmount;
        hitState = HitState.normal;
    }
    public virtual void OnPoolDectivate() {
        health = fullHealthAmount;
        hitState = HitState.normal;
        TagSystemData data = Toolbox.GetTagData(gameObject);
        data.targetPriority = 1;
        // Debug.Log("pool deactivate on destructible");
    }
}