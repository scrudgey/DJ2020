using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Bullet {
    public float damage;
    public float range;
    public Vector3 gunPosition;
    public Ray ray;
    public Bullet(Ray ray) {
        this.ray = ray;
    }

    public void DoImpacts() {
        RaycastHit[] hits = Physics.RaycastAll(ray, range); // get all hits
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (Impact(hit)) {
                // TODO: reinstate the random effect

                if (UnityEngine.Random.Range(0f, 1f) < 0.25f) {
                    SpawnBulletRay(gunPosition, hit.point);
                }
                break;
            }
        }

    }

    bool Impact(RaycastHit hit) {
        BulletDamage bulletDamage = new BulletDamage(this, hit);

        TagSystemData tagData = Toolbox.GetTagData(hit.collider.gameObject);

        // TODO: unify with others
        foreach (IDamageable damageable in hit.transform.GetComponentsInChildren<IDamageable>()) {
            damageable.TakeDamage(bulletDamage);
        }

        Rigidbody body = hit.transform.GetComponent<Rigidbody>();
        if (body != null) {
            body?.AddForceAtPosition(damage * -1f * hit.normal, hit.point, ForceMode.Impulse);
        }

        if (!tagData.bulletPassthrough) {
            // TODO: these effects should be data-driven

            // spawn decal
            GameObject decalObject = PoolManager.I.CreateDecal(hit, PoolManager.DecalType.normal);
            decalObject.transform.SetParent(hit.collider.transform, true);

            // spawn sparks
            GameObject.Instantiate(
                Resources.Load("prefabs/impactSpark"),
                hit.point + (hit.normal * 0.025f),
                Quaternion.LookRotation(hit.normal)
                );

            return true;
        }
        return false;
    }

    public void SpawnBulletRay(Vector3 startPoint, Vector3 endPoint) {
        GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/bulletRay"), gunPosition, Quaternion.identity) as GameObject;
        BulletFX ray = obj.GetComponent<BulletFX>();
        ray.Initialize(BulletFX.FadeStyle.streak, startPoint, endPoint);
        // ray.Initialize(BulletFX.FadeStyle.count, startPoint, endPoint);
    }
}
