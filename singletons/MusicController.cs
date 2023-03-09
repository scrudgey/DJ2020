using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class MusicController : Singleton<MusicController> {
    public static readonly Dictionary<MusicTrack, string[]> MUSIC_PATHS = new Dictionary<MusicTrack, string[]>{
        {MusicTrack.none, new string[0]},
        {MusicTrack.lethalGlee, new string[]{
            "Lethal Glee layer 1 LOW DJ3",
            "Lethal Glee layer 2 CADENCE DJ3",
            "Lethal Glee layer 3 ATTENTION DJ3",
            "Lethal Glee layer 4 DEATH DJ3"
        }}
    };
    public AudioSource[] audioSources;
    public AudioClip[] subtracks;

    void Awake() {
        GameManager.OnSuspicionChange += HandleSuspicionChange;
    }
    public void LoadTrack(MusicTrack track) {
        subtracks = MUSIC_PATHS[track].Select((filename) => {
            string path = MusicResourcePath(track, filename);
            return Resources.Load<AudioClip>(path) as AudioClip;
        }).ToArray();

        audioSources.ToList().ForEach(audiosource => audiosource.volume = 0f);
        // audioSources.Zip(subtracks, (audioSource, subtrack) => audioSource.clip = subtrack);
        foreach (var x in audioSources.Zip(subtracks, Tuple.Create)) {
            x.Item1.clip = x.Item2;
            x.Item1.Play();
        }
        // audioSources.ToList().ForEach(audiosource => audiosource.Play());
    }
    public void Stop() {
        audioSources.ToList().ForEach(audiosource => audiosource.Stop());
    }

    static string MusicResourcePath(MusicTrack track, string filename) {
        return $"music/{track}/{filename}";
    }

    public void HandleSuspicionChange() {
        Suspiciousness sus = GameManager.I.GetTotalSuspicion();
        // TODO: ease in the volumes
        switch (sus) {
            case Suspiciousness.normal:
                audioSources[0].volume = 1;
                audioSources[1].volume = 0;
                audioSources[2].volume = 0;
                audioSources[3].volume = 0;
                break;
            case Suspiciousness.suspicious:

                audioSources[0].volume = 1;
                audioSources[1].volume = 1;
                audioSources[2].volume = 0;
                audioSources[3].volume = 0;
                break;
            case Suspiciousness.aggressive:
                audioSources[0].volume = 1;
                audioSources[1].volume = 1;
                audioSources[2].volume = 1;
                audioSources[3].volume = 1;
                break;
        }
    }


}
