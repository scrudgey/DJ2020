using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Glass : IDamageable {
    public AudioClip[] hitSounds;
    public AudioClip[] destroySounds;
    public AudioSource audioSource;
    public List<GameObject> decals = new List<GameObject>();
    public GameObject glassGibs;
    private PrefabPool glassGibsPool;
    public bool doDestruct;
    Collider myCollider;
    public void Awake() {
        glassGibsPool = PoolManager.I.RegisterPool(glassGibs);
        myCollider = GetComponent<Collider>();
        RegisterDamageCallback<BulletDamage>(TakeBulletDamage);
        RegisterDamageCallback<ExplosionDamage>(TakeExplosionDamage);
    }
    public DamageResult TakeBulletDamage(BulletDamage damage) {
        GameObject decalObject = PoolManager.I.CreateDecal(damage.hit, PoolManager.DecalType.glass);
        decalObject.transform.SetParent(transform, true);
        decals.Add(decalObject);

        Toolbox.AudioSpeaker(damage.hit.point, hitSounds);
        return new DamageResult {
            damageAmount = damage.amount
        };
    }
    public DamageResult TakeExplosionDamage(ExplosionDamage explosion) {
        Vector3 myPosition = transform.position;
        if (myCollider != null) {
            myPosition = myCollider.bounds.center;
        }
        Vector3 force = explosion.GetDamageAtPoint(myPosition);
        return new DamageResult {
            damageAmount = force.magnitude * 100
        };
    }

    // TODO: move to IDamageable
    // public void FixedUpdate() {
    //     if (doDestruct) {
    //         doDestruct = false;
    //         Destruct();
    //     }
    // }
    override protected void Destruct(Damage damage) {
        Toolbox.AudioSpeaker(transform.position, destroySounds);
        Destroy(gameObject);
        PoolManager.I.RecallObjects(decals.ToArray());
        // TODO: amortize this expensive operation
        Collider myCollider = GetComponent<Collider>();
        for (int i = 0; i < Random.Range(10, 20); i++) {
            Vector3 initialPosition = myCollider.bounds.center;
            initialPosition += Random.Range(-1.2f, 1.2f) * Vector3.Cross(Vector3.up, lastDamage.direction);
            initialPosition += Random.Range(-0.1f, 0.1f) * lastDamage.direction;
            initialPosition += Random.Range(-0.25f, 0.25f) * transform.up;
            GameObject shard = glassGibsPool.GetObject(initialPosition);
            Rigidbody body = shard.GetComponent<Rigidbody>();
            body?.AddForce(
                UnityEngine.Random.Range(-0.5f, 1.5f) * transform.up +
                // UnityEngine.Random.Range(0.1f, 1f) * transform.right +
                UnityEngine.Random.Range(1.5f, 5.5f) * lastDamage.direction,
                ForceMode.VelocityChange); // TODO: what does force mode mean?
            body.AddRelativeTorque(UnityEngine.Random.Range(100f, 600f) * Random.insideUnitSphere);
        }
    }
}
