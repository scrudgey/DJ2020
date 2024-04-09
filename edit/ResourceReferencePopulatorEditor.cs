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
    SerializedProperty keyList;
    SerializedProperty valueList;
    int ListSize;

    void OnEnable() {
        t = (ResourceReferencePopulator)target;
        GetTarget = new SerializedObject(t);
        // ThisList = GetTarget.FindProperty("entries"); // Find the List in our script and create a refrence of it
        keyList = GetTarget.FindProperty("keys"); // Find the List in our script and create a refrence of it
        valueList = GetTarget.FindProperty("values"); // Find the List in our script and create a refrence of it
    }

    public override void OnInspectorGUI() {
        GetTarget.Update();
        if (GUILayout.Button("Populate resource list")) {
            Debug.Log("populating resource list...");
            List<ResourceEntry> entries = EnumerateAllResources();
            keyList.ClearArray();
            valueList.ClearArray();
            int i = 0;
            foreach (ResourceEntry entry in entries) {
                keyList.InsertArrayElementAtIndex(i);
                valueList.InsertArrayElementAtIndex(i);

                keyList.GetArrayElementAtIndex(i).objectReferenceValue = entry.key;
                valueList.GetArrayElementAtIndex(i).stringValue = entry.value;
                i++;
            }
            // populate list
        }

        //Resize our list
        ListSize = keyList.arraySize;
        ListSize = EditorGUILayout.IntField("size", ListSize);

        if (ListSize != keyList.arraySize) {
            while (ListSize > keyList.arraySize) {
                keyList.InsertArrayElementAtIndex(keyList.arraySize);
                valueList.InsertArrayElementAtIndex(keyList.arraySize);
            }
            while (ListSize < keyList.arraySize) {
                keyList.DeleteArrayElementAtIndex(keyList.arraySize - 1);
                valueList.DeleteArrayElementAtIndex(keyList.arraySize - 1);
            }
        }
        // EditorGUILayout.PropertyField(GetTarget.FindProperty("target"));
        // EditorGUILayout.Space();
        // if (GUILayout.Button("Add New")) {
        //     t.entries.Add(new ResourceEntry());
        // }

        // EditorGUILayout.Space();

        // for (int i = 0; i < keyList.arraySize; i++) {
        //     SerializedProperty keyRef = keyList.GetArrayElementAtIndex(i);
        //     SerializedProperty valueRef = valueList.GetArrayElementAtIndex(i);
        //     // SerializedProperty Key = MyListRef.FindPropertyRelative("key");
        //     // SerializedProperty Value = MyListRef.FindPropertyRelative("value");
        //     EditorGUILayout.PropertyField(keyRef);
        //     EditorGUILayout.PropertyField(valueRef);
        //     // if (GUILayout.Button($"remove {i}")) {
        //     //     ThisList.DeleteArrayElementAtIndex(i);
        //     // }
        //     EditorGUILayout.Space();
        // }

        GetTarget.ApplyModifiedProperties();
    }

    public List<ResourceEntry> EnumerateAllResources() {
        List<ResourceEntry> entries = new List<ResourceEntry>();
        // populator.entries = new List<ResourceEntry>();
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
                if (path.Contains("/Obi/"))
                    continue;
                int startIndex = path.IndexOf(RESOURCES);
                Debug.Log($"{startIndex} {path}");
                string resourcePath = path.Substring(startIndex + RESOURCES.Length + 1, path.Length - startIndex - RESOURCES.Length - 1);
                string finalpath = resourcePath.Substring(0, resourcePath.Length - file.Extension.Length);
                // Debug.Log($"resource: {finalpath}");
                UnityEngine.Object obj = Resources.Load(finalpath);
                if (obj != null) {
                    ResourceEntry entry = new ResourceEntry() {
                        key = obj,
                        value = finalpath
                    };
                    // populator.entries.Add(entry);
                    // ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
                    // ThisList.GetArrayElementAtIndex(ThisList.arraySize).objectReferenceValue = entry;
                    entries.Add(entry);
                } else {
                    Debug.LogError($"Failed to load resource: {file.FullName}");
                }
            }
        }
        return entries;
    }
}
#endif