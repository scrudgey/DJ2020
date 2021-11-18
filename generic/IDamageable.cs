using UnityEngine;
using System;
using System.Collections.Generic;
public struct DamageResult {
    public float damageAmount;
}
public abstract class IDamageable : MonoBehaviour {
    protected Damage lastDamage;
    protected DamageResult lastResult;

    Dictionary<Type, Func<Damage, DamageResult>> damageHandlers = new Dictionary<Type, Func<Damage, DamageResult>>();

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
    public virtual void TakeDamage<T>(T damage) where T : Damage {
        HandleDamage(damage, damage.GetType());
        HandleDamage(damage, typeof(Damage));
    }

    void HandleDamage(Damage damage, Type type) {
        if (damageHandlers.ContainsKey(type) && damageHandlers[type] != null) {
            DamageResult result = damageHandlers[type](damage);
            ApplyDamageResult(result);
            lastDamage = damage;
        }
    }
    protected virtual void ApplyDamageResult(DamageResult result) { }
}

