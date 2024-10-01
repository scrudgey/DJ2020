using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

public abstract class MusicMixController {
    public Coroutine[] fadeRoutines;
    public float[] targetVolumes;
    public AudioSource[] audioSources;
    public MusicTrack track;

    public abstract void Play();
    public abstract void Stop();
    public abstract void Pause();
    public abstract void Update();

    protected void SetTrackVolume(int index, float target) {
        if (targetVolumes[index] == target) {
            return;
        }
        targetVolumes[index] = target;
        if (fadeRoutines[index] != null) {
            MusicController.I.StopCoroutine(fadeRoutines[index]);
        }
        fadeRoutines[index] = MusicController.I.StartCoroutine(DoFade(audioSources[index], target));
    }

    protected IEnumerator DoFade(AudioSource source, float targetVolume) {
        return Toolbox.Ease(null, 1f, source.volume, targetVolume, PennerDoubleAnimation.Linear, (amount) => {
            source.volume = amount;
        }, unscaledTime: true);
    }
}