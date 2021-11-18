using UnityEngine;

public enum DamageType { normal, bullet, explosion }
public class Damage {
    // public DamageType type;
    public float amount;
    public Vector3 direction;
    public Vector3 position;
    public Vector3 force;
    public Damage(float amount, Vector3 direction, Vector3 position) {
        // this.type = type;
        this.amount = amount;
        this.direction = direction;
        this.position = position;
        this.force = amount * direction;
    }
}
public class ExplosionDamage : Damage {
    public ExplosionDamage(float amount, Vector3 direction, Vector3 position) : base(amount, direction, position) { }
}