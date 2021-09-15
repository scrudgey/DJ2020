using System.Collections.Generic;
using UnityEngine;


public abstract class Pool {
    private int maxConcurrentDecals = 100;
    private Queue<GameObject> decalsInPool;
    private Queue<GameObject> decalsActiveInWorld;
    public virtual void InitializeAllDecals() {
        decalsInPool = new Queue<GameObject>();
        decalsActiveInWorld = new Queue<GameObject>();
        for (int i = 0; i < maxConcurrentDecals; i++) {
            NewDecal();
        }
    }
    protected abstract GameObject InstantiateDecal();
    protected virtual void EnableDecal(GameObject decal) {
        decal.SetActive(true);
    }
    protected virtual void DisableDecal(GameObject decal) {
        decal.SetActive(false);
    }
    protected void NewDecal() {
        GameObject spawned = InstantiateDecal();
        decalsInPool.Enqueue(spawned);
        DisableDecal(spawned);
    }
    public void RecallDecal(GameObject decal) {
        if (decal == null) {
            Debug.LogWarning("RecallDecal called with null value");
        }
        // decal.SetActive(false);
        DisableDecal(decal);
        decalsInPool.Enqueue(decal);
    }
    public void RecallDecals(GameObject[] decals) {
        foreach (GameObject decal in decals) {
            RecallDecal(decal);
        }
    }
    protected GameObject GetNextAvailableDecal() {
        if (decalsInPool.Count > 0)
            return decalsInPool.Dequeue();
        var oldestActiveDecal = decalsActiveInWorld.Dequeue();
        return oldestActiveDecal;
    }

    public GameObject SpawnDecal(Vector3 position) {
        GameObject decal = GetNextAvailableDecal();
        if (decal != null) {
            decal.transform.position = position;
            EnableDecal(decal);
            decalsActiveInWorld.Enqueue(decal);
        }
        return decal;
    }
}
public class PrefabPool : Pool {
    GameObject prefab;
    public PrefabPool(string prefabPath) {
        prefab = Resources.Load(prefabPath) as GameObject;
    }
    protected override GameObject InstantiateDecal() {
        return GameObject.Instantiate(prefab);
    }

}
public class DecalPool : Pool {
    public enum DecalType { normal, glass }
    GameObject prefab;
    public DecalPool() {
        this.prefab = Resources.Load("prefabs/bullethole") as GameObject;
    }
    public static readonly Dictionary<DecalType, string> decalPaths = new Dictionary<DecalType, string>{
        {DecalType.normal, "sprites/particles/bulletholes_normal"},
        {DecalType.glass, "sprites/particles/bulletholes_glass"}
    };
    private static readonly Dictionary<DecalType, Sprite[]> decalSprites = new Dictionary<DecalType, Sprite[]>();

    public override void InitializeAllDecals() {
        foreach (KeyValuePair<DecalType, string> kvp in decalPaths) {
            decalSprites[kvp.Key] = Resources.LoadAll<Sprite>(kvp.Value) as Sprite[];
        }
        base.InitializeAllDecals();
    }
    protected override GameObject InstantiateDecal() {
        return GameObject.Instantiate(prefab);
    }
    public GameObject CreateDecal(RaycastHit hit, DecalType type) {
        GameObject decal = base.SpawnDecal(hit.point + (hit.normal * 0.025f));
        if (decal != null) {
            RandomizeSprite decalRandomizer = decal.GetComponent<RandomizeSprite>();
            decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
            decalRandomizer.sprites = decalSprites[type];
            decalRandomizer.Randomize();
        }
        return decal;
    }
}

public class PoolManager : Singleton<PoolManager> {
    public DecalPool decalPool;
    public PrefabPool leafPool;
    public PrefabPool damageDecalPool;
    void Awake() {
        decalPool = new DecalPool();
        leafPool = new PrefabPool("prefabs/fx/leaf");
        damageDecalPool = new PrefabPool("prefabs/damageDecal");

        decalPool.InitializeAllDecals();
        leafPool.InitializeAllDecals();
        damageDecalPool.InitializeAllDecals();
    }

}