using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSound : MonoBehaviour {
    enum State { shut, open }
    State state = State.shut;
    public HingeJoint hinge;
    public float openAngleThreshold = 5f;
    public float shutSpeedThreshold = 1f;
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    public AudioSource audioSource;

    void Update() {
        if (Mathf.Abs(hinge.angle) > openAngleThreshold && state == State.shut) {
            state = State.open;
            Toolbox.RandomizeOneShot(audioSource, openSounds);
        }
        if (Mathf.Abs(hinge.angle) < openAngleThreshold && Mathf.Abs(hinge.velocity) < shutSpeedThreshold && state == State.open) {
            state = State.shut;
            Toolbox.RandomizeOneShot(audioSource, closeSounds);
        }
    }
}
