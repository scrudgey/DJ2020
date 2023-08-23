using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElevatorIndicator : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] arrivalSound;
    public TextMeshPro floorIndicator;
    public SpriteRenderer lightIndicator;
    public void ElevatorArrival() {
        Toolbox.RandomizeOneShot(audioSource, arrivalSound);
    }
    public void UpdateCurrentFloor(int floorNumber) {
        floorIndicator.text = $"{floorNumber}";
    }

    public void ShowLight(bool value) {
        lightIndicator.enabled = value;
    }
}
