using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour {
    public float health;
    // public AudioSource audioSource;
    public GameObject[] destructionFx;
    public AudioClip[] destructSounds;
    public void TakeDamage(BulletImpact impact) {
        health -= impact.bullet.damage;
        if (health <= 0) {
            Destruct();
        }
    }

    public void Destruct() {
        // TODO: destroy parent?
        Destroy(transform.parent.gameObject, 5f);
        foreach (GameObject fx in destructionFx) {
            GameObject.Instantiate(fx, transform.position, Quaternion.identity);
        }
        if (destructSounds.Length > 0) {
            Toolbox.AudioSpeaker(transform.position, destructSounds);
        }

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) {
            Vector3 force = 50f * Random.onUnitSphere;
            force.y = Mathf.Abs(force.y);
            rigidbody.AddForce(force, ForceMode.Impulse);

        }

        // TODO: destroy more things?
        Destroy(this);
    }
}
