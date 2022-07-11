using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damageable : MonoBehaviour, IDamageReceiver {
    // basic message bus pattern
    public Gibs gibs;
    protected Damage lastDamage;
    protected DamageResult lastResult;
    protected Collider myCollider;
    Dictionary<Type, Func<Damage, DamageResult>> damageHandlers = new Dictionary<Type, Func<Damage, DamageResult>>();


    virtual public void Awake() {
        myCollider = GetComponentInChildren<Collider>();
        if (gibs != null)
            foreach (Gib gib in gibs.gibs) {
                PoolManager.I.RegisterPool(gib.prefab);
            }
    }
    protected void RegisterDamageCallback<T>(Func<T, DamageResult> handler) where T : Damage {
        Type tType = typeof(T);
        Func<Damage, DamageResult> wrapper = (Damage damage) => {
            if (damage is T tparam) {
                return handler(tparam);
            } else {
                Debug.LogError($"illegal state: damage handler of type {tType} cannot cast argument");
                return DamageResult.NONE;
            }
        };

        if (damageHandlers.ContainsKey(tType)) {
            damageHandlers[tType] += wrapper;
        } else {
            damageHandlers[tType] = wrapper;
        }
    }
    public virtual DamageResult TakeDamage(Damage damage) {
        DamageResult result1 = HandleDamage(damage, damage.GetType());
        DamageResult result2 = HandleDamage(damage, typeof(Damage));
        DamageResult result = result1.Add(result2);

        gibs.EmitOnImpact(gameObject, result, myCollider);
        return result;
    }
    DamageResult HandleDamage(Damage damage, Type type) {
        if (damageHandlers.ContainsKey(type) && damageHandlers[type] != null) {
            DamageResult result = damageHandlers[type](damage);
            ApplyDamageResult(result);
            lastDamage = damage;
            return result;
        } else return DamageResult.NONE;
    }
    protected virtual void ApplyDamageResult(DamageResult result) { }
}

