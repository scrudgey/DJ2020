using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GunModType { none, silencer, clipSize, damage, fireRate }
[CreateAssetMenu(menuName = "ScriptableObjects/GunMod")]
public class GunMod : ScriptableObject, IGunStatProvider {
    public GunModType type;
    public string title;
    [TextArea(15, 20)]
    public string description;
    public int cost;
    public string requiredSpriteSuffix;
    [Header("stats")]
    public float shootInterval;
    public float noise;
    public int clipSize;
    public float spread;
    public float shootInaccuracy;
    public float lockOnSize;
    public LoHi baseDamage;


    public GunStats GetGunStats() => new GunStats {
        shootInterval = shootInterval,
        noise = noise,
        clipSize = clipSize,
        spread = spread,
        shootInaccuracy = shootInaccuracy,
        lockOnSize = lockOnSize,
        baseDamage = baseDamage,
        recoil = new LoHi(0, 0)
    };
}