using UnityEngine;

public class Damage {
    public float amount;
    public Vector3 direction;
    public Vector3 position;
    public Vector3 force;   // TODO: difference between force and direction?
    public Vector3 source;
    public Damage(float amount, Vector3 direction, Vector3 position, Vector3 source) {
        this.amount = amount;
        this.direction = direction;
        this.position = position;
        this.force = amount * direction;
        this.source = source;
    }
    public Damage() { }

    public static readonly Damage NONE = new Damage() {
        amount = 0,
        direction = Vector3.zero,
        position = Vector3.zero,
        force = Vector3.zero,
        source = Vector3.zero
    };
}

public class ExplosionDamage : Damage {
    public ExplosionDamage(float amount, Vector3 direction, Vector3 position, Vector3 source) : base(amount, direction, position, source) { }
}