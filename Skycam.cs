using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skycam : MonoBehaviour {
    public float translationRatio = 0.01f;
    private Transform masterCameraTransform;
    private Vector3 masterCameraInitialPosition;
    private Vector3 myInitialPosition;
    public Camera masterCamera;
    public Camera myCamera;
    public void Initialize(Camera masterCamera, Vector3 offset) {
        this.masterCamera = masterCamera;
        masterCameraTransform = masterCamera.transform;
        masterCameraInitialPosition = masterCameraTransform.position;
        myInitialPosition = transform.position + offset;
        myCamera = GetComponent<Camera>();
    }
    void Update() {
        if (masterCamera == null) return;
        Vector3 offset = (masterCameraTransform.position - masterCameraInitialPosition);
        offset.y = 2f * offset.y;
        transform.position = myInitialPosition + offset * translationRatio;
        // transform.position = myInitialPosition + (masterCameraTransform.position - masterCameraInitialPosition) * translationRatio;
    }
}
