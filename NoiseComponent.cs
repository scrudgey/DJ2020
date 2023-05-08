using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseComponent : MonoBehaviour {
    public NoiseData data;
    public SphereCollider sphereCollider;
    public MeshCollider meshCollider;
    float timer;
    void Update() {
        timer += Time.deltaTime;
        if (timer > 0.1f) {
            PoolManager.I.RecallObject(gameObject);
        }
    }
}