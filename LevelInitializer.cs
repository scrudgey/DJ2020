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

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        // foreach (T other in edges) {
        //     if (other == null)
        //         continue;
        //     Gizmos.DrawLine(NodePosition(), other.NodePosition());
        // }
        Gizmos.color = Color.red;
        foreach (RandomPayDataInitializer initializer in payDataInitializers) {
            foreach (CyberDataStore dataStore in initializer.dataStores) {
                if (dataStore == null) continue;
                Gizmos.DrawLine(transform.position, dataStore.transform.position);
            }
        }
    }
#endif
}

[System.Serializable]
public class RandomPayDataInitializer {
    // public PayData payData;
    public ObjectiveData objectiveData;
    public List<CyberDataStore> dataStores;
    public void Apply() {
        CyberDataStore dataStore = Toolbox.RandomFromList(dataStores);
        Debug.Log($"apply random paydata state: {objectiveData.targetPaydata.name} -> {dataStore.gameObject}");
        dataStore.node.payData = objectiveData.targetPaydata;
        dataStore.RefreshState();
    }
}