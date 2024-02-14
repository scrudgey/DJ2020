using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject prefab;
    PrefabPool prefabPool;
    public float probability = 1;
    private float timer;
    public float delay;
    public bool repeating;
    public bool useObjectPool;
    public bool stopOnGround;
    Rigidbody myRigidBody;
    void Start() {
        if (useObjectPool)
            prefabPool = PoolManager.I.GetPool(prefab);
        myRigidBody = GetComponent<Rigidbody>();
    }

    void Update() {
        timer += Time.deltaTime;
        if (stopOnGround && Mathf.Abs(myRigidBody.velocity.y) < 0.1f) {
            return;
        }
        if (timer > delay) {
            if (Random.Range(0f, 1f) < probability) {
                if (useObjectPool) {
                    prefabPool.GetObject(transform.position);
                } else {
                    GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
                }
            }

            if (repeating) {
                timer -= delay;
            } else {
                Destroy(this);
            }
        }
    }
}
