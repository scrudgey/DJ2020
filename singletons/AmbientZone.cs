using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientZone : MonoBehaviour {
    public enum AmbienceType { loopRandom, sporadicRandom }
    public AmbienceType type;
    public Collider activeZone;
    public Collider sourceZone;
    public AudioClip[] sounds;
    public float volume;
    public LoHi sporadicInterval;
    Coroutine playRoutine;
    AudioSource audioSource;
    public void Play(AudioSource audioSource) {
        this.audioSource = audioSource;
        if (playRoutine != null)
            StopCoroutine(playRoutine);
        if (type == AmbienceType.loopRandom) {
            playRoutine = StartCoroutine(LoopRandomRoutine());
        } else if (type == AmbienceType.sporadicRandom) {
            playRoutine = StartCoroutine(SporadicRandomRoutine());
        }
    }

    public void Stop() {
        if (playRoutine != null)
            StopCoroutine(playRoutine);
        if (audioSource != null)
            audioSource.Stop();
    }

    IEnumerator LoopRandomRoutine() {
        AudioClip clip = Toolbox.RandomFromList(sounds);

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.pitch = 1;
        audioSource.Play();

        while (true) {
            yield return new WaitForSecondsRealtime(clip.length);
            clip = Toolbox.RandomFromList(sounds);
            audioSource.clip = clip;
            // audioSource.Play();
        }
    }

    IEnumerator SporadicRandomRoutine() {
        audioSource.loop = false;
        audioSource.pitch = 1;
        while (true) {
            yield return new WaitForSecondsRealtime(sporadicInterval.GetRandomInsideBound());
            AudioClip clip = Toolbox.RandomFromList(sounds);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }


}
