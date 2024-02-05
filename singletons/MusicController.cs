using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class MusicController : Singleton<MusicController> {
    public static readonly Dictionary<MusicTrack, string[]> MUSIC_PATHS = new Dictionary<MusicTrack, string[]>{
        {MusicTrack.none, new string[0]},
        {MusicTrack.lethalGlee, new string[]{
            "Lethal Glee layer 1 LOW DJ3",
            "Lethal Glee layer 2 CADENCE DJ3",
            "Lethal Glee layer 3 ATTENTION DJ3",
            "Lethal Glee layer 4 DEATH DJ3"
        }},
        {MusicTrack.obligateArsonist, new string[]{
            "Obligate Arsonist VER 2.0 layer 1 LOW DJ3",
            "Obligate Arsonist Ver 2.0 layer 2 CADENCE DJ3",
            "Obligate Arsonist VER 2.0 layer 3 ATTENTION DJ3",
            "Obligate Arsonist Ver 2.0 layer 4 DETH DJ3"
        }},
        {MusicTrack.antiAnecdote, new string[]{
            "Anti-Anecdote [Ver 2.0] layer 1 LOW DJ3 2023",
            "Anti-Anecdote [Ver 2.0] layer 2 CADENCE DJ3 2023",
            "Anti-Anecdote [Ver 2.0] layer 3 ATTENTION DJ3 2023",
            "Anti-Anecdote [Ver 2.0] layer 4 DEATH DJ3 2023"
        }},
        {MusicTrack.autoeroticDefenestration, new string[]{
            "Autoerotic Defenestration VER 1.0 - LOW layer1  - DJ3 2024",
            "Autoerotic Defenestration VER 1.0 - CADENCE layer2 - DJ3 2024",
            "Autoerotic Defenestration VER 1.0 - ATTN layer3 - DJ3 2024",
            "Autoerotic Defenestration VER 1.0 - DETH layer4 - DJ3 2024"
        }},
        {MusicTrack.hyperLysisCypher, new string[]{
            "Hyper Lysis Cypher - VER 1.5 - Layer 1 LOW DJ3 2023",
            "Hyper Lysis Cypher - VER 1.5 - Layer 2 CADENCE DJ3 2023",
            "Hyper Lysis Cypher - VER 1.5 - Layer 3 ATTENTION DJ3 2023",
            "Hyper Lysis Cypher - VER 1.5 - Layer 4 DETH - DJ3 2023"
        }},

        {MusicTrack.whereIHidTheBodies, new string[]{
            "This Is Where I Hid The Bodies VER 4.0 LOW Layer 1 - DJ3 2024",
            "This Is Where I Hid The Bodies VER 4.0 CADENCE Layer 2 - DJ3 2024",
            "This Is Where I Hid The Bodies VER 4.0 ATTN Layer 3 - DJ3 2024",
            "This Is Where I Hid The Bodies VER 4.0 DETH Layer 4 - DJ3 2024"
        }}
    };
    public AudioSource[] audioSources;
    public AudioClip[] subtracks;
    public Coroutine[] fadeRoutines;
    public float[] targetVolumes;

    void Awake() {
        fadeRoutines = new Coroutine[4];
        targetVolumes = new float[4];
        targetVolumes[0] = -1;
        targetVolumes[1] = -1;
        targetVolumes[2] = -1;
        targetVolumes[3] = -1;
        GameManager.OnSuspicionChange += HandleSuspicionChange;
    }
    public void LoadTrack(MusicTrack track) {
        subtracks = MUSIC_PATHS[track].Select((filename) => {
            string path = MusicResourcePath(track, filename);
            return Resources.Load<AudioClip>(path) as AudioClip;
        }).ToArray();

        audioSources.ToList().ForEach(audiosource => audiosource.volume = 0f);
        foreach (var x in audioSources.Zip(subtracks, Tuple.Create)) {
            x.Item1.clip = x.Item2;
            x.Item1.Play();
        }
        HandleSuspicionChange();
    }
    public void Stop() {
        audioSources.ToList().ForEach(audiosource => audiosource.Stop());
    }

    static string MusicResourcePath(MusicTrack track, string filename) {
        return $"music/{track}/{filename}";
    }

    public void HandleSuspicionChange() {
        Suspiciousness sus = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction(sus);
        // Debug.Log($"[music] handle suspicious change {sus}");
        // TODO: ease in the volumes
        switch (reaction) {
            case Reaction.ignore:
                SetTrackVolume(0, 1);
                SetTrackVolume(1, 0);
                SetTrackVolume(2, 0);
                SetTrackVolume(3, 0);
                // audioSources[0].volume = 1;
                // audioSources[1].volume = 0;
                // audioSources[2].volume = 0;
                // audioSources[3].volume = 0;
                break;
            case Reaction.investigate:
                // audioSources[0].volume = 1f;
                // audioSources[1].volume = 1f;
                // audioSources[2].volume = 0;
                // audioSources[3].volume = 0;

                SetTrackVolume(0, 1);
                SetTrackVolume(1, 1);
                SetTrackVolume(2, 0);
                SetTrackVolume(3, 0);
                break;
            case Reaction.attack:
                // audioSources[0].volume = 1f;
                // audioSources[1].volume = 1f;
                // audioSources[2].volume = 1f;
                // audioSources[3].volume = 1f;

                SetTrackVolume(0, 1);
                SetTrackVolume(1, 1);
                SetTrackVolume(2, 1);
                SetTrackVolume(3, 1);
                break;
        }

        // switch (sus) {
        //     case Suspiciousness.normal:
        //         audioSources[0].volume = 1;
        //         audioSources[1].volume = 1;
        //         audioSources[2].volume = 0;
        //         audioSources[3].volume = 0;
        //         break;
        //     case Suspiciousness.suspicious:
        //         audioSources[0].volume = 1;
        //         audioSources[1].volume = 1;
        //         audioSources[2].volume = 1;
        //         audioSources[3].volume = 0;
        //         break;
        //     case Suspiciousness.aggressive:
        //         audioSources[0].volume = 1;
        //         audioSources[1].volume = 1;
        //         audioSources[2].volume = 1;
        //         audioSources[3].volume = 1;
        //         break;
        // }
    }

    void SetTrackVolume(int index, float target) {
        if (targetVolumes[index] == target) {
            return;
        }
        targetVolumes[index] = target;
        if (fadeRoutines[index] != null) {
            StopCoroutine(fadeRoutines[index]);
        }
        fadeRoutines[index] = StartCoroutine(DoFade(audioSources[index], target));
    }

    IEnumerator DoFade(AudioSource source, float targetVolume) {
        return Toolbox.Ease(null, 1f, source.volume, targetVolume, PennerDoubleAnimation.Linear, (amount) => {
            source.volume = amount;
        }, unscaledTime: true);
    }



}
