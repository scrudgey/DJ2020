using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorIndicator : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] arrivalSound;

    public void ElevatorArrival() {
        Toolbox.RandomizeOneShot(audioSource, arrivalSound);
    }
}
