using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class CyberRandomizer : MonoBehaviour {
    public CyberRandomizerTemplate template;

    [Header("datastore")]
    public CyberDataStore dataStore;

    public void ApplyState(LevelTemplate levelTemplate) {
        if (dataStore != null) {
            dataStore.payData = Toolbox.RandomFromList(template.payDatas);
            dataStore.RefreshState();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        GUIStyle myStyle = new GUIStyle();
        myStyle.normal.textColor = template.color;
        Handles.Label(transform.position, $"{template.cyberName}", myStyle);
    }
#endif
}
