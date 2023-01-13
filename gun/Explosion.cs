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
                rb.AddExplosionForce(power * 20f, explosionPos, radius, 3.0F);

            // TODO: unify this with bullet method
            foreach (IDamageReceiver damageable in hit.GetComponentsInChildren<IDamageReceiver>()) {
                // TODO: calculate damage at point.
                damageable.TakeDamage(GetDamageAtPoint(hit.bounds.center));
            }
        }
        Destroy(gameObject);
        NoiseData data = new NoiseData {
            volume = radius * 10f,
            suspiciousness = Suspiciousness.aggressive
        };
        Toolbox.Noise(transform.position, data, transform.root.gameObject);
    }

    public ExplosionDamage GetDamageAtPoint(Vector3 point) {
        Vector3 direction = (point - transform.position).normalized;
        float dist = (point - transform.position).magnitude;
        if (dist > radius) {
            return new ExplosionDamage(0, direction, point);
        } else {
            return new ExplosionDamage((1.0f - dist / radius) * power, direction, point);
        }
    }

}
