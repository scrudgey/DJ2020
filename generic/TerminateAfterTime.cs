using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminateAfterTime : PoolObject {
    public float lifetime;
    private float timer;
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifetime) {
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
                rigidbody.isKinematic = true;
            }
            this.enabled = false;
        }
    }

    public override void OnPoolActivate() {
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
}
