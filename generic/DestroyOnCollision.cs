using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour {
    public GameObject floorDecalPrefab;
    public float floorDecalProbability;
    void OnCollisionEnter(Collision collision) {
        PoolManager.I.RecallObject(gameObject);
        if (floorDecalPrefab != null && Random.Range(0f, 1f) < floorDecalProbability) {
            GameObject pool = PoolManager.I.GetPool(floorDecalPrefab).GetObject(collision.contacts[0].point);
        }
    }
}
