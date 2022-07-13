using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet {
    public float damage;
    public float range;
    public Vector3 gunPosition;
    public Ray ray;
    public Bullet(Ray ray) {
        this.ray = ray;
    }

    public void DoImpacts() {
        RaycastHit[] hits = Physics.RaycastAll(ray, range, LayerUtil.GetMask(Layer.def, Layer.obj, Layer.interactive));
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (Impact(hit)) {
                Debug.DrawLine(gunPosition, hit.point, Color.green, 5f);
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

        DamageResult result = DamageResult.NONE;
        foreach (IDamageReceiver receiver in hit.transform.GetComponentsInChildren<IDamageReceiver>()) {
            if (receiver is Damageable damageable) {
                result = result.Add(damageable.TakeDamage(bulletDamage));
            } else {
                receiver.TakeDamage(bulletDamage);
            }
        }

        Rigidbody body = hit.transform.GetComponent<Rigidbody>();
        if (body != null) {
            body?.AddForceAtPosition(damage * -1f * hit.normal, hit.point, ForceMode.Impulse);
        }

        if (!tagData.bulletPassthrough) {
            // spawn decal
            // TODO: make this data driven
            if (!tagData.noDecal) {
                GameObject decalObject = PoolManager.I.CreateDecal(hit, PoolManager.DecalType.normal);
                decalObject.transform.SetParent(hit.collider.transform, true);
            }

            // spawn sparks by default
            if (result.Equals(DamageResult.NONE))
                GameObject.Instantiate(
                    Resources.Load("prefabs/fx/impactSpark"),
                    hit.point + (hit.normal * 0.025f),
                    Quaternion.LookRotation(hit.normal)
                    );

            return true;
        }
        return false;
    }

    public void SpawnBulletRay(Vector3 startPoint, Vector3 endPoint) {
        GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/fx/bulletRay"), gunPosition, Quaternion.identity) as GameObject;
        BulletFX ray = obj.GetComponent<BulletFX>();
        ray.Initialize(BulletFX.FadeStyle.streak, startPoint, endPoint);
        // ray.Initialize(BulletFX.FadeStyle.count, startPoint, endPoint);
    }
}
