using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GunType { unarmed, pistol, smg, shotgun, rifle, sword }
public enum CycleType { manual, semiautomatic, automatic }

[CreateAssetMenu(menuName = "ScriptableObjects/GunTemplate")]
public class GunTemplate : ScriptableObject, IGunStatProvider {
    new public string name;
    public string shortName;
    [TextArea(15, 20)]
    public string shopDescription;
    public int baseCost;
    public float likelihoodWeight;

    // TODO: separate guntype as graphics from gun functionality. allow gun to specify manual cycling, e.g.
    public GunType type;

    [Header("Stats")]
    public CycleType cycle;
    public float shootInterval;
    public float muzzleflashSize = 0.025f;
    public float noise = 3f;
    public float pitch = 1f;
    public int clipSize;
    public float range;
    public float spread;
    public float shotgunSpread;
    public float lockOnSize = 1f;
    public LoHi baseDamage;
    public List<Sprite> images;
    public bool silencer;
    public float weight;
    public LoHi recoil;
    public List<GunMod> availableMods;
    public int piercing;

    public List<GunPerk> possiblePerks;
    public List<GunPerk> intrinsicPerks;

    [Header("Resources")]
    public AudioClip[] shootSounds;
    public AudioClip[] silencedSounds;
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
    public static GunTemplate Load(string name) {
        return Resources.Load($"data/guns/{name}") as GunTemplate;
    }

    public GunStats GetGunStats() => new GunStats {
        shootInterval = shootInterval,
        noise = noise,
        clipSize = clipSize,
        spread = spread,
        lockOnSize = lockOnSize,
        baseDamage = baseDamage,
        recoil = recoil,
        weight = weight
    };
}
