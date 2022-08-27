using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damageable : MonoBehaviour, IDamageReceiver {
    // basic message bus pattern
    public Gibs gibs;
    public Damage lastDamage;
    protected Collider myCollider;
    public Action<Damageable, Damage> OnTakeDamage { get; set; }
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
    // probably unnecessary: all references here are internal to the gameobject.
    // void OnDestroy() {
    //     List<KeyValuePair<Type, Func<Damage, DamageResult>>> kvps = new List<KeyValuePair<Type, Func<Damage, DamageResult>>>();
    //     foreach (KeyValuePair<Type, Func<Damage, DamageResult>> kvp in damageHandlers) {
    //         kvps.Add(kvp);
    //     }
    //     foreach (KeyValuePair<Type, Func<Damage, DamageResult>> kvp in kvps) {
    //         damageHandlers[kvp.Key] -= kvp.Value;
    //     }
    // }
    public virtual DamageResult TakeDamage(Damage damage) {
        DamageResult result1 = HandleDamage(damage, damage.GetType());
        DamageResult result2 = HandleDamage(damage, typeof(Damage));
        DamageResult result = result1.Add(result2);

        gibs?.EmitOnImpact(gameObject, result, myCollider);
        OnTakeDamage?.Invoke(this, damage);
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

