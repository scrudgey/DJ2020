using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

public class SimpleMusicController : MusicMixController {
    public AudioClip subtrack;
    public SimpleMusicController(MusicTrack track, AudioSource[] audioSources) {
        this.track = track;
        this.audioSources = audioSources;

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
        audioSources[0].Stop();
        audioSources[0].clip = subtrack;
        SetTrackVolume(0, 1);
        audioSources[0].Play();
    }

    public override void Stop() {
        foreach (Coroutine routine in fadeRoutines) {
            if (routine == null) continue;
            GameManager.I.StopCoroutine(routine);
        }
        audioSources.ToList().ForEach(audiosource => {
            audiosource.volume = 0f;
            audiosource.Stop();
        });
    }

    public override void Pause() {
        throw new NotImplementedException();
    }
}