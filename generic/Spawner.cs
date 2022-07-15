using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject prefab;
    private float timer;
    public float delay;
    public bool repeating;
    public bool useObjectPool;

    // Update is called once per frame
    void Update() {
        timer += Time.deltaTime;
        if (timer > delay) {
            if (useObjectPool) {
                PoolManager.I.GetPool(prefab).GetObject(transform.position);
            } else {
                GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
            }
            if (repeating) {
                timer -= delay;
            } else {
                Destroy(this);
            }
        }
    }
}
