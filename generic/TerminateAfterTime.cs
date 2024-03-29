using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminateAfterTime : MonoBehaviour, IPoolable {
    public float lifetime;
    private float timer;
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifetime) {
            Terminate();
        }
    }

    void Terminate() {
        foreach (Collider collider in GetComponents<Collider>()) {
            collider.enabled = false;
        }
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource) {
            audioSource.enabled = false;
        }
        Toolbox.DisableIfExists<PlaySound>(gameObject);
        Toolbox.DisableIfExists<FlipScintillator>(gameObject);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) {
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rigidbody.isKinematic = true;
            // rigidbody.velocity = Vector3.zero;
            // rigidbody.inertiaTensorRotation = Quaternion.identity;
        }
        this.enabled = false;
        if (GameManager.I.clearSighterV3 != null) {
            GameManager.I.clearSighterV3.AddStatic(transform);
        }
    }

    public void OnPoolActivate() {
        timer = 0;
        this.enabled = true;
        foreach (Collider collider in GetComponents<Collider>()) {
            collider.enabled = true;
        }
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource) {
            audioSource.enabled = true;
        }
        Toolbox.EnableIfExists<PlaySound>(gameObject);
        Toolbox.EnableIfExists<FlipScintillator>(gameObject);

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) {
            rigidbody.isKinematic = false;
        }
    }
    public void OnPoolDectivate() { }
}
