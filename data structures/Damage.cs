using UnityEngine;

public class Damage {
    public float amount;
    public Vector3 direction;
    public Vector3 position;
    public Vector3 force;
    public Damage(float amount, Vector3 direction, Vector3 position) {
        this.amount = amount;
        this.direction = direction;
        this.position = position;
        this.force = amount * direction;
    }
    public Damage() { }

    public static readonly Damage NONE = new Damage() {
        amount = 0,
        direction = Vector3.zero,
        position = Vector3.zero,
        force = Vector3.zero
    };
}

public class ExplosionDamage : Damage {
    public ExplosionDamage(float amount, Vector3 direction, Vector3 position) : base(amount, direction, position) { }
}