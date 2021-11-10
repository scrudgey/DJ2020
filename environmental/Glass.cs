using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Glass : IDestructible {
    public AudioClip[] hitSounds;
    public AudioClip[] destroySounds;
    public AudioSource audioSource;
    public List<GameObject> decals = new List<GameObject>();
    public GameObject glassGibs;
    private PrefabPool glassGibsPool;
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

    override protected void Destruct(Damage damage) {
        base.Destruct(damage);

        Toolbox.AudioSpeaker(transform.position, destroySounds);
        Destroy(gameObject);
        PoolManager.I.RecallObjects(decals.ToArray());
        // TODO: amortize this expensive operation
        Collider myCollider = GetComponent<Collider>();
        for (int i = 0; i < Random.Range(20, 30); i++) {

            Vector3 initialPosition = new Vector3(
                Random.Range(-myCollider.bounds.extents.x, myCollider.bounds.extents.x),
                Random.Range(-myCollider.bounds.extents.y, myCollider.bounds.extents.y),
                Random.Range(-myCollider.bounds.extents.z, myCollider.bounds.extents.z)
            ) + myCollider.bounds.center;

            // project outward the closer we are to the center
            Vector3 displacement = damage.position - initialPosition;
            float distance = displacement.magnitude;
            initialPosition += 0.5f * lastDamage.direction / (Mathf.Max(0.01f, distance)); //  Random.Range(0.05f, 0.1f) *

            GameObject shard = glassGibsPool.GetObject(initialPosition);
            Rigidbody body = shard.GetComponent<Rigidbody>();
            body?.AddForce(
                        // UnityEngine.Random.Range(0.75f, 1.5f) * transform.up +
                        UnityEngine.Random.Range(1.5f, 5.5f) * lastDamage.direction,
                        ForceMode.VelocityChange);
            body.AddRelativeTorque(UnityEngine.Random.Range(100f, 600f) * Random.insideUnitSphere);
            Collider gibsCollider = shard.GetComponentInChildren<Collider>();
            Physics.IgnoreCollision(gibsCollider, myCollider, true);
        }
        // Debug.Break();
    }
}