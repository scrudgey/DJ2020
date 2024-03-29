﻿using System.Linq;
using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    // protected string prefabPath = "required/singletonTemplate";
    private static T _instance;
    private static object _lock = new object();
    public static T I {
        get {
            if (applicationIsQuitting) {
                // Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                //                  "' already destroyed on application quit." +
                //                  " Won't create again - returning null.");
                return null;
            }
            lock (_lock) {
                if (_instance == null) {
                    InitializeInstance();
                }
                return _instance;
            }
        }
    }
    public static void InitializeInstance() {
        // Debug.Log($"********** initialize singleton instance: {typeof(T)}");
        if (_instance != null)
            return;
        _instance = (T)FindObjectOfType(typeof(T));
        if (FindObjectsOfType(typeof(T)).Length > 1) {
            Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopenning the scene might fix it.");
            return;
        }
        if (_instance == null) {
            GameObject[] singletonPrefabs = Resources.LoadAll("singletons/", typeof(GameObject))
                .Cast<GameObject>()
                .ToArray();

            string prefabPath = "singletons/singletonTemplate";

            foreach (GameObject prefab in singletonPrefabs) {
                if (prefab.GetComponent<T>() != null) {
                    prefabPath = "singletons/" + prefab.name;
                    // Debug.Log($"using singleton prefab {prefab.name}");
                }
            }

            GameObject singleton = GameObject.Instantiate(Resources.Load(prefabPath)) as GameObject;

            _instance = singleton.GetComponent<T>();
            if (_instance == null)
                _instance = singleton.AddComponent<T>();
            singleton.name = "(singleton) " + typeof(T).ToString();
            DontDestroyOnLoad(singleton);
            // Debug.Log("[Singleton] An instance of " + typeof(T) +
            //          " is needed in the scene, so '" + singleton +
            //          "' was created with DontDestroyOnLoad.");
            _instance.transform.SetParent(null);
        } else {
            // Debug.Log("[Singleton] Using instance already created: " +
            // _instance.gameObject.name);
        }
    }
    protected static bool applicationIsQuitting = false;
    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public virtual void OnDestroy() {
        Debug.Log("[Singleton] destroying singleton " + typeof(T));
        applicationIsQuitting = true;
    }
}