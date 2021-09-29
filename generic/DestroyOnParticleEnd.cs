using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnParticleEnd : MonoBehaviour {
    ParticleSystem[] particles;
    void Awake() {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update() {
        bool isPlaying = false;
        foreach (ParticleSystem sys in particles) {
            if (sys.isPlaying) {
                isPlaying = true;
                break;
            }
        }
        if (!isPlaying) {
            Destroy(gameObject);
        }
    }
}
