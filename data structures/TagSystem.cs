using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceType { normal, metal, grass, tree }

[System.Serializable]
public class TagSystemData {

    // if true, don't block bullet raycasts, and on collision check for Glass component to take damage.
    public bool bulletPassthrough;

    // if true, I cause the entry chime to play
    public bool isActor;

    // if true, I do not hide when between the player and camera
    public bool dontHideInterloper;

    // if true, I do not hide when above the player
    public bool dontHideAbove;

    // if greater than 0, the player will target this object when mouse over
    public int targetPriority = -1;

    // determines the sound footsteps make when the player is walking on me
    public SurfaceType surfaceSoundType;
}
public class TagSystem : MonoBehaviour {
    public TagSystemData data;
}
