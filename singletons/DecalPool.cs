using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: abstract the pool behavior
// TODO: allow decalPrefab to be set in editor
// TODO: allow multiple pools
// TODO: actions to take on enable / disable to reset object state
public class DecalPool : Singleton<DecalPool> {
    [SerializeField]
    private GameObject decalPrefab;

    [SerializeField]
    private int maxConcurrentDecals = 100;

    private Queue<GameObject> decalsInPool;
    private Queue<GameObject> decalsActiveInWorld;

    private void Awake() {
        // decalPrefab = Resources.Load("prefabs/bullethole") as GameObject;
        InitializeDecals();
    }

    private void InitializeDecals() {
        decalsInPool = new Queue<GameObject>();
        decalsActiveInWorld = new Queue<GameObject>();

        for (int i = 0; i < maxConcurrentDecals; i++) {
            InstantiateDecal();
        }
    }

    private void InstantiateDecal() {
        var spawned = GameObject.Instantiate(decalPrefab);
        spawned.transform.SetParent(this.transform);

        decalsInPool.Enqueue(spawned);
        spawned.SetActive(false);
    }

    public GameObject SpawnDecal(Vector3 position) {
        GameObject decal = GetNextAvailableDecal();
        if (decal != null) {
            decal.transform.position = position;
            // decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);

            decal.SetActive(true);

            decalsActiveInWorld.Enqueue(decal);
        }
        return decal;
    }

    private GameObject GetNextAvailableDecal() {
        if (decalsInPool.Count > 0)
            return decalsInPool.Dequeue();

        var oldestActiveDecal = decalsActiveInWorld.Dequeue();
        return oldestActiveDecal;
    }

#if UNITY_EDITOR

    private void Update() {
        if (transform.childCount < maxConcurrentDecals)
            InstantiateDecal();
        else if (ShoudlRemoveDecal())
            DestroyExtraDecal();
    }

    private bool ShoudlRemoveDecal() {
        return transform.childCount > maxConcurrentDecals;
    }

    private void DestroyExtraDecal() {
        if (decalsInPool.Count > 0)
            Destroy(decalsInPool.Dequeue());
        else if (ShoudlRemoveDecal() && decalsActiveInWorld.Count > 0)
            Destroy(decalsActiveInWorld.Dequeue());
    }

#endif
}