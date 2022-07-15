using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour {
    void OnCollisionEnter(Collision collision) {
        PoolManager.I.RecallObject(gameObject);
    }
}
