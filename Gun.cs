using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Gun")]
public class Gun : ScriptableObject {
    [Header("Stats")]
    public float shootInterval;
    public bool automatic;
    public float muzzleflashSize = 0.025f;
    public int clipSize;
    public float range;
    public float spread;

    [Header("Resources")]
    public AudioClip[] shootSounds;
    public AudioClip[] clipOut;
    public AudioClip[] unholster;
    public AnimationClip shootAnimation;
    public AnimationClip walkAnimation;
    public Octet<Sprite> idle;
    public Octet<Sprite[]> walk;
    public Octet<Sprite[]> shoot;
    public GameObject muzzleFlash;
    public GameObject shellCasing;
    public static Gun Load(string name) {
        return Resources.Load($"data/guns/{name}") as Gun;
    }

}
