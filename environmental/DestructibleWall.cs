using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWall : MonoBehaviour {
    public float health = 5f;
    public Gibs gibs;
    Collider myCollider;
    void Awake() {
        myCollider = GetComponent<Collider>();
    }
    public void TakeExplosionDamage(Explosion explosion) {
        Vector3 myPosition = transform.position;
        if (myCollider != null) {
            myPosition = myCollider.bounds.center;
        }
        Vector3 force = Toolbox.CalculateExplosionVector(explosion, myPosition);
        health -= force.magnitude;
        if (health <= 0) {
            Destruct(force);
        }
    }

    public void Destruct(Vector3 lastDamage) {
        Destroy(transform.parent.gameObject);
        Collider myCollider = GetComponent<Collider>();
        Vector3 position = transform.position;
        if (myCollider != null) {
            position = Toolbox.RandomInsideBounds(myCollider);
        }
        if (gibs != null) {
            foreach (Gib gib in gibs.gibs) {
                gib.Emit(position, lastDamage);
            }
        }
        Debug.Break();
    }
}
