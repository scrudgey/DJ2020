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
    public float volume = 1f;
    public float probability = 1f;
    public float velocityThreshold = 0f;
    public float cooldownInterval = 1f;
    public float cooldownTimer = 0f;
    public float randomPitchWidth = 0.2f;
    private int currentNumberOfParticles;
    public bool jukebox;
    public void Play() {
        if (cooldownTimer > 0)
            return;
        if (Random.Range(0, 1f) < probability) {
            DoPlay();
            if (repeat == RepeatType.once) {
                Destroy(this);
            } else if (repeat == RepeatType.cooldown) {
                cooldownTimer = cooldownInterval;
            }
        }
    }
    public void DoPlay() {
        if (jukebox) {
            Toolbox.AudioSpeaker(transform.position, sounds, volume: volume);
        } else {
            Toolbox.RandomizeOneShot(audioSource, sounds, randomPitchWidth: randomPitchWidth);
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
        audioSource = Toolbox.SetUpAudioSource(gameObject);
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
