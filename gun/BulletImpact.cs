using UnityEngine;

public class BulletDamage : Damage {
    public RaycastHit hit;
    public Bullet bullet;
    public BulletDamage(Bullet bullet, RaycastHit hit) : base(DamageType.bullet, bullet.damage, bullet.ray.direction, hit.point) {
        this.bullet = bullet;
        this.hit = hit;
    }
}