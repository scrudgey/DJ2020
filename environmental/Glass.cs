using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Glass : MonoBehaviour {
    public float health = 100f;
    public AudioClip[] hitSounds;
    public AudioClip[] destroySounds;
    public AudioSource audioSource;
    public List<GameObject> decals = new List<GameObject>();
    public GameObject glassGibs;
    private PrefabPool glassGibsPool;
    RaycastHit lastImpact;
    Ray lastRay;
    public bool doDestruct;
    public void Awake() {
        glassGibsPool = PoolManager.I.RegisterPool(glassGibs);
    }
    public void BulletHit(BulletImpact impact) {
        GameObject decalObject = PoolManager.I.CreateDecal(impact.hit, PoolManager.DecalType.glass);
        decalObject.transform.SetParent(transform, true);
        decals.Add(decalObject);

        health -= impact.bullet.damage;
        Toolbox.AudioSpeaker(impact.hit.point, hitSounds);
        if (health <= 0) {
            doDestruct = true;
        }
        lastImpact = impact.hit;
        lastRay = impact.bullet.ray;
    }
    public void FixedUpdate() {
        if (doDestruct) {
            doDestruct = false;
            Destruct();
        }
    }
    public void Destruct() {
        Toolbox.AudioSpeaker(transform.position, destroySounds);
        Destroy(gameObject);
        PoolManager.I.RecallObjects(decals.ToArray());
        // TODO: amortize this expensive operation
        Collider myCollider = GetComponent<Collider>();
        for (int i = 0; i < Random.Range(10, 20); i++) {
            Vector3 initialPosition = myCollider.bounds.center;
            initialPosition += Random.Range(-1.2f, 1.2f) * Vector3.Cross(Vector3.up, lastImpact.normal);
            initialPosition += Random.Range(-0.1f, 0.1f) * lastRay.direction;
            initialPosition += Random.Range(-0.25f, 0.25f) * transform.up;
            GameObject shard = glassGibsPool.GetObject(initialPosition);
            Rigidbody body = shard.GetComponent<Rigidbody>();
            body?.AddForce(
                UnityEngine.Random.Range(-0.5f, 1.5f) * transform.up +
                // UnityEngine.Random.Range(0.1f, 1f) * transform.right +
                UnityEngine.Random.Range(1.5f, 5.5f) * lastRay.direction,
                ForceMode.VelocityChange); // TODO: what does force mode mean?
            body.AddRelativeTorque(UnityEngine.Random.Range(100f, 600f) * Random.insideUnitSphere);
        }
    }
}
