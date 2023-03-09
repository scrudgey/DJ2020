using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
public class Rocket : MonoBehaviour {
    public DirectionalBillboard billboard;
    public Rigidbody body;
    public Explosive explosive;
    public C4Data data;
    bool didExplosion;
    public Transform smokeTrail;
    public Collider myCollider;
    Vector3 previousVel;
    Vector3 previousAngVel;
    Vector3 previousPos;
    void Update() {
        if (body.velocity.magnitude > 1f) {
            billboard.direction = body.velocity.normalized;
        }
    }
    void LateUpdate() {
        previousVel = body.velocity;
        previousAngVel = body.angularVelocity;
        previousPos = transform.position;
    }
    void OnCollisionEnter(Collision collision) {
        if (didExplosion) return;

        TagSystemData tagData = Toolbox.GetTagData(collision.collider.gameObject);
        if (!tagData.bulletPassthrough) {

            explosive.Explode();
            didExplosion = true;
            smokeTrail.SetParent(null, true);
            Destroy(smokeTrail.gameObject, 2f);
            Destroy(gameObject);
        } else {
            ExplosionDamage damage = new ExplosionDamage(500f, body.velocity.normalized, collision.contacts[0].point, transform.position);
            foreach (IDamageReceiver receiver in collision.collider.gameObject.GetComponentsInChildren<IDamageReceiver>()) {
                receiver.TakeDamage(damage);
            }

            Physics.IgnoreCollision(myCollider, collision.collider);
            body.velocity = previousVel;
            body.angularVelocity = previousAngVel;
            transform.position = previousPos + body.velocity * Time.deltaTime;
        }

    }
}
