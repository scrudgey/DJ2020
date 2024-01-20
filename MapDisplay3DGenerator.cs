using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapDisplay3DGenerator : MonoBehaviour, IBindable<MapDisplay3DGenerator> {
    enum Mode { playerfocus, rotate }
    public Action<MapDisplay3DGenerator> OnValueChanged { get; set; }
    Mode mode;
    MapDisplay3DView mapDisplayView;
    public List<MeshRenderer> quads;
    public Material materialFloorHidden;
    public Material materialFloorHighlight;
    List<Texture2D> mapImages;
    [Header("camera")]
    public Camera mapCamera;
    public Transform cameraTransform;
    [Header("textures")]
    public PixelUpscaleRender upscaleRender;
    public RawImage mapViewImage;
    public RenderTexture texture_256;
    public RenderTexture texture_512;
    public RenderTexture texture_1024;
    public RenderTexture texture_2048;
    // public Vector3 
    int currentFloor;
    int numberFloors;
    float theta;
    float thetaVelocity;
    Vector3 origin;
    float zoomFloatAmount;
    int zoomLevel;
    LevelTemplate template;
    public void Initialize(MapDisplay3DView mapDisplayView, LevelTemplate template, List<Texture2D> mapImages) {
        this.mapDisplayView = mapDisplayView;
        this.mapImages = mapImages;
        this.template = template;

        numberFloors = mapImages.Count;
        currentFloor = 0;
        theta = 3.925f;

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
        zoomFloatAmount = 1.5f;
        SetZoomLevel(1);
        SelectFloor(0, 0);
        ChangeMode(Mode.rotate);
    }


    public void UpdateWithInput(PlayerInput input, float timeDelta, MapDisplay3DView.MouseHeldType mouseHeldType) {
        if (mouseHeldType == MapDisplay3DView.MouseHeldType.right) {
            thetaVelocity = 0f;
            theta += input.mouseDelta.x * timeDelta;
        } else if (mouseHeldType == MapDisplay3DView.MouseHeldType.left) {
            thetaVelocity = 0f;
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(mapCamera.transform.rotation * Vector3.forward, Vector3.up).normalized;
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Vector3.up);
            Vector3 inputDirection = new Vector3(input.mouseDelta.x, 0, input.mouseDelta.y);
            Vector3 translation = -1f * (cameraPlanarRotation * inputDirection);
            origin += translation * timeDelta * mapCamera.orthographicSize;
            origin = Vector3.ClampMagnitude(origin, 1f);
        }
        HandleZoomInput(input.zoomInput.y * timeDelta);
    }
    void HandleZoomInput(float increment) {
        zoomFloatAmount += increment;
        zoomFloatAmount = Mathf.Min(zoomFloatAmount, 4f);
        zoomFloatAmount = Mathf.Max(zoomFloatAmount, 0f);

        if ((int)zoomFloatAmount != zoomLevel) {
            SetZoomLevel((int)zoomFloatAmount);
        }
    }
    void Update() {
        theta += thetaVelocity * Time.unscaledDeltaTime;
        if (theta < -6.28) {
            theta += 6.28f;
        } else if (theta > 6.28) {
            theta -= 6.28f;
        }

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

        Vector3 targetPosition = new Vector3(x, y, z) + origin;
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

    public void ZoomIncrementButtonCallback(int increment) {
        HandleZoomInput(increment);
    }
    public void ModeButtonCallback() {
        Mode newMode = mode switch {
            Mode.playerfocus => Mode.rotate,
            Mode.rotate => Mode.playerfocus,
            _ => Mode.rotate
        };
        ChangeMode(newMode);
    }
    void ChangeMode(Mode newMode) {
        mode = newMode;
        switch (mode) {
            case Mode.playerfocus:
                origin = WorldToQuadPosition(GameManager.I.playerPosition, 0);
                thetaVelocity = 0f;
                break;
            case Mode.rotate:
                thetaVelocity = 0.5f;
                origin = Vector3.zero;
                break;
        }
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

    void SetZoomLevel(int level) {
        // small: 1024 large:   2045    camera: 1
        // small: 512  large: 1024     camera: 0.5
        // small: 256  large:  512     camera: 0.25
        //                             camera: 0.125
        zoomLevel = level;
        switch (level) {
            case 0:
                upscaleRender.small = texture_1024;
                upscaleRender.large = texture_2048;
                mapCamera.orthographicSize = 1;
                break;
            case 1:
                upscaleRender.small = texture_512;
                upscaleRender.large = texture_1024;
                mapCamera.orthographicSize = 0.5f;
                break;
            case 2:
                upscaleRender.small = texture_256;
                upscaleRender.large = texture_512;
                mapCamera.orthographicSize = 0.25f;
                break;
            case 3:
                upscaleRender.small = texture_256;
                upscaleRender.large = texture_512;
                mapCamera.orthographicSize = 0.125f;
                break;
        }
        mapCamera.targetTexture = upscaleRender.large;
        mapViewImage.texture = upscaleRender.small;
    }
    public string GetStatsString() {
        return $"{zoomFloatAmount:F2}\n{theta:F2}\n({origin.x:F2}, {origin.z:F2})";
    }

    public Vector3 WorldToQuadPosition(Vector3 worldPosition, int floorNumber) {
        // transform to quad position
        Vector2 quadPosition = new Vector2(
            template.mapUnitNorth.x * worldPosition.x,
            template.mapUnitEast.y * worldPosition.z) + new Vector2(template.mapOrigin.x, template.mapOrigin.y);

        // transform to map generator position
        MeshRenderer quad = quads[0];
        Vector3 generatorPosition = new Vector3(
                quad.bounds.extents.x * quadPosition.x * 2,
                0f,
                quad.bounds.extents.z * quadPosition.y * 2) + quad.transform.position - quad.bounds.extents;

        generatorPosition.y += floorNumber * 0.125f;

        return generatorPosition;
    }
}