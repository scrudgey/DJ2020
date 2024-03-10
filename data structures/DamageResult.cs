using System;

public struct DamageResult : IEquatable<DamageResult> {
    public enum Type { none, impact, blocked }
    public DamageResult.Type type;
    public float damageAmount;
    public Damage damage;

    public DamageResult Add(DamageResult other) {
        Type finalType = type;
        if (type == Type.blocked || other.type == Type.blocked) {
            finalType = Type.blocked;
        }
        return new DamageResult {
            damageAmount = damageAmount + other.damageAmount,
            // damage = other.damage
            damage = damage,
            type = finalType
        };
    }

    public readonly static DamageResult NONE = new DamageResult() {
        damage = Damage.NONE,
        type = Type.none
    };


    public override bool Equals(object? obj) => obj is DamageResult other && this.Equals(other);

    public bool Equals(DamageResult p) => type == p.type && damageAmount == p.damageAmount;  // TODO: COMPARE damage as well

    public override int GetHashCode() => (type, damageAmount, damage).GetHashCode();

    public static bool operator ==(DamageResult lhs, DamageResult rhs) => lhs.Equals(rhs);

    public static bool operator !=(DamageResult lhs, DamageResult rhs) => !(lhs == rhs);
}