using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GunType { unarmed, pistol, smg, shotgun, rifle, sword }

[CreateAssetMenu(menuName = "ScriptableObjects/Gun")]
public class Gun : ScriptableObject {

    // TODO: separate guntype as graphics from gun functionality. allow gun to specify manual cycling, e.g.
    public GunType type;

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
    public AudioClip[] aimSounds;
    public AudioClip[] rackSounds;
    public AnimationClip shootAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip rackAnimation;
    public GameObject muzzleFlash;
    public GameObject shellCasing;
    public static Gun Load(string name) {
        return Resources.Load($"data/guns/{name}") as Gun;
    }

}
