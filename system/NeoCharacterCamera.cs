using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Easings;

// TODO: eliminate this enum
public enum CameraState { normal, wallPress }

// a single parameter lerps between states, and there is one transitional state.

public struct CameraInput {
    public CameraState state;
    public enum RotateInput { none, left, right }
    public float deltaTime;
    public RotateInput rotation;
    public Vector3 wallNormal;
    public Vector2 lastWallInput;
    public bool crouchHeld;
}
public class NeoCharacterCamera : MonoBehaviour {

    public CameraState state;
    public static Quaternion rotationOffset;
    public PostProcessVolume volume;
    public PostProcessProfile isometricProfile;
    public PostProcessProfile wallPressProfile;
    public Material isometricSkybox;
    public Material wallPressSkybox;
    public Transform maskCylinder;
    [Header("Framing")]
    public Camera Camera;
    public Vector2 FollowPointFraming = new Vector2(0f, 0f);
    public float FollowingSharpness = 10000f;
    public Quaternion isometricRotation;

    [Header("Distance")]
    public float DefaultDistance = 6f;
    public float MinDistance = 0f;
    public float MaxDistance = 3f;
    public float DistanceMovementSpeed = 5f;
    public float DistanceMovementSharpness = 10f;

    [Header("Rotation")]
    public float RotationSpeed = 1f;
    public float RotationSharpness = 10000f;
    Quaternion targetRotation = Quaternion.identity;


    [Header("Obstruction")]
    public float ObstructionCheckRadius = 0.2f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;
    public List<Collider> IgnoredColliders = new List<Collider>();

    public Transform Transform { get; private set; }
    public Transform FollowTransform { get; private set; }

    public Vector3 PlanarDirection { get; set; }
    public float TargetDistance { get; set; }

    private bool _distanceIsObstructed;
    private float _currentDistance;
    private float _targetVerticalAngle;
    private RaycastHit _obstructionHit;
    private int _obstructionCount;
    private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
    private float _obstructionTime;
    private Vector3 _currentFollowPosition;

    private const int MaxObstructions = 32;
    private float transitionTime;
    void OnValidate() {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
        // DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
    }

    void Awake() {
        Transform = this.transform;

        _currentDistance = DefaultDistance;
        TargetDistance = _currentDistance;

        _targetVerticalAngle = 0f;

        PlanarDirection = Vector3.forward;
    }

    // Set the transform that the camera will orbit around
    public void SetFollowTransform(Transform t) {
        FollowTransform = t;
        PlanarDirection = Quaternion.Euler(0, -45, 0) * Vector3.right; // TODO: configurable per level
        _currentFollowPosition = FollowTransform.position;

        // the initial rotation here will be an offset to all subsequent rotations
        rotationOffset = Quaternion.Euler(Vector3.up * 20f);
        Quaternion rotationFromInput = rotationOffset;
        PlanarDirection = rotationFromInput * PlanarDirection;
        PlanarDirection = Vector3.Cross(Vector3.up, Vector3.Cross(PlanarDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, Vector3.up);
        Quaternion verticalRot = Quaternion.Euler(30f, 0, 0);
        targetRotation = planarRot * verticalRot;
    }

    public void UpdateWithInput(CameraInput input) {
        state = input.state;
        if (FollowTransform) {
            switch (input.state) {
                default:
                case CameraState.normal:
                    RenderSettings.skybox = isometricSkybox;
                    if (transitionTime > 0) {
                        transitionTime -= Time.deltaTime;
                    }
                    break;
                case CameraState.wallPress:
                    RenderSettings.skybox = wallPressSkybox;
                    if (transitionTime < 1) {
                        transitionTime += Time.deltaTime;
                    }
                    break;
            }
            transitionTime = Mathf.Clamp(transitionTime, 0, 1f);

            if (state == CameraState.normal) {
                ApplyTargetParameters(NormalUpdate(input));
                volume.profile = isometricProfile;
            } else if (state == CameraState.wallPress) {
                ApplyTargetParameters(WallPressUpdate(input));
                volume.profile = wallPressProfile;
            }
        }
    }
    public class CameraTargetParameters {
        public float fieldOfView;
        public float deltaTime;
        public Quaternion rotation;
        public bool orthographic;
        public float orthographicSize;
        public float targetDistance;
        public Vector3 targetPosition;
    }
    public CameraTargetParameters NormalUpdate(CameraInput input) {
        // Process rotation input
        float rotationInput = 0f;
        switch (input.rotation) {
            case CameraInput.RotateInput.left:
                rotationInput = 90f;
                break;
            case CameraInput.RotateInput.right:
                rotationInput = -90f;
                break;
            default:
            case CameraInput.RotateInput.none:
                break;
        }
        Quaternion verticalRot = Quaternion.Euler(30f, 0, 0);
        Quaternion rotationFromInput = Quaternion.Euler(Vector3.up * rotationInput);// * RotationSpeed));
        PlanarDirection = rotationFromInput * PlanarDirection;
        PlanarDirection = Vector3.Cross(Vector3.up, Vector3.Cross(PlanarDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, Vector3.up);

        return new CameraTargetParameters() {
            fieldOfView = 70f,
            orthographic = true,
            rotation = planarRot * verticalRot,
            deltaTime = input.deltaTime,
            targetDistance = 20,
            targetPosition = FollowTransform.position,
            orthographicSize = 8
        };
    }

    public CameraTargetParameters WallPressUpdate(CameraInput input) {
        // Find the smoothed follow position
        Vector3 LROffset = FollowTransform.right * -0.5f * Mathf.Sign(input.lastWallInput.x);
        Vector3 distOffset = input.wallNormal * TargetDistance;
        Vector3 heightOffset = new Vector3(0, -0.2f, 0);
        if (input.crouchHeld) {
            heightOffset = new Vector3(0, -0.5f, 0);
        }

        Quaternion verticalRot = Quaternion.Euler((float)PennerDoubleAnimation.ExpoEaseIn(transitionTime, 30f, -30, 1f), 0, 0);
        Vector3 camDirection = -1f * input.wallNormal;
        camDirection = Vector3.Cross(Vector3.up, Vector3.Cross(camDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(camDirection, Vector3.up);

        return new CameraTargetParameters() {
            fieldOfView = 45f,
            orthographic = false,
            rotation = planarRot,
            deltaTime = input.deltaTime,
            targetDistance = 1.5f,
            targetPosition = FollowTransform.position + distOffset + LROffset + heightOffset,
            orthographicSize = 4f
        };
    }

    public void ApplyTargetParameters(CameraTargetParameters input) {
        // Process distance input
        TargetDistance = input.targetDistance;
        if (_distanceIsObstructed) {//&& Mathf.Abs(zoomInput) > 0f) {
            TargetDistance = _currentDistance;
        }
        TargetDistance = Mathf.Clamp(TargetDistance, MinDistance, MaxDistance);

        // apply FOV
        // TODO: lerp it
        Camera.fieldOfView = input.fieldOfView;

        // apply orthographic
        Camera.orthographic = input.orthographic;
        // TODO: lerp it
        Camera.orthographicSize = input.orthographicSize;

        // apply rotation
        targetRotation = Quaternion.Slerp(targetRotation, input.rotation, 1f - Mathf.Exp(-RotationSharpness * input.deltaTime));
        Transform.rotation = targetRotation;

        // apply distance
        _currentDistance = Mathf.Lerp(_currentDistance, TargetDistance, 1 - Mathf.Exp(-DistanceMovementSharpness * input.deltaTime));

        // Find the smoothed follow position
        _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, input.targetPosition, 1f - Mathf.Exp(-FollowingSharpness * input.deltaTime));

        // Find the smoothed camera orbit position
        Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);


        // Handle framing
        targetPosition += Transform.right * FollowPointFraming.x;
        targetPosition += Vector3.up * FollowPointFraming.y;

        // Apply position
        Transform.position = targetPosition;
        isometricRotation = Transform.rotation;
    }
}