using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnParticleEnd : MonoBehaviour {
    ParticleSystem particles;
    void Awake() {
        particles = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update() {
        if (!particles.isPlaying) {
            Destroy(gameObject);
        }
    }
}
