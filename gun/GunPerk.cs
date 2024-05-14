using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GunPerk")]
public class GunPerk : ScriptableObject, IGunStatProvider {
    public enum GunPerkType { cost, damage, accuracy, armorPiercing }
    public GunPerkType type;
    public int cost;
    public float probability;
    [Header("stats")]
    public int armorPiercing;
    public float shootInterval;
    public float noise;
    public int clipSize;
    public float spread;
    public float lockOnSize;
    public LoHi baseDamage;
    public float weight;

    public GunStats GetGunStats() => new GunStats {
        shootInterval = shootInterval,
        noise = noise,
        clipSize = clipSize,
        spread = spread,
        lockOnSize = lockOnSize,
        baseDamage = baseDamage,
        recoil = new LoHi(0, 0),
        weight = weight
    };
}