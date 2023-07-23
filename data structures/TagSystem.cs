using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceType { normal, metal, grass, tree, tile, carpet }

[System.Serializable]
public class TagSystemData {

    // if true, don't block bullet raycasts, and on collision check for Glass component to take damage.
    public bool bulletPassthrough;

    public bool noDecal;

    // if true, I cause the entry chime to play
    public bool isActor;

    // if true, I do not hide when between the player and camera
    // public bool dontHideInterloper; // TODO: use layers instead
    // currently used by: transparent objects

    // if true, I do not hide when above the player
    public bool dontHideAbove;  // TODO: use layers instead

    // if true, I do not hide when above the player
    public bool dontHideInterloper;  // TODO: use layers instead

    // if greater than 0, the player will target this object when mouse over
    public int targetPriority = -1;

    // determines the sound footsteps make when the player is walking on me
    public SurfaceType surfaceSoundType;

    // if set, aiming at this object will point here. otherwise, collider bounds center
    public Transform targetPoint;

    public bool debugClearSighter;
}
public class TagSystem : MonoBehaviour {
    public TagSystemData data;
}
