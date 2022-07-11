using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Gibs")]
public class Gibs : ScriptableObject {
    public List<Gib> gibs;
    public void EmitOnDamage(GameObject host, Damage damage, Collider bounds) =>
        gibs.Where(gib =>
            !gib.impact &&
            gib.damageConditional.ConditionIsMet(damage))
        .ToList()
        .ForEach(gib => {
            gib.Emit(host, damage, bounds);
        });
    public void EmitOnImpact(GameObject host, DamageResult result, Collider bounds) =>
        gibs.Where(gib =>
            gib.impact &&
            gib.impactConditional.ConditionIsMet(result))
        .ToList()
        .ForEach(gib => {
            gib.Emit(host, result.damage, bounds);
        });
}
public enum GibType { normal, particleEffect }

[System.Serializable]
public class Gib {
    public DamageConditional damageConditional;
    public DamageResultConditional impactConditional;
    public bool impact;
    public GibType type;
    public LoHi number;
    public GameObject prefab;
    public LoHi velocity;
    public LoHi dispersion;
    public float directional = 1f;
    public void Emit(GameObject host, Damage damage, Collider collider) {
        if (type == GibType.normal) {
            EmitParticle(damage, collider);
        } else if (type == GibType.particleEffect) {
            EmitParticleSystem(host, damage, collider);
        }
    }
    void EmitParticleSystem(GameObject host, Damage damage, Collider collider) {
        GameObject fx = GameObject.Instantiate(prefab, collider.bounds.center, Quaternion.identity);
        fx.transform.SetParent(host.transform, true);
    }
    void EmitParticle(Damage damage, Collider bounds) {
        int num = (int)number.Random();
        for (int i = 0; i < num; i++) {
            DoEmit(damage, bounds);
        }
    }
    void DoEmit(Damage damage, Collider bounds) {
        Vector3 position = Toolbox.RandomInsideBounds(bounds);
        Vector3 force = damage.force;
        DoEmit(position, force);
    }
    void DoEmit(Vector3 position, Vector3 inDirection) {
        GameObject bit = PoolManager.I.GetPool(prefab).GetObject(position);
        Rigidbody rigidbody = bit.GetComponent<Rigidbody>();

        Vector3 direction = (directional * inDirection) + ((1 - directional) * Vector3.up);
        direction = (dispersion.Random() * Toolbox.RandomPointOnPlane(Vector3.zero, direction, 1f)) + direction.normalized;
        direction = direction.normalized * velocity.Random();

        rigidbody.AddForce(direction, ForceMode.Force);
    }

}