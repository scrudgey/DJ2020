using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

public class Bush : MonoBehaviour {
    public GameObject leafPrefab;
    private PrefabPool leafPool;
    public AudioSource audioSource;
    public AudioClip[] sounds;
    private float shakeAmount;
    public float shakeThreshold = 100f;

    public List<Coroutine> shakeRoutines = new List<Coroutine>();

    private static Dictionary<Collider, CharacterController> bodies = new Dictionary<Collider, CharacterController>();
    public float leafProbability = 0.5f;
    new private Collider collider;

    Quaternion initialRotation;
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        collider = GetComponent<Collider>();
        leafPool = PoolManager.I.RegisterPool(leafPrefab);
        initialRotation = transform.rotation;
    }
    public CharacterController GetRigidbody(Collider key) {
        CharacterController outBody;
        if (bodies.TryGetValue(key, out outBody)) {
            return outBody;
        } else {
            outBody = key.transform.root.GetComponentInChildren<CharacterController>();
            bodies[key] = outBody;
            return outBody;
        }

    }
    void OnTriggerStay(Collider other) {
        CharacterController body = GetRigidbody(other);
        if (body != null) {
            body.EnterBush();
            shakeAmount += body.Motor.Velocity.magnitude;
            // Debug.Log(shakeAmount);
            if (shakeAmount > shakeThreshold) {
                Shake(other);
                shakeAmount = 0f;
            }
        }
    }
    void OnTriggerEnter(Collider other) {
        // Shake();
    }
    public void Shake(Collider other) {
        Toolbox.RandomizeOneShot(audioSource, sounds);
        Toolbox.Noise(transform.position, new NoiseData() {
            volume = 4,
            suspiciousness = Suspiciousness.suspicious,
            player = other.transform.IsChildOf(GameManager.I.playerObject.transform)
        }, other.transform.root.gameObject);
        ClearCoroutines();
        Coroutine newRoutine = StartCoroutine(Toolbox.ShakeTree(transform.parent, initialRotation));
        shakeRoutines.Add(newRoutine);

        if (Random.Range(0f, 1f) < leafProbability) {
            leafPool.GetObject(Toolbox.RandomInsideBounds(collider, padding: 1.5f));
        }
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
