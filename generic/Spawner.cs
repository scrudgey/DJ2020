using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject prefab;
    private float timer;
    public float delay;
    public bool repeating;
    public bool useObjectPool;
    public bool stopOnGround;
    Rigidbody myRigidBody;
    void Start() {
        myRigidBody = GetComponent<Rigidbody>();
    }

    void Update() {
        timer += Time.deltaTime;
        if (stopOnGround && Mathf.Abs(myRigidBody.velocity.y) < 0.1f) {
            return;
        }
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
