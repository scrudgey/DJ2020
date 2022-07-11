public struct DamageResult {
    public float damageAmount;
    public Damage damage;

    public DamageResult Add(DamageResult other) {
        return new DamageResult {
            damageAmount = damageAmount + other.damageAmount,
            damage = other.damage
        };
    }

    public readonly static DamageResult NONE = new DamageResult() {
        damage = Damage.NONE
    };
}