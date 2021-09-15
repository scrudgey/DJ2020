using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnSoundStop : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] clip;
    void Start() {
        audioSource = GetComponent<AudioSource>();
        if (clip.Length > 0)
            Toolbox.RandomizeOneShot(audioSource, clip);
    }

    // Update is called once per frame
    void Update() {
        if (!audioSource.isPlaying) {
            Destroy(gameObject);
        }
    }
}
