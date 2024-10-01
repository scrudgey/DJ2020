using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

public class SimpleMusicController : MusicMixController {
    public AudioClip subtrack;
    bool looping;
    bool isPlaying;
    float playTime;
    public SimpleMusicController(MusicTrack track, AudioSource[] audioSources, bool looping = true) {
        this.track = track;
        this.audioSources = audioSources;
        this.looping = looping;

        // LoadTrack();
        string path = MusicController.MusicSingleResourcePath(track);
        subtrack = Resources.Load<AudioClip>(path) as AudioClip;

        fadeRoutines = new Coroutine[4];
        targetVolumes = new float[4];
        targetVolumes[0] = -1;
        targetVolumes[1] = -1;
        targetVolumes[2] = -1;
        targetVolumes[3] = -1;

    }

    public override void Play() {
        if (!looping) {
            audioSources[0].loop = false;
        } else {
            audioSources[0].loop = true;
        }
        isPlaying = true;
        audioSources[0].Stop();
        audioSources[0].clip = subtrack;
        SetTrackVolume(0, 1);
        audioSources[0].Play();
    }

    public override void Stop() {
        foreach (Coroutine routine in fadeRoutines) {
            if (routine == null) continue;
            GameManager.I?.StopCoroutine(routine);
        }
        audioSources.ToList().ForEach(audiosource => {
            audiosource.volume = 0f;
            audiosource.Stop();
        });
        isPlaying = false;
    }

    public override void Pause() {
        isPlaying = false;

        // throw new NotImplementedException();
    }
    public override void Update() {
        if (isPlaying) {
            playTime += Time.unscaledDeltaTime;
        }
        if (!looping && playTime > subtrack.length) {
            Stop();
            MusicController.I.PopMix(this);
        }
    }
}