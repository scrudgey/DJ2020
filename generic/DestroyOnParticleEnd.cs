using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnParticleEnd : MonoBehaviour {
    ParticleSystem[] particles;
    public GameObject prefab;
    PrefabPool pool;
    void Awake() {
        if (prefab == null) {
            Debug.LogError($"destroy on end: no prefab {gameObject}");
        }
        pool = PoolManager.I.GetPool(prefab);
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update() {
        bool isPlaying = false;
        foreach (ParticleSystem sys in particles) {
            if (sys.isPlaying) {
                isPlaying = true;
                break;
            }
        }
        if (!isPlaying) {
            // Destroy(gameObject);
            pool.RecallObject(gameObject);
        }
    }
}
