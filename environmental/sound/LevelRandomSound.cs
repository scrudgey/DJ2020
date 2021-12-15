using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRandomSound : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    private float timer;
    public LoHi intervalRange;
    private float interval;

    void Awake() {
        audioSource = Toolbox.GetOrCreateComponent<AudioSource>(gameObject);
        // audioSource.spatialBlend = 0;
        interval = Toolbox.RandomFromLoHi(intervalRange);
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > interval) {
            timer -= interval;
            Toolbox.RandomizeOneShot(audioSource, audioClips);
            interval = Toolbox.RandomFromLoHi(intervalRange);
        }
    }
}
