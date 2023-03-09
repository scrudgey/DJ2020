using UnityEngine;

public class BulletDamage : Damage {
    public RaycastHit hit;
    public Bullet bullet;
    public BulletDamage(Bullet bullet, RaycastHit hit) : base(bullet.damage, bullet.ray.direction, hit.point, bullet.source) {
        this.bullet = bullet;
        this.hit = hit;
        this.direction = bullet.ray.direction;
    }
}