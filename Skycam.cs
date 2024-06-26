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
    public void Initialize(Vector3 initialposition, Camera masterCamera, Vector3 offset) {
        this.masterCamera = masterCamera;
        masterCameraTransform = masterCamera.transform;
        // masterCameraInitialPosition = masterCameraTransform.position;
        masterCameraInitialPosition = initialposition;

        myInitialPosition = transform.position + offset;
        // Debug.Log($"intialize skybox with: {offset} {myInitialPosition} {masterCameraInitialPosition}");
        myCamera = GetComponent<Camera>();
    }
    void Update() {
        if (masterCamera == null || masterCameraTransform == null) return;
        Vector3 offset = (masterCameraTransform.position - masterCameraInitialPosition);
        // offset.y = 2f * offset.y;
        transform.position = myInitialPosition + offset * translationRatio;
        // transform.position = myInitialPosition + (masterCameraTransform.position - masterCameraInitialPosition) * translationRatio;
    }
}
