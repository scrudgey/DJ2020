using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour {
    public enum TriggerType { impact, start, enter, particleEmit }
    public enum RepeatType { repeatedly, once, cooldown }
    public TriggerType trigger;
    public RepeatType repeat;
    public AudioClip[] sounds;
    public AudioSource audioSource;
    public ParticleSystem particles;
    public float probability = 1f;
    public float velocityThreshold = 0f;
    public float cooldownInterval = 1f;
    public float cooldownTimer = 0f;
    public float randomPitchWidth = 0.2f;
    private int currentNumberOfParticles;
    private void Play() {
        if (cooldownTimer > 0)
            return;
        if (Random.Range(0, 1f) < probability) {
            Toolbox.RandomizeOneShot(audioSource, sounds, randomPitchWidth: randomPitchWidth);
            if (repeat == RepeatType.once) {
                Destroy(this);
            } else if (repeat == RepeatType.cooldown) {
                cooldownTimer = cooldownInterval;
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (trigger != TriggerType.impact)
            return;
        if (collision.relativeVelocity.magnitude > velocityThreshold) {
            Play();
        }
    }
    void OnKinematicCharacterImpact() {
        // TODO: use parameters
        if (trigger != TriggerType.impact)
            return;
        Play();
    }

    void Start() {
        if (trigger == TriggerType.start) {
            Play();
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!Toolbox.GetTagData(other.gameObject).isActor)
            return;
        if (trigger == TriggerType.enter) {
            Play();
        }
    }
    void Update() {
        if (cooldownTimer > 0) {
            cooldownTimer -= Time.deltaTime;
        }
        if (trigger == TriggerType.particleEmit) {
            if (particles.particleCount > currentNumberOfParticles) {
                Play();
            }
            currentNumberOfParticles = particles.particleCount;
        }
    }
}
