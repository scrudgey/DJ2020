using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabPool {
    protected GameObject prefab;
    private int maxConcurrentObjects = 100;
    public Queue<GameObject> objectsInPool;
    public Queue<GameObject> objectsActiveInWorld;
    public PrefabPool(string prefabPath, int poolSize = 100) {
        this.prefab = Resources.Load(prefabPath) as GameObject;
        this.maxConcurrentObjects = poolSize;
    }
    public PrefabPool(GameObject prefab, int poolSize = 100) {
        this.prefab = prefab;
        this.maxConcurrentObjects = poolSize;

    }
    protected GameObject InstantiatePrefab() {
        GameObject obj = GameObject.Instantiate(prefab);
        return obj;
    }
    protected virtual void EnableObject(GameObject obj) {
        obj.SetActive(true);
        obj.transform.SetParent(null, true);
        foreach (IPoolable poolable in obj.GetComponentsInChildren<IPoolable>()) {
            poolable.OnPoolActivate();
        }
    }
    protected virtual void DisableObject(GameObject obj) {
        obj.SetActive(false);
        foreach (IPoolable poolable in obj.GetComponentsInChildren<IPoolable>()) {
            // Debug.Log($"deactivating {poolable}");
            poolable.OnPoolDectivate();
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
        DisableObject(obj);
        objectsInPool.Enqueue(obj);
        objectsActiveInWorld = new Queue<GameObject>(objectsActiveInWorld.Where(x => x != obj));
        obj.transform.SetParent(null);
        // if (obj.name.ToLower().Contains("npc"))
        //     Debug.Log($"RECALL active: {objectsActiveInWorld.Count} pooled: {objectsInPool.Count}");
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
        } else {
            obj = InstantiatePrefab();
            obj.transform.position = position;
        }
        EnableObject(obj);
        objectsActiveInWorld.Enqueue(obj);
        // if (obj.name.ToLower().Contains("npc"))
        //     Debug.Log($"GET active: {objectsActiveInWorld.Count} pooled: {objectsInPool.Count}");
        return obj;
    }
    public GameObject GetObject() {
        GameObject obj = GetNextAvailableObject();
        if (obj == null) {
            obj = InstantiatePrefab();
        }
        EnableObject(obj);
        objectsActiveInWorld.Enqueue(obj);
        // if (obj.name.ToLower().Contains("npc"))
        //     Debug.Log($"GET active: {objectsActiveInWorld.Count} pooled: {objectsInPool.Count}");
        return obj;
    }
}


public class PoolManager : Singleton<PoolManager> {
    private Dictionary<string, PrefabPool> prefabPools = new Dictionary<string, PrefabPool>();

    public enum DecalType { normal, glass, blood }
    public static readonly Dictionary<DecalType, string> decalPaths = new Dictionary<DecalType, string>{
        {DecalType.normal, "sprites/particles/bulletholes_normal"},
        {DecalType.glass, "sprites/particles/bulletholes_glass"},
        {DecalType.blood, "sprites/particles/blood_decal"}
    };
    private static readonly Dictionary<DecalType, Sprite[]> decalSprites = new Dictionary<DecalType, Sprite[]>();
    void Awake() {
        foreach (KeyValuePair<DecalType, string> kvp in decalPaths) {
            decalSprites[kvp.Key] = Resources.LoadAll<Sprite>(kvp.Value) as Sprite[];
        }
        RegisterPool("prefabs/fx/bullethole");
        RegisterPool("prefabs/fx/blood_decal", poolSize: 5);
    }
    public PrefabPool RegisterPool(string prefabPath, int poolSize = 100) {
        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        return RegisterPool(prefab, poolSize: poolSize);
    }
    public PrefabPool RegisterPool(GameObject prefab, int poolSize = 100) {
        string prefabname = Toolbox.NameWithoutClone(prefab);
        if (prefabPools.ContainsKey(prefabname)) {
            return prefabPools[prefabname];
        }
        Debug.Log($"creating new prefab pool: {prefab} {poolSize}");
        PrefabPool pool = new PrefabPool(prefab, poolSize: poolSize);
        prefabPools[prefabname] = pool;
        pool.InitializePool();
        return pool;
    }
    public void RecallObject(GameObject obj) {
        if (obj == null)
            return;
        GetPool(obj).RecallObject(obj);
    }
    public void RecallObjects(GameObject[] objects) {
        PrefabPool pool = null;
        foreach (GameObject obj in objects) {
            if (obj == null) continue;
            pool = GetPool(obj);
            pool.RecallObject(obj);
        }
    }
    public PrefabPool GetPool(string prefabPath) {
        GameObject prefab = Resources.Load(prefabPath) as GameObject;
        return GetPool(prefab);
    }

    // TODO: refactor, use Instantiate syntax.
    public PrefabPool GetPool(GameObject prefab) {
        string prefabname = Toolbox.NameWithoutClone(prefab);
        if (prefabPools.ContainsKey(prefabname)) {
            return prefabPools[prefabname];
        } else {
            return RegisterPool(prefab);
        }
    }
    public GameObject CreateDecal(RaycastHit hit, DecalType type) {
        PrefabPool pool = GetPool("prefabs/fx/bullethole"); // TODO: fix?
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