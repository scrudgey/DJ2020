using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour {
    public enum TriggerType { impact, start }
    public TriggerType trigger;
    public AudioClip[] sounds;
    public AudioSource audioSource;
    public float probability = 1f;
    public float velocityThreshold = 0f;
    private void Play() {
        Toolbox.RandomizeOneShot(audioSource, sounds);
    }

    void OnCollisionEnter(Collision collision) {
        if (trigger != TriggerType.impact)
            return;
        if (Random.Range(0, 1f) < probability && collision.relativeVelocity.magnitude > velocityThreshold) {
            Play();
        }
    }

    void Start() {
        if (trigger == TriggerType.start && Random.Range(0, 1f) < probability) {
            Play();
            // Destroy(this);
        }
    }
}
