using UnityEngine;

public enum DamageType { normal, bullet, explosion }
public class Damage {
    public DamageType type;
    public float amount;
    public Vector3 direction;
    public Damage(DamageType type, float amount, Vector3 direction) {
        this.type = type;
        this.amount = amount;
        this.direction = direction;
    }
    public virtual Vector3 GetDamageAtPoint(Vector3 point) {
        return direction.normalized * amount;
    }
}
public class ExplosionDamage : Damage {
    public Vector3 source;
    public float range;
    public ExplosionDamage(Explosion explosion) : base(DamageType.explosion, explosion.power, Vector3.up) {
        // TODO: this is a hack
        this.source = explosion.transform.position;
        this.range = explosion.radius;
    }
    public ExplosionDamage(Vector3 source, float range, float amount, Vector3 direction) : base(DamageType.explosion, amount, direction) {
        this.source = source;
        this.range = range;
    }
    public override Vector3 GetDamageAtPoint(Vector3 point) {
        Vector3 direction = (point - source).normalized;
        float dist = (point - source).magnitude;
        if (dist > range)
            return Vector3.zero;
        return (1.0f - dist / range) * amount * direction;
    }
}