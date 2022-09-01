using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/FootstepData")]
public class FootstepData : ScriptableObject {
    public AudioClip[] grassSounds;
    public AudioClip[] defaultSounds;
    public AudioClip[] metalSounds;
    public AudioClip[] bushSounds;
    public AudioClip[] tileSounds;
    public AudioClip[] GetSoundSet(SurfaceType surfaceType) {
        return surfaceType switch {
            SurfaceType.grass => grassSounds,
            SurfaceType.metal => metalSounds,
            SurfaceType.normal => defaultSounds,
            SurfaceType.tree => bushSounds,
            SurfaceType.tile => tileSounds,
            _ => defaultSounds
        };
    }
}
