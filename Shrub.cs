using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class Shrub : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] sounds;
    private float shakeAmount;
    public float shakeThreshold = 100f;

    public List<Coroutine> shakeRoutines = new List<Coroutine>();

    private static Dictionary<Collider, NeoCharacterController> bodies = new Dictionary<Collider, NeoCharacterController>();
    public float leafProbability = 0.5f;
    new private Collider collider;
    void Awake() {
        collider = GetComponent<Collider>();
    }
    public NeoCharacterController GetRigidbody(Collider key) {
        NeoCharacterController outBody;
        if (bodies.TryGetValue(key, out outBody)) {
            return outBody;
        } else {
            outBody = key.transform.root.GetComponentInChildren<NeoCharacterController>();
            bodies[key] = outBody;
            return outBody;
        }

    }
    void OnTriggerStay(Collider other) {
        NeoCharacterController body = GetRigidbody(other);
        if (body != null) {
            shakeAmount += body.Motor.Velocity.magnitude;
            // Debug.Log(shakeAmount);
            if (shakeAmount > shakeThreshold) {
                Shake();
                shakeAmount = 0f;
            }
        }
    }
    void OnTriggerEnter(Collider other) {
        // Shake();
    }
    public void Shake() {
        Toolbox.RandomizeOneShot(audioSource, sounds);
        ClearCoroutines();
        Coroutine newRoutine = StartCoroutine(ShakeTree(transform.parent));
        shakeRoutines.Add(newRoutine);

        if (Random.Range(0f, 1f) < leafProbability) {
            PoolManager.I.leafPool.SpawnDecal(Toolbox.RandomInsideBounds(collider, padding: 1.5f));
        }
    }
    public IEnumerator ShakeTree(Transform target) {
        // Debug.Log(target);
        // Transform target = tree;
        Quaternion initial = target.rotation;

        float timer = 0f;
        float length = Random.Range(0.5f, 1.2f);
        float intensity = Random.Range(5f, 15f);

        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 axis = new Vector3(randomCircle.x, 0f, randomCircle.y);

        while (timer < length) {
            timer += Time.deltaTime;

            float angle = (float)PennerDoubleAnimation.ElasticEaseOut(timer, intensity, -intensity, length);
            target.rotation = Quaternion.AngleAxis(angle, axis);
            yield return null;
        }

        target.rotation = initial;
    }
    public void ClearCoroutines() {
        foreach (Coroutine routine in shakeRoutines) {
            StopCoroutine(routine);
        }
        shakeRoutines.RemoveAll((_) => true);
    }
    private void OnDestroy() {
        ClearCoroutines();
    }
}
