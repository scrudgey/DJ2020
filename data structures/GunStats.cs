public class GunStats {
    public float shootInterval;
    public float noise;
    public int clipSize;
    public float spread;
    public float lockOnSize;
    public LoHi baseDamage;
    public LoHi recoil;

    public static GunStats operator +(GunStats a, GunStats b) => new GunStats {
        shootInterval = a.shootInterval + b.shootInterval,
        noise = a.noise + b.noise,
        clipSize = a.clipSize + b.clipSize,
        spread = a.spread + b.spread,
        lockOnSize = a.lockOnSize + b.lockOnSize,
        baseDamage = a.baseDamage + b.baseDamage,
        recoil = a.recoil + b.recoil
    };
}