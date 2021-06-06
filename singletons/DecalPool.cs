// using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: abstract the pool behavior
// TODO: allow decalPrefab to be set in editor
// TODO: allow multiple pools
// TODO: actions to take on enable / disable to reset object state
public class DecalPool : Singleton<DecalPool> {
    public enum DecalType { normal, glass }
    [SerializeField]
    private GameObject decalPrefab;

    [SerializeField]
    private int maxConcurrentDecals = 100;
    private Queue<GameObject> decalsInPool;
    private Queue<GameObject> decalsActiveInWorld;
    public static readonly Dictionary<DecalType, string> decalPaths = new Dictionary<DecalType, string>{
        {DecalType.normal, "sprites/bulletholes"},
        {DecalType.glass, "sprites/Bullet decals"}
    };
    private static readonly Dictionary<DecalType, Sprite[]> decalSprites = new Dictionary<DecalType, Sprite[]>();
    private void Awake() {
        foreach (KeyValuePair<DecalType, string> kvp in decalPaths) {
            decalSprites[kvp.Key] = Resources.LoadAll<Sprite>(kvp.Value) as Sprite[];
        }
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

    public GameObject SpawnDecal(RaycastHit hit, DecalType type) {
        GameObject decal = GetNextAvailableDecal();
        if (decal != null) {
            decal.transform.position = hit.point + (hit.normal * 0.025f);
            decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
            decal.SetActive(true);
            decalsActiveInWorld.Enqueue(decal);
            RandomizeSprite decalRandomizer = decal.GetComponent<RandomizeSprite>();
            decalRandomizer.sprites = decalSprites[type];
            decalRandomizer.Randomize();
        }
        return decal;
    }
    public void RecallDecal(GameObject decal) {
        if (decal == null) {
            Debug.LogWarning("RecallDecal called with null value");
        }
        decal.SetActive(false);
        decalsInPool.Enqueue(decal);
    }
    public void RecallDecals(GameObject[] decals) {
        foreach (GameObject decal in decals) {
            RecallDecal(decal);
        }
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