using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
public class Bullet {
    public float damage;
    public float range;
    public Vector3 gunPosition;
    public Ray ray;
    public Vector3 source;
    public Bullet(Ray ray) {
        this.ray = ray;
    }

    public void DoImpacts(Transform shooter) {
        // TODO: nonalloc
        RaycastHit[] hits = Physics.RaycastAll(ray, range, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive, Layer.bulletPassThrough, Layer.bulletOnly), QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.collider.transform.IsChildOf(shooter))
                continue;
            if (Impact(hit)) {
                Debug.DrawLine(gunPosition, hit.point, Color.green, 5f);
                if (UnityEngine.Random.Range(0f, 1f) < 0.25f) {
                    SpawnBulletRay(gunPosition, hit.point);
                }
                break;
            }
        }

    }
    // void Test() {
    //     // Perform a single raycast using RaycastCommand and wait for it to complete
    //     // Setup the command and result buffers
    //     var results = new NativeArray<RaycastHit>(1, Allocator.Temp);

    //     var commands = new NativeArray<RaycastCommand>(1, Allocator.Temp);

    //     // Set the data of the first command
    //     Vector3 origin = Vector3.forward * -10;

    //     Vector3 direction = Vector3.forward;

    //     commands[0] = new RaycastCommand(origin, direction);
    //     // new RaycastCommand()

    //     // Schedule the batch of raycasts
    //     JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));

    //     handle.IsCompleted

    //     // Wait for the batch processing job to complete
    //     handle.Complete();

    //     // Copy the result. If batchedHit.collider is null there was no hit
    //     RaycastHit batchedHit = results[0];

    //     // Dispose the buffers
    //     results.Dispose();
    //     commands.Dispose();
    // }

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
            body?.AddForceAtPosition(damage * -1f * hit.normal / 10f, hit.point, ForceMode.Impulse);
        }

        if (!tagData.bulletPassthrough) {
            // spawn decal
            // TODO: make this data driven
            if (!tagData.noDecal) {
                GameObject decalObject = PoolManager.I.CreateDecal(hit, PoolManager.DecalType.normal);
                decalObject.transform.SetParent(hit.collider.transform, true);
            }

            // spawn sparks by default
            if (result.Equals(DamageResult.NONE)) {
                PrefabPool pool = PoolManager.I.GetPool("prefabs/fx/impactSpark");
                GameObject sparkObject = pool.GetObject(hit.point + (hit.normal * 0.025f));
                sparkObject.transform.rotation = Quaternion.LookRotation(hit.normal);
            }

            return true;
        }
        return false;
    }

    public void SpawnBulletRay(Vector3 startPoint, Vector3 endPoint) {
        // TODO: use pooling
        // GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/fx/bulletRay"), gunPosition, Quaternion.identity) as GameObject;
        GameObject obj = PoolManager.I.GetPool("prefabs/fx/bulletRay").GetObject(gunPosition);
        BulletFX ray = obj.GetComponent<BulletFX>();
        ray.Initialize(BulletFX.FadeStyle.streak, startPoint, endPoint);
        // ray.Initialize(BulletFX.FadeStyle.count, startPoint, endPoint);
    }
}
