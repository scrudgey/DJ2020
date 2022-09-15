using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonInitializer : MonoBehaviour {
    void Awake() {
        GameManager.InitializeInstance();
        PoolManager.InitializeInstance();
        Destroy(gameObject);
    }
}
