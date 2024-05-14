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
    public string Description() {
        string description = "";
        if (armorPiercing != 0) {
            description += $"{sign(armorPiercing)} armor piercing\n";
        }
        if (shootInterval != 0) {
            description += $"{sign(1f / shootInterval)} fire rate\n";
        }
        if (noise != 0) {
            description += $"{sign(noise)} noise\n";
        }
        if (clipSize != 0) {
            description += $"{sign(clipSize)} clip size\n";
        }
        if (spread != 0) {
            description += $"{sign(1 / spread)} accuracy\n";
        }
        if (lockOnSize != 0) {
            description += $"{sign(lockOnSize)} lock radius\n";
        }
        if (baseDamage.low != 0 && baseDamage.high != 0) {
            description += $"{sign(baseDamage)} damage\n";
        }
        if (weight != 0) {
            description += $"{sign(weight)} weight\n";
        }
        if (cost != 0) {
            description += $"{sign(cost)} cost\n";
        }
        return description;
    }

    string sign(int amount) => amount > 0 ? $"+{amount}" : $"{amount}";
    string sign(float amount) => amount > 0 ? $"+{amount}" : $"{amount}";
    string sign(LoHi amount) {
        if (amount.low >= 0 && amount.high >= 0) {
            return $"+{amount.low}-{amount.high}";
        } else if (amount.low <= 0 && amount.high <= 0) {
            return $"-{amount.low}-{amount.high}";
        } else if (amount.low <= 0 && amount.high >= 0) {
            return $"-{amount.low}-+{amount.high}";
        } else {
            return $"{amount.low}-{amount.high}";
        }
    }

}