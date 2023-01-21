using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
public class Rocket : MonoBehaviour {
    // public GameObject smokeEffect;
    public DirectionalBillboard billboard;
    public Rigidbody body;
    public Explosive explosive;
    public C4Data data;
    bool didExplosion;
    public Transform smokeTrail;
    void Update() {
        if (body.velocity.magnitude > 1f) {
            billboard.direction = body.velocity.normalized;
        }
    }
    void OnCollisionEnter(Collision collision) {
        if (didExplosion) return;
        explosive.Explode();
        didExplosion = true;
        smokeTrail.SetParent(null, true);
        Destroy(smokeTrail.gameObject, 2f);
        Destroy(gameObject);
    }
}
