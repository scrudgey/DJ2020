using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject prefab;
    private float timer;
    public float delay;

    // Update is called once per frame
    void Update() {
        timer += Time.deltaTime;
        if (timer > delay) {
            GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
            Destroy(this);
        }
    }
}
