using UnityEngine;

public enum DamageType { normal, bullet, explosion }
public class Damage {
    public DamageType type;
    public float amount;
    public Vector3 direction;
    public Vector3 position;
    public Damage(DamageType type, float amount, Vector3 direction, Vector3 position) {
        this.type = type;
        this.amount = amount;
        this.direction = direction;
        this.position = position;
    }
    public virtual Vector3 GetDamageAtPoint(Vector3 point) {
        return direction.normalized * amount;
    }
}
public class ExplosionDamage : Damage {
    // public Vector3 source;
    public float range;
    public ExplosionDamage(Explosion explosion) : base(DamageType.explosion, explosion.power, Vector3.up, explosion.transform.position) {
        // TODO: this is a hack
        this.range = explosion.radius;
    }
    public ExplosionDamage(Vector3 source, float range, float amount, Vector3 direction) : base(DamageType.explosion, amount, direction, source) {
        this.range = range;
    }
    public override Vector3 GetDamageAtPoint(Vector3 point) {
        Vector3 direction = (point - position).normalized;
        float dist = (point - position).magnitude;
        if (dist > range)
            return Vector3.zero;
        return (1.0f - dist / range) * amount * direction;
    }
}