using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDropper : MonoBehaviour {
    public Destructible destructible;
    public List<LootDropElementWithProbability> loot;
    void Start() {
        destructible.OnDestruct += HandleDestruct;
    }
    void OnDestroy() {
        destructible.OnDestruct -= HandleDestruct;
    }

    public void HandleDestruct() {
        foreach (LootDropElementWithProbability data in loot) {
            if (Random.Range(0f, 1f) < data.probability) {
                Vector3 position = transform.position + Vector3.up;
                GameObject obj = data.loot.instantiateLoot(position);
                Rigidbody body = obj.GetComponentInChildren<Rigidbody>();
                if (body != null) {
                    Vector3 velocity = Random.Range(2f, 5f) * Random.insideUnitCircle;
                    velocity.y = Mathf.Abs(velocity.y);
                    body.velocity = velocity;
                }
            }
        }
    }
}


[System.Serializable]
public class LootDropElementWithProbability {
    public float probability;
    public LootDropElement loot;
}