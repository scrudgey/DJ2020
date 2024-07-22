using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnSoundStop : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] clip;
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        audioSource.minDistance = 2f;
    }
    void Start() {
        if (clip.Length > 0)
            Toolbox.RandomizeOneShot(audioSource, clip);
    }
    void Update() {
        if (!audioSource.isPlaying) {
            PoolManager.I.RecallObject(gameObject);
        }
    }
}
