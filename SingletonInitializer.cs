using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonInitializer : MonoBehaviour {
    void Awake() {
        GameManager.InitializeInstance();
        DecalPool.InitializeInstance();
        Destroy(gameObject);
    }
}
