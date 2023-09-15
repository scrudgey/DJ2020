using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LevelInitializer : MonoBehaviour {
    public List<RandomPayDataInitializer> payDataInitializers;
    public void ApplyState() {
        foreach (RandomPayDataInitializer payDataInitializer in payDataInitializers) {
            payDataInitializer.Apply();
        }
    }
}

[System.Serializable]
public class RandomPayDataInitializer {
    public PayData payData;
    public List<CyberDataStore> dataStores;
    public void Apply() {
        CyberDataStore dataStore = Toolbox.RandomFromList(dataStores);
        Debug.Log($"apply random paydata state: {payData.name} -> {dataStore.gameObject}");
        dataStore.payData = payData;
    }
}