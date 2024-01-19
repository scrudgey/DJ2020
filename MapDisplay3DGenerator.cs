using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DGenerator : MonoBehaviour {
    MapDisplay3DView mapDisplayView;
    public List<MeshRenderer> quads;
    public Material materialFloorHidden;
    public Material materialFloorHighlight;
    List<Texture2D> mapImages;
    [Header("camera")]
    public Camera mapCamera;
    public Transform cameraTransform;
    // public Vector3 
    int currentFloor;
    int numberFloors;
    float theta;
    float thetaVelocity;
    public void Initialize(MapDisplay3DView mapDisplayView, LevelTemplate template, List<Texture2D> mapImages) {
        this.mapDisplayView = mapDisplayView;
        this.mapImages = mapImages;

        numberFloors = mapImages.Count;
        currentFloor = 0;
        theta = 3.925f;
        thetaVelocity = 0.5f;

        for (int i = 0; i < quads.Count; i++) {
            MeshRenderer renderer = quads[i];
            if (i >= mapImages.Count) {
                renderer.enabled = false;
            } else {
                renderer.enabled = true;
                renderer.material = materialFloorHidden;
                renderer.material.mainTexture = mapImages[i];
            }
        }
        SelectFloor(0, 0);
    }


    public void UpdateWithInput(PlayerInput input, float timeDelta, MapDisplay3DView.MouseHeldType mouseHeldType) {
        if (mouseHeldType == MapDisplay3DView.MouseHeldType.left) {
            thetaVelocity = 0f;
            theta += input.mouseDelta.x * timeDelta;
        } else if (mouseHeldType == MapDisplay3DView.MouseHeldType.right) {
            thetaVelocity = 0f;
            // theta += input.mouseDelta.x * timeDelta * 2;
            Vector3 position = mapCamera.transform.position;
            position.x += input.mouseDelta.x * timeDelta;
            position.z += input.mouseDelta.y * timeDelta;
            mapCamera.transform.position = position;
        }
    }
    void Update() {
        theta += thetaVelocity * Time.unscaledDeltaTime;

        // TODO: allow floating origin

        float y = 1.375f + (0.125f * currentFloor);
        float x = 1.412f * Mathf.Cos(theta);
        float z = 1.412f * Mathf.Sin(theta);

        float rotX = 45;
        float rotY = 270 - (45 * (theta / 0.785f));
        float rotZ = 0f;

        if (rotY < 0) {
            rotY += 360f;
        }

        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, rotZ);

        Vector3 targetPosition = new Vector3(x, y, z);
        // Vector3 updatedPosition = Vector3.Lerp(cameraTransform.localPosition, targetPosition, 0.5f);
        Vector3 updatedPosition = targetPosition;

        cameraTransform.localPosition = updatedPosition;
        cameraTransform.localRotation = targetRotation;
    }


    public void FloorIncrementButtonCallback(int increment) {
        int targetFloor = currentFloor + increment;
        targetFloor = Mathf.Max(0, targetFloor);
        targetFloor = Mathf.Min(targetFloor, numberFloors - 1);
        SelectFloor(currentFloor, targetFloor);
    }

    void SelectFloor(int fromFloor, int toFloor) {
        for (int i = 0; i < quads.Count; i++) {
            MeshRenderer renderer = quads[i];
            if (i >= mapImages.Count) {
                renderer.enabled = false;
            } else if (i > toFloor) {
                renderer.enabled = false;
            } else if (i == toFloor) {
                renderer.enabled = true;
                renderer.material = materialFloorHighlight;
                renderer.material.mainTexture = mapImages[toFloor];
            } else if (i < toFloor) {
                renderer.enabled = true;
                renderer.material = materialFloorHidden;
                renderer.material.mainTexture = mapImages[i];
            }
        }

        mapDisplayView.OnFloorSelected(fromFloor, toFloor);
        currentFloor = toFloor;
    }
}
