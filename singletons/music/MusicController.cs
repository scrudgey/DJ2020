using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class MusicController : Singleton<MusicController> {
    enum State { mission, single }
    Stack<MusicMixController> controllers = new Stack<MusicMixController>();
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
        }},
        {MusicTrack.plexiglassParallax, new string[]{
            "Plexiglass Parallax VER 2.5 - LOW layer1 - DJ3 2024",
            "Plexiglass Parallax VER 2.5 - CADENCE layer2 - DJ3 2024",
            "Plexiglass Parallax VER 2.5 - ATTN layer3 - DJ3 2024",
            "Plexiglass Parallax VER 2.5 - DETH layer4 - DJ3 2024"
        }},
        {MusicTrack.moistPlace, new string[]{
            "New Moist Place LOOP - VER 1.5 - DJ3 2024"
        }},
        {MusicTrack.shopNGo, new string[]{
            "Shop N Go Mart LOOP - VER 2.0 - DJ3 2024"
        }},
        {MusicTrack.sympatheticDetonation, new string[]{
            "Sympathetic Detonation LOOP - Ver 1.0 - DJ3 2024"
        }}
    };
    public AudioSource[] audioSources;

    public void Stop() {
        StopAllMixes();
        audioSources.ToList().ForEach(audiosource => audiosource.Stop());
    }

    public static string MusicResourcePath(MusicTrack track, string filename) {
        return $"music/{track}/{filename}";
    }
    public static string MusicSingleResourcePath(MusicTrack track) {
        string filename = MUSIC_PATHS[track][0];
        return $"music/{filename}";
    }

    override public void OnDestroy() {
        base.OnDestroy();
        StopAllMixes();
    }

    public void PlayMissionTrack(MusicTrack track) {
        if (controllers.Count > 0) {
            if (controllers.Peek().track == track) return;
        }
        StopAllMixes();
        MissionMusicController missionMusicController = new MissionMusicController(track, audioSources);
        EnqueueMix(missionMusicController);
    }
    public void PlaySimpleTrack(MusicTrack track) {
        if (controllers.Count > 0) {
            if (controllers.Peek().track == track) return;
        }
        StopAllMixes();
        SimpleMusicController simpleMusicController = new SimpleMusicController(track, audioSources);
        EnqueueMix(simpleMusicController);
    }

    void StopAllMixes() {
        while (controllers.Count > 0) {
            MusicMixController controller = controllers.Pop();
            controller.Stop();
        }
    }

    void EnqueueMix(MusicMixController controller) {
        if (controllers.Count > 0) {
            controllers.Peek().Pause();
        }
        controllers.Push(controller);
        controller.Play();
    }

}




