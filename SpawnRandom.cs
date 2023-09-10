using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRandom : MonoBehaviour {
    public float probability = 1f;
    public GameObject[] prefabs;

    public void Start() {
        if (Random.Range(0f, 1f) < probability) {
            GameObject.Instantiate(getPrefab(), transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public GameObject getPrefab() {
        return Toolbox.RandomFromList(prefabs);
    }
}
