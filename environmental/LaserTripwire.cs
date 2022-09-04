using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserTripwire : MonoBehaviour {
    public LaserBeam[] laserBeams;
    public AudioClip[] spottedSound;
    float cooldown;
    AudioSource audioSource;

    public void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        foreach (LaserBeam beam in laserBeams) {
            beam.tripWire = this;
        }
    }

    public void LaserTripCallback() {
        if (cooldown > 0)
            return;
        GameManager.I.ActivateAlarm();
        cooldown = 5f;
        Toolbox.RandomizeOneShot(audioSource, spottedSound);
    }

    void Update() {
        if (cooldown > 0f) {
            cooldown -= Time.deltaTime;
        }
    }
}
