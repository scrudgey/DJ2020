using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRandom : MonoBehaviour {
    public float probability = 1f;
    public GameObject[] prefabs;
    public bool makeLootNotStealing;
    public void Start() {
        if (Random.Range(0f, 1f) < probability) {
            GameObject obj = GameObject.Instantiate(getPrefab(), transform.position, Quaternion.identity);
            if (makeLootNotStealing) {
                LootObject lootObject = obj.GetComponentInChildren<LootObject>();
                if (lootObject != null) {
                    lootObject.isStealing = false;
                }
            }
        }
        Destroy(gameObject);
    }

    public GameObject getPrefab() {
        return Toolbox.RandomFromList(prefabs);
    }
}
