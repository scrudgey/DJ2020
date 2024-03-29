using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEmitter : MonoBehaviour, IDamageReceiver {
    public float probability = 0.5f;
    public LoHi velocity;
    public GameObject[] particles;
    private Dictionary<GameObject, PrefabPool> pools = new Dictionary<GameObject, PrefabPool>();
    void Awake() {
        foreach (GameObject particle in particles) {
            pools[particle] = PoolManager.I.RegisterPool(particle);
        }
    }
    public DamageResult TakeDamage(Damage impact) {
        if (impact is BulletDamage bullet && Random.Range(0, 1f) < probability) {
            Emit(bullet);
        }
        return DamageResult.NONE;
    }
    public void Emit(BulletDamage impact) {
        GameObject prefab = particles[Random.Range(0, particles.Length)];
        PrefabPool pool = pools[prefab];
        if (prefab != null) {
            GameObject particle = pool.GetObject(impact.hit.point + 0.1f * impact.hit.normal);
            particle.transform.rotation = Random.rotation;
            Rigidbody body = particle.GetComponentInChildren<Rigidbody>();
            body.velocity = impact.hit.normal * velocity.GetRandomInsideBound();
            // randomize direction (how?)
        }
    }
}
