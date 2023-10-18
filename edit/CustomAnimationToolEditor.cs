#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomAnimationTool))]
public class CustomAnimationToolEditor : Editor {

    public override void OnInspectorGUI() {
        if (GUILayout.Button("compute animations")) {
            Debug.Log("compute animations");
            AnimationClip[] animationClips = Resources.LoadAll<AnimationClip>("animations/");
            foreach (AnimationClip animationClip in animationClips) {
                AnimationEvent[] events = AnimationUtility.GetAnimationEvents(animationClip);
                Debug.Log($"writing {animationClip}...");
                CustomAnimationClip newClip = new CustomAnimationClip(events, animationClip);
                newClip.Write(animationClip.name);
            }
            AssetDatabase.Refresh();
        }
    }
}

#endif
