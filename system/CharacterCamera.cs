using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// TODO: eliminate this enum
public enum CameraState { normal, wallPress, attractor }

// a single parameter lerps between states, and there is one transitional state.

public struct CameraInput {
    // public RotateInput rotation;
    // public CameraAttractorZone attractor;
    // public CameraState state;
    public enum RotateInput { none, left, right }
    public float deltaTime;
    public Vector3 wallNormal;
    public Vector2 lastWallInput;
    public bool crouchHeld;
    public Vector3 playerPosition;
    public CharacterState state;
}
public class CharacterCamera : IBinder<CharacterController>, IInputReceiver {
    private CameraState _state;
    public CameraState state {
        get { return _state; }
    }
    // public CharacterController target { get; set; }
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
    public float followingSharpnessDefault = 10000f;
    public Quaternion isometricRotation;

    [Header("Distance")]
    public float DefaultDistance = 6f;
    public float MinDistance = 0f;
    public float MaxDistance = 3f;
    public float distanceMovementSpeedDefault = 5f;
    public float distanceMovementSharpnessDefault = 10f;

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
    private RaycastHit _obstructionHit;
    private int _obstructionCount;
    private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
    private float _obstructionTime;
    private Vector3 _currentFollowPosition;
    private const int MaxObstructions = 32;
    private float transitionTime;
    private float currentDistanceMovementSharpness;
    private float currentFollowingSharpness;
    private float currentDistanceMovementSpeed;
    private bool clampOrthographicSize;
    private static List<CameraAttractorZone> attractors = new List<CameraAttractorZone>();
    CameraInput.RotateInput currentRotationInput;
    CameraAttractorZone currentAttractor = null;
    void OnValidate() {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
    }

    public void SetInputs(PlayerInput input) {
        currentRotationInput = CameraInput.RotateInput.none;
        if (input.rotateCameraRightPressedThisFrame) {
            currentRotationInput = CameraInput.RotateInput.right;
        } else if (input.rotateCameraLeftPressedThisFrame) {
            currentRotationInput = CameraInput.RotateInput.left;
        }
    }
    void Awake() {
        Transform = this.transform;

        _currentDistance = DefaultDistance;
        TargetDistance = _currentDistance;

        PlanarDirection = Vector3.forward;

        currentDistanceMovementSharpness = distanceMovementSharpnessDefault;
        currentFollowingSharpness = followingSharpnessDefault;
        currentDistanceMovementSpeed = distanceMovementSpeedDefault;
    }

    void Start() {
        GameManager.OnFocusChanged += Bind;
        GameManager.OnFocusChanged += SetFollowTransform;

        // TODO: move into on level load
        attractors = new List<CameraAttractorZone>();
        foreach (GameObject attractor in GameObject.FindGameObjectsWithTag("cameraAttractor")) {
            CameraAttractorZone collider = attractor.GetComponent<CameraAttractorZone>();
            if (collider != null)
                attractors.Add(collider);
        }

        // IgnoredColliders.Clear();
        // IgnoredColliders.AddRange(Character.gameObject.GetComponentsInChildren<Collider>());
    }
    public void TransitionToState(CameraState toState) {
        if (toState == _state)
            return;
        transitionTime = 0f;
        CameraState tmpInitialState = state;
        OnStateExit(tmpInitialState, toState);
        _state = toState;
        OnStateEnter(toState, tmpInitialState);
    }
    private void OnStateEnter(CameraState state, CameraState fromState) {
        // Debug.Log($"entering state {state} from {fromState}");
        switch (state) {
            case CameraState.wallPress:
                currentDistanceMovementSharpness = distanceMovementSharpnessDefault * 3f;
                currentFollowingSharpness = followingSharpnessDefault * 3f;
                currentDistanceMovementSpeed = distanceMovementSpeedDefault * 3f;
                break;
            case CameraState.attractor:
                if (fromState == CameraState.wallPress) {
                    currentDistanceMovementSharpness = distanceMovementSharpnessDefault;
                    currentFollowingSharpness = followingSharpnessDefault;
                    currentDistanceMovementSpeed = distanceMovementSpeedDefault;
                } else {
                    currentDistanceMovementSharpness = 1;
                    currentDistanceMovementSpeed = 0.1f;
                }
                break;
            default:
                currentDistanceMovementSharpness = distanceMovementSharpnessDefault;
                currentFollowingSharpness = followingSharpnessDefault;
                currentDistanceMovementSpeed = distanceMovementSpeedDefault;
                break;
        }
    }
    public void OnStateExit(CameraState state, CameraState toState) {
        switch (state) {
            default:
                break;
        }
    }
    // Set the transform that the camera will orbit around
    public void SetFollowTransform(GameObject g) {
        Debug.Log("set follow transform");

        Transform t = g.transform;
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

        IgnoredColliders.Clear();
        IgnoredColliders.AddRange(t.gameObject.GetComponentsInChildren<Collider>());
    }

    override public void HandleValueChanged(CharacterController controller) {
        CameraInput input = controller.BuildCameraInput();
        UpdateWithInput(input);
    }
    public void UpdateWithInput(CameraInput input) {
        CameraState camState = CameraState.normal;
        currentAttractor = null;
        if (input.state == CharacterState.wallPress) {
            camState = CameraState.wallPress;
        } else {
            // check / update Attractor
            foreach (CameraAttractorZone attractor in attractors) {
                if (attractor.sphereCollider.bounds.Contains(input.playerPosition)) {
                    currentAttractor = attractor;
                    camState = CameraState.attractor;
                    break;
                }
            }
        }

        TransitionToState(camState);
        if (transitionTime < 1) {
            transitionTime += Time.deltaTime;
        }
        switch (camState) {
            default:
            case CameraState.normal:
                RenderSettings.skybox = isometricSkybox;
                break;
            case CameraState.wallPress:
                RenderSettings.skybox = wallPressSkybox;
                break;
        }
        transitionTime = Mathf.Clamp(transitionTime, 0, 1f);

        if (state == CameraState.attractor) {
            ApplyTargetParameters(AttractorUpdate(input));
            volume.profile = isometricProfile;
        } else if (state == CameraState.normal) {
            ApplyTargetParameters(NormalUpdate(input));
            volume.profile = isometricProfile;
        } else if (state == CameraState.wallPress) {
            ApplyTargetParameters(WallPressUpdate(input));
            volume.profile = wallPressProfile;
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
        public float followingSharpness;
        public float distanceMovementSpeed;
        public float distanceMovementSharpness;
        public CharacterState state;
    }
    public CameraTargetParameters NormalUpdate(CameraInput input) {

        Vector3 targetPosition = FollowTransform?.position ?? Vector3.zero;
        // Process rotation input
        float rotationInput = 0f;
        switch (currentRotationInput) {
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
        currentRotationInput = CameraInput.RotateInput.none;
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
            targetPosition = targetPosition,
            orthographicSize = 6, // 8 TODO: set by attractor and/or level
            distanceMovementSharpness = currentDistanceMovementSharpness,
            followingSharpness = currentFollowingSharpness,
            distanceMovementSpeed = currentDistanceMovementSpeed,
            state = input.state
        };
    }
    public CameraTargetParameters AttractorUpdate(CameraInput input) {

        CameraTargetParameters parameters = NormalUpdate(input);
        if (currentAttractor != null) {
            parameters.followingSharpness = currentAttractor.movementSharpness;
            Vector3 delta = FollowTransform?.position - currentAttractor.sphereCollider.bounds.center ?? Vector3.zero;
            if (currentAttractor.useInnerFocus && delta.magnitude < currentAttractor.innerFocusRadius) {
                parameters.orthographicSize = currentAttractor.innerFocusOrthographicSize;
            } else {
                parameters.targetPosition = currentAttractor.sphereCollider.bounds.center;
            }
        }
        return parameters;
    }
    public CameraTargetParameters WallPressUpdate(CameraInput input) {
        // Find the smoothed follow position
        Vector3 LROffset = FollowTransform.right * -0.5f * Mathf.Sign(input.lastWallInput.x);
        Vector3 distOffset = input.wallNormal * TargetDistance;
        Vector3 heightOffset = new Vector3(0, 1f, 0);
        if (input.crouchHeld) {
            heightOffset = new Vector3(0, 0.5f, 0);
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
            orthographicSize = 4f,
            distanceMovementSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            followingSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            distanceMovementSpeed = currentDistanceMovementSpeed,
            state = input.state
        };
    }

    bool PlayerInsideBounds(Vector3 screenPoint) {
        return screenPoint.x > 0.1 && screenPoint.y > 0.1 && screenPoint.x < 0.9 && screenPoint.y < 0.9;
    }
    public void ApplyTargetParameters(CameraTargetParameters input) {
        if (FollowTransform == null)
            return;
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
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(FollowTransform.position);
        float desiredOrthographicSize = input.orthographicSize;
        float previousOrthographicSize = Camera.orthographicSize;

        if (input.state != CharacterState.wallPress) {
            int i = 0;
            if (PlayerInsideBounds(screenPoint)) {
                if (Camera.orthographicSize < desiredOrthographicSize) {
                    while (i < 10 && Camera.orthographicSize < desiredOrthographicSize && PlayerInsideBounds(screenPoint)) {
                        i++;
                        Camera.orthographicSize += 0.01f;
                        screenPoint = Camera.main.WorldToViewportPoint(FollowTransform.position);
                    }
                    Camera.orthographicSize = Math.Min(desiredOrthographicSize, Camera.orthographicSize);
                } else if (Camera.orthographicSize > desiredOrthographicSize) {
                    while (i < 10 && Camera.orthographicSize > desiredOrthographicSize && PlayerInsideBounds(screenPoint)) {
                        i++;
                        Camera.orthographicSize -= 0.01f;
                        screenPoint = Camera.main.WorldToViewportPoint(FollowTransform.position);
                    }
                    if (i == 1) {
                        Camera.orthographicSize = previousOrthographicSize;
                    } else {
                        Camera.orthographicSize = Math.Max(desiredOrthographicSize, Camera.orthographicSize);
                    }
                }
            } else {
                while (i < 10 && !PlayerInsideBounds(screenPoint)) {
                    i++;
                    Camera.orthographicSize += 0.01f;
                    screenPoint = Camera.main.WorldToViewportPoint(FollowTransform.position);
                }
            }
        }

        // Debug.Log(Camera.orthographicSize);

        // apply rotation
        targetRotation = Quaternion.Slerp(targetRotation, input.rotation, 1f - Mathf.Exp(-RotationSharpness * input.deltaTime));
        Transform.rotation = targetRotation;

        // apply distance
        _currentDistance = Mathf.Lerp(_currentDistance, TargetDistance, 1 - Mathf.Exp(-input.distanceMovementSharpness * input.deltaTime));

        // Debug.Log(TargetDistance);
        // Debug.Log(_currentDistance);

        // Find the smoothed follow position
        _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, input.targetPosition, 1f - Mathf.Exp(-input.followingSharpness * input.deltaTime));

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