using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAmbientSound : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    private float timer;
    private float interval;
    void Awake() {
        audioSource = Toolbox.GetOrCreateComponent<AudioSource>(gameObject);
        audioSource.loop = true;
    }
    void Start() {
        PlayRandomSound();
    }
    void LateUpdate() {
        timer += Time.deltaTime;
        if (timer > interval) {
            PlayRandomSound();
            timer = 0;
        }
    }
    void PlayRandomSound() {
        AudioClip clip = Toolbox.RandomFromList(audioClips);
        audioSource.PlayOneShot(clip);
        interval = clip.length;
    }
}
