using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEmitter : MonoBehaviour {
    public float probability = 0.5f;
    public LoHi velocity;
    // use object pool
    public GameObject[] particles;
    private Dictionary<GameObject, PrefabPool> pools = new Dictionary<GameObject, PrefabPool>();
    void Awake() {
        foreach (GameObject particle in particles) {
            pools[particle] = PoolManager.I.RegisterPool(particle);
        }
    }
    public void TakeDamage(BulletImpact impact) {
        if (Random.Range(0, 1f) < probability) {
            Emit(impact);
        }
    }
    public void Emit(BulletImpact impact) {
        GameObject prefab = particles[Random.Range(0, particles.Length)];
        PrefabPool pool = pools[prefab];
        if (prefab != null) {
            GameObject particle = pool.GetObject(impact.hit.point + 0.1f * impact.hit.normal);
            particle.transform.rotation = Random.rotation;
            Rigidbody body = particle.GetComponentInChildren<Rigidbody>();
            body.velocity = impact.hit.normal * Toolbox.RandomFromLoHi(velocity);
            // randomize direction (how?)
        }
    }
}
