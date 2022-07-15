using System.Collections.Generic;
using UnityEngine;


public class PrefabPool {
    protected GameObject prefab;
    private int maxConcurrentObjects = 100;
    private Queue<GameObject> objectsInPool;
    private Queue<GameObject> objectsActiveInWorld;
    public PrefabPool(string prefabPath) {
        this.prefab = Resources.Load(prefabPath) as GameObject;
    }
    public PrefabPool(GameObject prefab) {
        this.prefab = prefab;
    }
    protected GameObject InstantiatePrefab() {
        GameObject obj = GameObject.Instantiate(prefab);
        return obj;
    }
    protected virtual void EnableObject(GameObject obj) {
        obj.SetActive(true);
        obj.transform.SetParent(null, true);
        // allow decals to define enable methods
        PoolObject poolObject = obj.GetComponentInChildren<PoolObject>();
        if (poolObject != null) {
            poolObject.OnPoolActivate();
        }
    }
    protected virtual void DisableObject(GameObject obj) {
        obj.SetActive(false);
        // allow decals to define disable methods
        PoolObject poolObject = obj.GetComponentInChildren<PoolObject>();
        if (poolObject != null) {
            poolObject.OnPoolDectivate();
        }
    }
    public virtual void InitializePool() {
        objectsInPool = new Queue<GameObject>();
        objectsActiveInWorld = new Queue<GameObject>();
        for (int i = 0; i < maxConcurrentObjects; i++) {
            NewObject();
        }
    }
    protected void NewObject() {
        GameObject spawned = InstantiatePrefab();
        objectsInPool.Enqueue(spawned);
        DisableObject(spawned);
    }
    public void RecallObject(GameObject obj) {
        if (obj == null) {
            Debug.LogWarning("RecallObject called with null value");
        }
        // decal.SetActive(false);
        DisableObject(obj);
        objectsInPool.Enqueue(obj);
        obj.transform.SetParent(null);
    }
    public void RecallObjects(GameObject[] objects) {
        foreach (GameObject obj in objects) {
            RecallObject(obj);
        }
    }
    protected GameObject GetNextAvailableObject() {
        if (objectsInPool.Count > 0)
            return objectsInPool.Dequeue();
        var oldestActiveDecal = objectsActiveInWorld.Dequeue();
        return oldestActiveDecal;
    }
    public GameObject GetObject(Vector3 position) {
        GameObject obj = GetNextAvailableObject();
        if (obj != null) {
            obj.transform.position = position;
            // obj.GetComponent<Rigidbody>().Move
        } else {
            obj = InstantiatePrefab();
        }
        EnableObject(obj);
        objectsActiveInWorld.Enqueue(obj);
        return obj;
    }
    public GameObject GetObject() {
        GameObject obj = GetNextAvailableObject();
        if (obj == null) {
            obj = InstantiatePrefab();
        }
        EnableObject(obj);
        objectsActiveInWorld.Enqueue(obj);
        return obj;
    }
}


public class PoolManager : Singleton<PoolManager> {
    private Dictionary<GameObject, PrefabPool> prefabPools = new Dictionary<GameObject, PrefabPool>();

    public enum DecalType { normal, glass }
    public static readonly Dictionary<DecalType, string> decalPaths = new Dictionary<DecalType, string>{
        {DecalType.normal, "sprites/particles/bulletholes_normal"},
        {DecalType.glass, "sprites/particles/bulletholes_glass"}
    };
    private static readonly Dictionary<DecalType, Sprite[]> decalSprites = new Dictionary<DecalType, Sprite[]>();
    void Awake() {
        foreach (KeyValuePair<DecalType, string> kvp in decalPaths) {
            decalSprites[kvp.Key] = Resources.LoadAll<Sprite>(kvp.Value) as Sprite[];
        }
        RegisterPool("prefabs/fx/bullethole");
    }
    public PrefabPool RegisterPool(string prefabPath) {
        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        return RegisterPool(prefab);
    }
    public PrefabPool RegisterPool(GameObject prefab) {
        if (prefabPools.ContainsKey(prefab)) {
            return prefabPools[prefab];
        }
        // Debug.Log($"initializing prefabpool for {prefab}");
        PrefabPool pool = new PrefabPool(prefab);
        prefabPools[prefab] = pool;
        pool.InitializePool();
        return pool;
    }
    public void RecallObject(GameObject obj) {
        if (obj == null)
            return;
        // get key from component
        PoolObject poolObject = obj.GetComponent<PoolObject>();
        if (poolObject != null) {
            GetPool(poolObject.prefabKey).RecallObject(obj);
        } else {
            Debug.LogWarning($"PoolObject not found on RecallObject {obj}");
        }
    }
    public void RecallObjects(GameObject[] objects) {
        PrefabPool pool = null;
        foreach (GameObject obj in objects) {
            if (obj == null) continue;
            if (pool == null) {
                PoolObject poolObject = obj.GetComponentInChildren<PoolObject>();
                if (poolObject != null) {
                    pool = GetPool(poolObject.prefabKey);
                }
            } else {
                pool.RecallObject(obj);
            }
        }
    }
    public PrefabPool GetPool(string prefabPath) {
        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        return GetPool(prefab);
    }

    // TODO: refactor, use Instantiate syntax.
    public PrefabPool GetPool(GameObject prefab) {
        if (prefabPools.ContainsKey(prefab)) {
            return prefabPools[prefab];
        } else {
            return RegisterPool(prefab);
        }
    }
    public GameObject CreateDecal(RaycastHit hit, DecalType type) {
        PrefabPool pool = GetPool("prefabs/fx/bullethole");
        GameObject decal = pool.GetObject(hit.point + (hit.normal * 0.025f));
        if (decal != null) {
            RandomizeSprite decalRandomizer = decal.GetComponent<RandomizeSprite>();
            decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
            decalRandomizer.sprites = decalSprites[type];
            decalRandomizer.Randomize();
        }
        return decal;
    }

}