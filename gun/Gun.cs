using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GunType { unarmed, pistol, smg, shotgun, rifle, sword }
public enum CycleType { manual, semiautomatic, automatic }

[CreateAssetMenu(menuName = "ScriptableObjects/Gun")]
public class Gun : ScriptableObject {
    new public string name;

    // TODO: separate guntype as graphics from gun functionality. allow gun to specify manual cycling, e.g.
    public GunType type;

    [Header("Stats")]
    public CycleType cycle;
    public float shootInterval;
    public float muzzleflashSize = 0.025f;
    public float noise = 3f;
    public int clipSize;
    public float range;
    public float spread;
    public float shootInaccuracy;
    public LoHi baseDamage;
    public Sprite image;

    [Header("Resources")]
    public AudioClip[] shootSounds;
    public AudioClip[] clipOut;
    public AudioClip[] clipIn;
    public AudioClip[] unholster;
    public AudioClip[] aimSounds;
    public AudioClip[] rackSounds;
    public AnimationClip shootAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip rackAnimation;
    public AnimationClip reloadAnimation;
    public GameObject muzzleFlash;
    public GameObject shellCasing;
    public GameObject magazine;
    public static Gun Load(string name) {
        return Resources.Load($"data/guns/{name}") as Gun;
    }
    public float getBaseDamage() {
        return Random.Range(baseDamage.low, baseDamage.high);
    }
    public NoiseData shootNoise() {
        return new NoiseData() {
            volume = noise,
            suspiciousness = Suspiciousness.aggressive
        };
    }
}
