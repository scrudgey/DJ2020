using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

public class MissionMusicController : MusicMixController {
    public AudioClip[] subtracks;
    public MissionMusicController(MusicTrack track, AudioSource[] audioSources) {
        this.track = track;
        this.audioSources = audioSources;

        subtracks = MusicController.MUSIC_PATHS[track].Select((filename) => {
            string path = MusicController.MusicResourcePath(track, filename);
            return Resources.Load<AudioClip>(path) as AudioClip;
        }).ToArray();

        fadeRoutines = new Coroutine[4];
        targetVolumes = new float[4];
        targetVolumes[0] = -1;
        targetVolumes[1] = -1;
        targetVolumes[2] = -1;
        targetVolumes[3] = -1;
    }
    public override void Stop() {

        GameManager.OnSuspicionChange -= HandleSuspicionChange;
        foreach (Coroutine routine in fadeRoutines) {
            if (routine == null || GameManager.I == null) continue;

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

    public override void Play() {
        GameManager.OnSuspicionChange += HandleSuspicionChange;
        audioSources.ToList().ForEach(audiosource => audiosource.volume = 0f);
        foreach (var x in audioSources.Zip(subtracks, Tuple.Create)) {
            x.Item1.Stop();
            x.Item1.clip = x.Item2;
            x.Item1.Play();
        }
        HandleSuspicionChange();
    }

    void HandleSuspicionChange() {
        Suspiciousness sus = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction(sus);
        bool alarmActive = GameManager.I.gameData.levelState.delta.alarmGraph.anyAlarmTerminalActivated();
        switch (reaction) {
            case Reaction.ignore:
                SetTrackVolume(0, 1);
                SetTrackVolume(1, 0);
                SetTrackVolume(2, 0);
                SetTrackVolume(3, 0);
                break;
            case Reaction.investigate:
                SetTrackVolume(0, 1);
                SetTrackVolume(1, 1);
                SetTrackVolume(2, 0);
                SetTrackVolume(3, 0);
                break;
            case Reaction.attack:
                SetTrackVolume(0, 1);
                SetTrackVolume(1, 1);
                SetTrackVolume(2, 1);
                if (alarmActive) {
                    SetTrackVolume(3, 1);
                } else {
                    SetTrackVolume(3, 0);
                }
                break;
        }
    }
}