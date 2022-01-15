using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonInitializer : MonoBehaviour {
    void Start() {
        GameManager.InitializeInstance();
        PoolManager.InitializeInstance();
        if (!SceneManager.GetSceneByName("UI").isLoaded) {
            SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);
        }
        Destroy(gameObject);
    }
}
