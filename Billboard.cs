using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour {
    Transform myTransform;
    public Transform myCamera;
    void Start() {
        myTransform = transform;
    }
    void Update() {
        // myTransform.forward = myCamera.forward;

        var target = myCamera.transform.position;
        target.y = transform.position.y;
        transform.LookAt(target);

#if UNITY_EDITOR
        if (Application.isEditor && !Application.isPlaying) {
            //do what you want
            transform.forward = SceneView.lastActiveSceneView.camera.transform.forward;

        }
#endif
    }
}
