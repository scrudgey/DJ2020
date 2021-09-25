using UnityEngine;

public class BulletImpact {
    public RaycastHit hit;
    public Bullet bullet;
    public BulletImpact(Bullet bullet, RaycastHit hit) {
        this.bullet = bullet;
        this.hit = hit;
    }
}