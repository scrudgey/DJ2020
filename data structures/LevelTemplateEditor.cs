// #if UNITY_EDITOR

// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// [CustomEditor(typeof(LevelTemplate))]
// public class LevelTemplateEditor : Editor {

//     LevelTemplate t;

//     SerializedObject GetTarget;
//     SerializedProperty objectivesList;
//     int ListSize;


//     void OnEnable() {
//         t = (LevelTemplate)target;
//         GetTarget = new SerializedObject(t);
//         objectivesList = GetTarget.FindProperty("objectives");
//     }

//     public override void OnInspectorGUI() {
//         GetTarget.Update();

//         //Resize our list
//         // ListSize = objectivesList.arraySize;
//         // ListSize = EditorGUILayout.IntField("size", ListSize);
//         // if (ListSize != objectivesList.arraySize) {
//         //     while (ListSize > objectivesList.arraySize) {
//         //         objectivesList.InsertArrayElementAtIndex(objectivesList.arraySize);
//         //     }
//         //     while (ListSize < objectivesList.arraySize) {
//         //         objectivesList.DeleteArrayElementAtIndex(objectivesList.arraySize - 1);
//         //     }
//         // }
//         EditorGUILayout.PropertyField(GetTarget.FindProperty("levelName"));
//         EditorGUILayout.PropertyField(GetTarget.FindProperty("sensitivityLevel"));
//         EditorGUILayout.PropertyField(GetTarget.FindProperty("alarmAudioClip"));
//         EditorGUILayout.PropertyField(GetTarget.FindProperty("strikeTeamResponseTime"));
//         EditorGUILayout.PropertyField(GetTarget.FindProperty("strikeTeamTemplate"));

//         for (int i = 0; i < objectivesList.arraySize; i++) {
//             SerializedProperty keyRef = objectivesList.GetArrayElementAtIndex(i);

//             EditorGUILayout.PropertyField(keyRef);

//             Objective objective = (Objective)keyRef.objectReferenceValue;
//             EditorGUILayout.TextField("title", objective.title);
//             EditorGUILayout.TextArea("description", objective.description);

//             // EditorGUILayout.PropertyField(keyRef.FindProperty("title"));
//             // objective.title = EditorGUILayout.InputField("Str", asSub.str);

//             // baseOrSub.integer = EditorGUILayout.IntField("Integer", baseOrSub.integer);
//             // var asSub = baseOrSub as sub;
//             // if (asSub != null) {
//             //     asSub.str = EditorGUILayout.InputField("Str", asSub.str);
//             // }

//             // EditorGUILayout.PropertyField(keyRef);
//             switch (objective) {
//                 case ObjectiveData od:
//                     EditorGUILayout.PropertyField(keyRef.FindPropertyRelative("targetFileNames"));
//                     break;
//             }

//             if (GUILayout.Button($"remove {i}")) {
//                 objectivesList.DeleteArrayElementAtIndex(i);
//             }
//             EditorGUILayout.Space();
//         }
//         // add objectives buttons
//         if (GUILayout.Button("New Data Objective")) {
//             t.objectives.Add(new ObjectiveData());
//         }

//         GetTarget.ApplyModifiedProperties();
//     }
// }
// #endif
