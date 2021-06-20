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
    RaycastHit lastImpact;
    Ray lastRay;
    public bool doDestruct;
    public void BulletHit(RaycastHit hit, Ray ray) {

        GameObject decalObject = DecalPool.I.SpawnDecal(hit, DecalPool.DecalType.glass);
        decals.Add(decalObject);
        health -= Random.Range(5f, 10f);
        Toolbox.RandomizeOneShot(audioSource, hitSounds);
        if (health <= 0) {
            doDestruct = true;
        }
        lastImpact = hit;
        lastRay = ray;
        // var sparks = Resources.Load("prefabs/impactSpark");
        // GameObject sparkObject = GameObject.Instantiate(sparks,
        // hit.point + (hit.normal * 0.025f),
        // Quaternion.LookRotation(hit.normal)) as GameObject;
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
        DecalPool.I.RecallDecals(decals.ToArray()); // return decals to the pool
        // TODO: amortize this expensive operation
        // TODO: use a pooling structure
        for (int i = 0; i < Random.Range(10, 20); i++) {
            Vector3 initialPosition = transform.position;
            initialPosition += Random.Range(-1.2f, 1.2f) * Vector3.Cross(Vector3.up, lastImpact.normal);
            initialPosition += Random.Range(-0.1f, 0.1f) * lastRay.direction;
            initialPosition += Random.Range(-0.25f, 0.25f) * transform.up;
            GameObject shard = GameObject.Instantiate(
                glassGibs,
                initialPosition,
                Random.rotation);
            Rigidbody body = shard.GetComponent<Rigidbody>();
            body.AddForce(
                UnityEngine.Random.Range(-0.5f, 1.5f) * transform.up +
                // UnityEngine.Random.Range(0.1f, 1f) * transform.right +
                UnityEngine.Random.Range(1.5f, 5.5f) * lastRay.direction,
                ForceMode.VelocityChange); // TODO: what does force mode mean?
            body.AddRelativeTorque(UnityEngine.Random.Range(100f, 600f) * Random.insideUnitSphere);
        }
    }
}
