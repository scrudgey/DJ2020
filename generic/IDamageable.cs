using UnityEngine;
using System;
using System.Collections.Generic;
public struct DamageResult {
    public float damageAmount;
}
public abstract class IDamageable : MonoBehaviour {

    public delegate DamageResult HandleDamage(Damage damage);

    protected HandleDamage handlers;

    public float health;
    protected Damage lastDamage;
    protected DamageResult lastResult;
    public Gibs gibs;
    public bool indestructable;

    Dictionary<Type, Func<Damage, DamageResult>> damageHandlers = new Dictionary<Type, Func<Damage, DamageResult>>();

    // register only once plz
    protected void RegisterDamageCallback<T>(Func<T, DamageResult> handler) where T : Damage {
        Type tType = typeof(T);
        Func<Damage, DamageResult> wrapper = (Damage damage) => {
            if (damage is T tparam) {
                return handler(tparam);
            } else {
                Debug.LogError($"illegal state: damage handler of type {tType} cannot cast argument");
                return new DamageResult();
            }
        };

        if (damageHandlers.ContainsKey(tType)) {
            damageHandlers[tType] += wrapper;
        } else {
            damageHandlers[tType] = wrapper;
        }
    }
    public void TakeDamage<T>(T damage) where T : Damage {
        ApplyDamageResult(damage, damage.GetType());
        ApplyDamageResult(damage, typeof(Damage));
        if (health <= 0 && !indestructable) {
            Destruct(damage);
        }
    }

    void ApplyDamageResult(Damage damage, Type type) {
        if (damageHandlers.ContainsKey(type) && damageHandlers[type] != null) {
            DamageResult result = damageHandlers[type](damage);
            health -= result.damageAmount;
            lastDamage = damage;
        }
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

