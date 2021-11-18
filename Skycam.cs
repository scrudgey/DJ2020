using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skycam : MonoBehaviour {
    public float translationRatio = 0.01f;
    private Transform masterCameraTransform;
    private Vector3 masterCameraInitialPosition;
    private Vector3 myInitialPosition;
    public Camera masterCamera;
    private Camera myCamera;
    void Start() {
        masterCameraTransform = masterCamera.transform;
        masterCameraInitialPosition = masterCameraTransform.position;
        myInitialPosition = transform.position;
        myCamera = GetComponent<Camera>();
    }
    void Update() {
        transform.position = myInitialPosition + (masterCameraTransform.position - masterCameraInitialPosition) * translationRatio;
        transform.rotation = masterCameraTransform.rotation;
    }
}
