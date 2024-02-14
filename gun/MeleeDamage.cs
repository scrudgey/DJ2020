using UnityEngine;

public class MeleeDamage : Damage {
    public MeleeDamage(float amount, Vector3 source, Vector3 point) : base(amount, (point - source).normalized, point, source) {

    }
}