#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ResourceReferencePopulator))]
public class ResourceReferencePopulatorEditor : Editor {
    public static readonly string RESOURCES = "Resources";

    ResourceReferencePopulator t;
    SerializedObject GetTarget;
    SerializedProperty ThisList;
    int ListSize;

    void OnEnable() {
        t = (ResourceReferencePopulator)target;
        GetTarget = new SerializedObject(t);
        ThisList = GetTarget.FindProperty("entries"); // Find the List in our script and create a refrence of it
    }

    public override void OnInspectorGUI() {
        GetTarget.Update();
        if (GUILayout.Button("Populate resource list")) {
            Debug.Log("populating resource list...");
            EnumerateAllResources(t);
        }

        //Resize our list
        ListSize = ThisList.arraySize;
        ListSize = EditorGUILayout.IntField("size", ListSize);

        if (ListSize != ThisList.arraySize) {
            while (ListSize > ThisList.arraySize) {
                ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
            }
            while (ListSize < ThisList.arraySize) {
                ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
            }
        }
        EditorGUILayout.PropertyField(GetTarget.FindProperty("target"));
        EditorGUILayout.Space();
        if (GUILayout.Button("Add New")) {
            t.entries.Add(new ResourceEntry());
        }

        EditorGUILayout.Space();

        for (int i = 0; i < ThisList.arraySize; i++) {
            SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
            SerializedProperty Key = MyListRef.FindPropertyRelative("key");
            SerializedProperty Value = MyListRef.FindPropertyRelative("value");
            EditorGUILayout.PropertyField(Key);
            EditorGUILayout.PropertyField(Value);
            if (GUILayout.Button($"remove {i}")) {
                ThisList.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.Space();
        }

        GetTarget.ApplyModifiedProperties();
    }

    public void EnumerateAllResources(ResourceReferencePopulator populator) {
        populator.entries = new List<ResourceEntry>();
        DirectoryInfo levelDirectoryPath = new DirectoryInfo(Application.dataPath);
        FileInfo[] fileInfo = levelDirectoryPath.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (FileInfo file in fileInfo) {
            if (file.FullName.Contains(RESOURCES) && file.Extension != ".meta") {
                string path = file.FullName;
                if (path.Contains(".DS_Store"))
                    continue;
                if (path.Contains(".git"))
                    continue;
                if (path.Contains("/Scripts/"))
                    continue;
                int startIndex = path.IndexOf(RESOURCES);
                string resourcePath = path.Substring(startIndex + RESOURCES.Length + 1, path.Length - startIndex - RESOURCES.Length - 1);
                string finalpath = resourcePath.Substring(0, resourcePath.Length - file.Extension.Length);
                // Debug.Log($"resource: {finalpath}");
                UnityEngine.Object obj = Resources.Load(finalpath);
                if (obj != null) {
                    ResourceEntry entry = new ResourceEntry() {
                        key = obj,
                        value = finalpath
                    };
                    populator.entries.Add(entry);
                } else {
                    Debug.LogError("Failed to load resource");
                }
            }
        }
    }
}
#endif