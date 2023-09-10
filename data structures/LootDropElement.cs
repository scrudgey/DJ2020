using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/LootDropElement")]
public class LootDropElement : ScriptableObject {
    public LoHi number;
    public List<WeightedLoot> loots;

    [Header("specifics")]
    public int[] keyIds;
    public LoHi creditAmount;
    public GameObject instantiateLoot(Vector3 position) {
        WeightedLoot loot = Toolbox.RandomFromListByWeight(loots, (WeightedLoot loot) => loot.weight);

        GameObject prefab = loot.prefab;

        SpawnRandom randomSpawner = loot.prefab.GetComponent<SpawnRandom>();
        if (randomSpawner != null) {
            prefab = randomSpawner.getPrefab();
        }

        GameObject obj = GameObject.Instantiate(prefab, position, Quaternion.identity);

        // apply keyId if prefab is a key
        Key key = obj.GetComponentInChildren<Key>();
        if (key != null) {
            key.keyId = Toolbox.RandomFromList(keyIds);
        }

        // apply credit range if prefab is credstick
        Credstick credstick = obj.GetComponentInChildren<Credstick>();
        if (credstick != null) {
            credstick.amount = (int)creditAmount.GetRandomInsideBound();
        }

        return obj;
    }
}


[System.Serializable]
public class WeightedLoot {
    public float weight;
    public GameObject prefab;
}