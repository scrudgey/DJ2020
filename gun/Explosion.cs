using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    public float radius = 2.0F;
    public float power = 10.0F;

    void LateUpdate() {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders) {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);

            // TODO: unify this with bullet method
            foreach (IDamageable damageable in hit.GetComponentsInChildren<IDamageable>()) {
                damageable.TakeDamage(new ExplosionDamage(this));
            }
        }
        Destroy(gameObject);
    }

}
