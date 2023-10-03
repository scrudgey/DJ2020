using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


[CustomEditor(typeof(PerkButton))]
public class PerkButtonEditor : Editor {
    PerkButton t;
    SerializedObject GetTarget;
    SerializedProperty levelData;

    Image icon;
    TextMeshProUGUI caption;

    void OnEnable() {
        t = (PerkButton)target;
        GetTarget = new SerializedObject(t);
        // levelData = GetTarget.FindProperty("levelData");
    }
    public override void OnInspectorGUI() {

        icon = (Image)base.serializedObject.FindProperty("icon").objectReferenceValue;
        caption = (TextMeshProUGUI)base.serializedObject.FindProperty("caption").objectReferenceValue;

        // EditorGUILayout.PropertyField(levelData);
        DrawDefaultInspector();
        if (GUILayout.Button("populate")) {
            // LoadSpriteData(sheetTypeString);
            Debug.Log("populate");
            PopulateGraphics();
        }
    }

    void PopulateGraphics() {
        icon.sprite = t.perk.icon;
        caption.text = t.perk.readableName;
    }

}