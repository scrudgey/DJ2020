using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using Easings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

// TODO: eliminate this enum
public enum CameraState { normal, wallPress, attractor, aim, burgle, free, firstPerson, overlayView }
public enum IsometricOrientation { NE, SE, SW, NW }

public class CharacterCamera : MonoBehaviour, IInputReceiver, IBindable<CharacterCamera> { //IBinder<CharacterController>, 
    public Action<CharacterCamera> OnValueChanged { get; set; }
    public static bool ORTHOGRAPHIC_MODE = true;
    // public static bool ORTHOGRAPHIC_MODE = false;
    public IsometricOrientation initialOrientation;
    public IsometricOrientation currentOrientation;
    private CameraState _state;
    public CameraState state {
        get { return _state; }
    }
    public OutlineEffect outlineEffect;
    public static Quaternion rotationOffset;
    public PostProcessVolume volume;
    public PostProcessProfile isometricProfile;
    public PostProcessProfile wallPressProfile;
    public PostProcessProfile aimProfile;
    public PostProcessProfile thermalProfile;
    public Material isometricSkybox;
    public Material wallPressSkybox;
    public Transform maskCylinder;
    public Camera[] skyBoxCameras = new Camera[0];
    public Camera[] subCameras;
    [Header("Framing")]
    public Camera Camera;
    public Vector2 FollowPointFraming = new Vector2(0f, 0f);
    public float followingSharpnessDefault = 5f;
    public Quaternion isometricRotation;
    public float followCursorCoefficient = 5f;

    [Header("Distance")]
    public float DefaultDistance = 6f;
    public float MinDistance = 0f;
    public float MaxDistance = 3f;
    public float distanceMovementSpeedDefault = 5f;
    public float distanceMovementSharpnessDefault = 10f;

    [Header("Rotation")]
    public float RotationSpeed = 1f;
    public float RotationSharpness = 1f;
    Quaternion targetRotation = Quaternion.identity;
    public Quaternion idealRotation = Quaternion.identity;
    public float initialRotationOffset = 20f;
    public float verticalRotationOffset = 30f;

    [Header("Obstruction")]
    public float ObstructionCheckRadius = 0.2f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;
    public List<Collider> IgnoredColliders = new List<Collider>();

    public Transform Transform { get; private set; }
    public Vector3 PlanarDirection { get; set; }
    public float TargetDistance { get; set; }
    public bool disableLockOn;

    private float _currentDistance;
    private RaycastHit _obstructionHit;
    private int _obstructionCount;
    private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
    private float _obstructionTime;
    public Vector3 _currentFollowPosition;
    private const int MaxObstructions = 32;
    public float transitionTime;
    private float currentDistanceMovementSharpness;
    private float currentFollowingSharpness;
    private float currentDistanceMovementSpeed;
    private bool clampOrthographicSize;
    public Vector3 lastTargetPosition;
    public float zoomCoefficient = 1f;
    public float zoomCoefficientTarget = 1f;
    float zoomVelocity = 0.0f;
    private bool horizontalAimParity;
    private Dictionary<Quaternion, IsometricOrientation> cardinalDirections;
    private static List<CameraAttractorZone> attractors = new List<CameraAttractorZone>();
    CameraInput.RotateInput currentRotationInput;
    CameraAttractorZone currentAttractor = null;
    private static float shakeJitter = 0f;
    private float currentOrthographicSize;
    public Vector3 cullingTargetPosition;
    bool thermalGogglesActive;
    CameraTargetParameters lastWallPressTargetParameters;
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
        zoomCoefficientTarget -= input.zoomInput.y * Time.unscaledDeltaTime * 0.1f;
        zoomCoefficientTarget = Math.Clamp(zoomCoefficientTarget, 0.25f, 1.0f);
        zoomCoefficient = Mathf.SmoothDamp(zoomCoefficient, zoomCoefficientTarget, ref zoomVelocity, 0.05f, 100f, Time.unscaledDeltaTime);
    }
    void Awake() {
        Transform = this.transform;

        _currentDistance = DefaultDistance;
        TargetDistance = _currentDistance;

        PlanarDirection = Vector3.forward;

        currentDistanceMovementSharpness = distanceMovementSharpnessDefault;
        currentFollowingSharpness = followingSharpnessDefault;
        currentDistanceMovementSpeed = distanceMovementSpeedDefault;

        // initial planar
        // the initial rotation here will be an offset to all subsequent rotations
        float initialPlanarAngle = initialOrientation switch {
            IsometricOrientation.NE => 45f,
            IsometricOrientation.SE => 135f,
            IsometricOrientation.SW => 225f,
            IsometricOrientation.NW => 315f,
            _ => 45f
        };

        PlanarDirection = Quaternion.Euler(0, initialPlanarAngle, 0) * Vector3.right;
        rotationOffset = Quaternion.Euler(Vector3.up * initialRotationOffset);
        Quaternion rotationFromInput = rotationOffset;
        PlanarDirection = rotationFromInput * PlanarDirection;
        PlanarDirection = Vector3.Cross(Vector3.up, Vector3.Cross(PlanarDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, Vector3.up);
        Quaternion verticalRot = Quaternion.Euler(verticalRotationOffset, 0, 0);
        targetRotation = planarRot * verticalRot;

        cardinalDirections = new Dictionary<Quaternion, IsometricOrientation>(){
            {Quaternion.Euler(0f, 45f, 0f) * rotationFromInput, IsometricOrientation.NE},
            {Quaternion.Euler(0f, 135f, 0f) * rotationFromInput, IsometricOrientation.SE},
            {Quaternion.Euler(0f, 225, 0f) * rotationFromInput, IsometricOrientation.SW},
            {Quaternion.Euler(0f, 315, 0f) * rotationFromInput, IsometricOrientation.NW},
        };
    }

    void Start() {
        _currentDistance = 20f;
        GameManager.OnFocusChanged += SetFollowTransform;
        SetFollowTransform(GameManager.I.playerObject);
        GameManager.OnEyeVisibilityChange += HandleEyeVisibilityChange;

        // TODO: move into on level load
        attractors = new List<CameraAttractorZone>();
        foreach (GameObject attractor in GameObject.FindGameObjectsWithTag("cameraAttractor")) {
            CameraAttractorZone collider = attractor.GetComponent<CameraAttractorZone>();
            if (collider != null)
                attractors.Add(collider);
        }
    }
    void HandleEyeVisibilityChange(PlayerState playerData) {
        thermalGogglesActive = playerData.cyberEyesThermalBuff;
        if (playerData.cyberEyesThermal || playerData.cyberEyesThermalBuff) {
            QualitySettings.shadowDistance = 0;
            ShowLasers();
        } else {
            QualitySettings.shadowDistance = 1000;
            HideLasers();
        }
    }
    public void OnDestroy() {
        GameManager.OnFocusChanged -= SetFollowTransform;
        GameManager.OnEyeVisibilityChange -= HandleEyeVisibilityChange;
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
        Camera.clearFlags = CameraClearFlags.SolidColor;
        switch (state) {
            case CameraState.free:
                Camera.clearFlags = CameraClearFlags.Skybox;
                break;
            case CameraState.firstPerson:
                Camera.clearFlags = CameraClearFlags.Skybox;
                currentDistanceMovementSharpness = distanceMovementSharpnessDefault * 3f;
                currentFollowingSharpness = followingSharpnessDefault * 3f;
                currentDistanceMovementSpeed = distanceMovementSpeedDefault * 3f;
                break;
            case CameraState.normal:
                Camera.clearFlags = CameraClearFlags.Skybox;
                if (fromState == CameraState.aim || fromState == CameraState.wallPress) {
                    // kind of hacky but w/e
                    _currentDistance = 20f;
                }
                break;
            case CameraState.aim:
                Camera.clearFlags = CameraClearFlags.Skybox;
                currentDistanceMovementSharpness = distanceMovementSharpnessDefault * 3f;
                currentFollowingSharpness = followingSharpnessDefault * 3f;
                currentDistanceMovementSpeed = distanceMovementSpeedDefault * 3f;
                break;
            case CameraState.wallPress:
                Camera.clearFlags = CameraClearFlags.Skybox;
                currentDistanceMovementSharpness = distanceMovementSharpnessDefault * 3f;
                currentFollowingSharpness = followingSharpnessDefault * 3f;
                currentDistanceMovementSpeed = distanceMovementSpeedDefault * 3f;
                break;
            case CameraState.attractor:
                if (fromState == CameraState.aim || fromState == CameraState.wallPress) {
                    // kind of hacky but w/e
                    _currentDistance = 20f;
                }
                if (fromState == CameraState.wallPress || fromState == CameraState.burgle) {
                    currentDistanceMovementSharpness = distanceMovementSharpnessDefault;
                    currentFollowingSharpness = followingSharpnessDefault;
                    currentDistanceMovementSpeed = distanceMovementSpeedDefault;
                } else {
                    currentDistanceMovementSharpness = 1;
                    currentDistanceMovementSpeed = 0.1f;
                }
                break;
            case CameraState.burgle:
                currentFollowingSharpness = followingSharpnessDefault * 3f;
                currentDistanceMovementSharpness = distanceMovementSharpnessDefault * 10f;
                break;
            default:
                Camera.clearFlags = CameraClearFlags.SolidColor;
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
    public void SetFollowTransform(GameObject g) {
        if (g == null)
            return;
        IgnoredColliders.Clear();
        IgnoredColliders.AddRange(g.GetComponentsInChildren<Collider>());
    }

    public void UpdateWithInput(CameraInput input) {
        if (Camera == null) return;
        CameraState camState = input.cameraState;
        currentAttractor = null;

        if (camState == CameraState.normal && !input.ignoreAttractor) {
            // check / update Attractor
            foreach (CameraAttractorZone attractor in attractors) {
                if (attractor == null) continue;
                if (attractor.sphereCollider.bounds.Contains(input.targetPosition)) {
                    currentAttractor = attractor;
                    camState = CameraState.attractor;
                    break;
                }
            }
        }

        TransitionToState(camState);
        if (transitionTime < 1) {
            transitionTime += Time.unscaledDeltaTime;
        }
        transitionTime = Mathf.Clamp(transitionTime, 0, 1f);
        switch (state) {
            case CameraState.attractor:
                ApplyTargetParameters(AttractorParameters(input));
                volume.profile = isometricProfile;
                SetSkyBoxCamerasEnabled(false);
                break;
            default:
            case CameraState.overlayView:
            case CameraState.normal:
                if (ORTHOGRAPHIC_MODE) {
                    RenderSettings.skybox = isometricSkybox;
                    Camera.clearFlags = CameraClearFlags.SolidColor;
                } else {
                    RenderSettings.skybox = wallPressSkybox;
                    Camera.clearFlags = CameraClearFlags.Skybox;
                }
                ApplyTargetParameters(NormalParameters(input));
                volume.profile = isometricProfile;
                SetSkyBoxCamerasEnabled(false);
                break;
            case CameraState.wallPress:
                RenderSettings.skybox = wallPressSkybox;
                ApplyTargetParameters(WallPressParameters(input));
                volume.profile = wallPressProfile;
                SetSkyBoxCamerasEnabled(true);
                break;
            case CameraState.burgle:
                RenderSettings.skybox = wallPressSkybox;
                if (input.currentAttackSurface != null && input.currentAttackSurface.useBurgleCam) {
                    ApplyTargetParameters(BurgleParameters(input));
                } else {
                    ApplyTargetParameters(NormalParameters(input));
                }
                volume.profile = aimProfile;
                SetSkyBoxCamerasEnabled(false);
                break;
            case CameraState.aim:
                RenderSettings.skybox = wallPressSkybox;
                ApplyTargetParameters(AimParameters(input));
                volume.profile = aimProfile;
                SetSkyBoxCamerasEnabled(true);
                break;
            case CameraState.firstPerson:
                RenderSettings.skybox = wallPressSkybox;
                ApplyTargetParameters(FirstPersonParameters(input));
                volume.profile = aimProfile;
                SetSkyBoxCamerasEnabled(true);
                break;
            case CameraState.free:
                RenderSettings.skybox = wallPressSkybox;
                ApplyTargetParameters(FreeCamParameters(input));
                volume.profile = isometricProfile;
                SetSkyBoxCamerasEnabled(true);
                break;
        }
        if (thermalGogglesActive) {
            volume.profile = thermalProfile;
        }
        foreach (Camera subCamera in subCameras) {
            subCamera.orthographicSize = Camera.orthographicSize;
            subCamera.orthographic = Camera.orthographic;
            subCamera.fieldOfView = Camera.fieldOfView;
        }
        cullingTargetPosition = input.cullingTargetPosition;
        OnValueChanged?.Invoke(this);
    }
    public void SetSkyBoxCamerasEnabled(bool enabled) {
        foreach (Camera camera in skyBoxCameras) {
            if (camera == null) continue;
            camera.enabled = enabled;
        }
    }

    public CameraTargetParameters NormalParameters(CameraInput input) {
        // Process rotation input
        float rotationInputDegrees = 0f;
        switch (currentRotationInput) {
            case CameraInput.RotateInput.left:
                rotationInputDegrees = 90f;
                break;
            case CameraInput.RotateInput.right:
                rotationInputDegrees = -90f;
                break;
            default:
            case CameraInput.RotateInput.none:
                break;
        }
        Quaternion verticalRot = Quaternion.Euler(verticalRotationOffset, 0, 0);
        Quaternion rotationFromInput = Quaternion.Euler(Vector3.up * rotationInputDegrees);// * RotationSpeed));
        PlanarDirection = rotationFromInput * PlanarDirection;
        PlanarDirection = Vector3.Cross(Vector3.up, Vector3.Cross(PlanarDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, Vector3.up);

        Quaternion closestCardinal = Toolbox.SnapToClosestRotation(planarRot, cardinalDirections.Keys.ToList());
        currentOrientation = cardinalDirections[closestCardinal];

        float desiredOrthographicSize = 1f;
        float fieldOfView = 70f;
        if (ORTHOGRAPHIC_MODE) {
            desiredOrthographicSize = 9 * zoomCoefficient;
            desiredOrthographicSize = Math.Max(1, desiredOrthographicSize);
            desiredOrthographicSize = Math.Min(10, desiredOrthographicSize);
        } else {
            desiredOrthographicSize = zoomCoefficient * 10f;
            desiredOrthographicSize = Math.Max(3f, desiredOrthographicSize);
            desiredOrthographicSize = Math.Min(20f, desiredOrthographicSize);
            // orthographic size is half-size physical length
            fieldOfView = (float)Mathf.Atan(desiredOrthographicSize / (_currentDistance * Camera.aspect)) * (360f / 6.28f) * 2f;
        }
        if (input.snapToOrthographicSize) {
            desiredOrthographicSize = input.orthographicSize;
            currentOrthographicSize = input.orthographicSize;
            zoomCoefficient = desiredOrthographicSize / 9f;
            zoomCoefficientTarget = zoomCoefficient;
        }
        currentOrthographicSize = desiredOrthographicSize;

        Vector3 targetPosition = lastTargetPosition;
        if (input.characterState == CharacterState.superJump || input.characterState == CharacterState.landStun) {
            zoomCoefficientTarget += Time.unscaledDeltaTime * 0.1f;
        } else if (transitionTime >= 0.1f && input.targetData != null) {
            Vector3 screenOffset = Vector3.zero;
            if (state == CameraState.overlayView) {
                screenOffset = new Vector2(currentOrthographicSize / 3f, 0f);
            } else if (state == CameraState.burgle) {
                // orthographic size is half-size in world units
                screenOffset = new Vector2(currentOrthographicSize, 0f);
            } else {
                screenOffset = (input.targetData.screenPositionNormalized - new Vector2(0.5f, 0.5f));
                screenOffset *= screenOffset.sqrMagnitude;
                screenOffset *= followCursorCoefficient;
            }
            Vector3 worldOffset = planarRot * new Vector3(screenOffset.x, 0f, screenOffset.y);
            targetPosition = input.targetPosition + worldOffset;
            lastTargetPosition = targetPosition;
        }
        return new CameraTargetParameters() {
            fieldOfView = fieldOfView,
            orthographic = ORTHOGRAPHIC_MODE,
            rotation = planarRot * verticalRot,
            deltaTime = input.deltaTime,
            targetDistance = MaxDistance,
            targetPosition = targetPosition,
            orthographicSize = currentOrthographicSize,
            distanceMovementSharpness = currentDistanceMovementSharpness,
            followingSharpness = currentFollowingSharpness,
            minDistance = MinDistance,
            snapTo = input.snapTo
        };
    }
    public CameraTargetParameters AttractorParameters(CameraInput input) {
        CameraTargetParameters parameters = NormalParameters(input);
        if (currentAttractor != null) {
            parameters.followingSharpness = currentAttractor.movementSharpness;
            if (ORTHOGRAPHIC_MODE) {
                parameters.targetPosition = (currentAttractor.sphereCollider.bounds.center * (zoomCoefficient - 0.25f)) + (parameters.targetPosition * (1.25f - zoomCoefficient));
            } else {
                parameters.targetPosition = (currentAttractor.sphereCollider.bounds.center * (zoomCoefficient - 0.25f)) + (parameters.targetPosition * (1f - zoomCoefficient));
                parameters.targetPosition.y = input.targetPosition.y;
            }
        }
        return parameters;
    }
    public CameraTargetParameters WallPressParameters(CameraInput input) {
        // Find the smoothed follow position
        Vector3 LROffset = Vector3.zero;
        LROffset = input.targetTransform.right * -0.5f * Mathf.Sign(input.lastWallInput.x);
        // if we are at wall edge?
        if (input.atLeftEdge) {
            LROffset += 0.9f * input.targetTransform.right;
        } else if (input.atRightEdge) {
            LROffset += -0.9f * input.targetTransform.right;
        }
        if (input.characterState == CharacterState.popout) LROffset = -1f * LROffset;

        Vector3 distOffset = input.wallNormal * TargetDistance;
        // Vector3 heightOffset = new Vector3(0, -0.5f, 0);
        Vector3 heightOffset = Vector3.zero;

        if (input.crouchHeld) {
            heightOffset = new Vector3(0, -0.75f, 0);
        }

        Quaternion verticalRot = Quaternion.Euler((float)PennerDoubleAnimation.ExpoEaseIn(transitionTime, verticalRotationOffset, -1f * verticalRotationOffset, 1f), 0, 0);
        Vector3 camDirection = -1f * input.wallNormal;
        camDirection = Vector3.Cross(Vector3.up, Vector3.Cross(camDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(camDirection, Vector3.up);

        CameraTargetParameters targetParameters = new CameraTargetParameters() {
            fieldOfView = 50f,
            orthographic = false,
            rotation = planarRot,
            deltaTime = input.deltaTime,
            targetDistance = 1.5f,
            targetPosition = input.targetPosition + distOffset + LROffset + heightOffset,
            orthographicSize = 4f,
            distanceMovementSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            followingSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            minDistance = MinDistance
        };

        if (input.characterState == CharacterState.wallPress || input.characterState == CharacterState.burgle) {
            lastWallPressTargetParameters = targetParameters;
            return targetParameters;
        } else {
            return lastWallPressTargetParameters;
        }

    }
    public CameraTargetParameters AimParameters(CameraInput input) {
        float verticalPixels = (Camera.scaledPixelHeight / Camera.aspect);
        float horizontalPixels = (Camera.scaledPixelHeight * Camera.aspect);
        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        Vector2 cursorPositionNormalized = new Vector2(cursorPosition.x / horizontalPixels, cursorPosition.y / verticalPixels) + new Vector2(0f, -0.5f);
        Vector3 distOffset = Vector3.zero;

        Vector3 heightOffset = Vector3.zero;
        if (input.crouchHeld) {
            heightOffset -= new Vector3(0, 0.5f, 0);
        }

        // if (cursorPositionNormalized.x > 0.9f) {
        //     horizontalAimParity = true;
        // }
        // if (cursorPositionNormalized.x < 0.1f) {
        //     horizontalAimParity = false;
        // }

        float horizontalParity = horizontalAimParity ? 1f : -1f;
        Vector3 horizontalOffset = horizontalParity * Vector3.Cross(Vector3.up, input.playerDirection) * 0.65f;

        Quaternion cameraRotation = Quaternion.identity;
        if (GameManager.I.inputMode == InputMode.aim) {
            cameraRotation = input.targetRotation;
        } else {
            input.playerDirection.y = 0f;
            Vector3 camDirectionPoint = Vector3.zero;
            camDirectionPoint += input.playerDirection * 7;
            camDirectionPoint += Vector3.up * cursorPositionNormalized.y;// + new Vector3(0f, -0.5f, 0f);
            camDirectionPoint += Vector3.Cross(Vector3.up, input.playerDirection) * cursorPositionNormalized.x;
            cameraRotation = Quaternion.LookRotation(camDirectionPoint, Vector3.up);
        }


        // // update PlanarDirection to snap to nearest of 4 quadrants
        Quaternion closestCardinal = Toolbox.SnapToClosestRotation(cameraRotation, cardinalDirections.Keys.ToList());
        PlanarDirection = closestCardinal * Vector3.forward;

        return new CameraTargetParameters() {
            fieldOfView = 75f,
            orthographic = false,
            rotation = cameraRotation,
            deltaTime = input.deltaTime,
            targetDistance = 0.5f,
            targetPosition = input.targetPosition + horizontalOffset + heightOffset,
            orthographicSize = 4f,
            // distanceMovementSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            // followingSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            rotationSharpness = 1000f,
            distanceMovementSharpness = 1000f,
            followingSharpness = 1000f,
            minDistance = MinDistance

        };
    }
    public CameraTargetParameters FirstPersonParameters(CameraInput input) {
        float verticalPixels = (Camera.scaledPixelHeight / Camera.aspect);
        float horizontalPixels = (Camera.scaledPixelHeight * Camera.aspect);
        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        Vector2 cursorPositionNormalized = new Vector2(cursorPosition.x / horizontalPixels, cursorPosition.y / verticalPixels) + new Vector2(0f, -0.5f);

        Quaternion cameraRotation = Quaternion.identity;
        cameraRotation = input.targetRotation;

        // update PlanarDirection to snap to nearest of 4 quadrants
        Quaternion closestCardinal = Toolbox.SnapToClosestRotation(cameraRotation, cardinalDirections.Keys.ToList());
        PlanarDirection = closestCardinal * Vector3.forward;

        return new CameraTargetParameters() {
            fieldOfView = 65f,
            orthographic = false,
            rotation = cameraRotation,
            deltaTime = input.deltaTime,
            targetDistance = 0f,
            targetPosition = input.targetPosition,
            orthographicSize = 4f,
            distanceMovementSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            followingSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            minDistance = 0
        };
    }
    public CameraTargetParameters BurgleParameters(CameraInput input) {
        return new CameraTargetParameters() {
            fieldOfView = 50f,
            orthographic = false,
            rotation = GameManager.I.activeBurgleTargetData.target.mainCameraPosition.rotation,
            deltaTime = input.deltaTime,
            targetDistance = 0f,
            targetPosition = GameManager.I.activeBurgleTargetData.target.mainCameraPosition.position,
            orthographicSize = 4f,
            distanceMovementSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 200, -199, 1),
            followingSharpness = (float)PennerDoubleAnimation.ExpoEaseOut(transitionTime, 10, 1, 1),
            minDistance = 0
        };
    }
    public CameraTargetParameters FreeCamParameters(CameraInput input) {
        return new CameraTargetParameters() {
            fieldOfView = 65f,
            orthographic = false,
            rotation = input.targetRotation,
            deltaTime = input.deltaTime,
            targetDistance = 0f,
            targetPosition = input.targetPosition,
            orthographicSize = 4f,
            distanceMovementSharpness = 100,
            followingSharpness = 100,
            minDistance = 0,
            snapTo = true
        };
    }

    bool PlayerInsideBounds(Vector3 screenPoint) {
        return screenPoint.x > 0.1 && screenPoint.y > 0.1 && screenPoint.x < 0.9 && screenPoint.y < 0.9;
    }
    public void ApplyTargetParameters(CameraTargetParameters input) {
        // Process distance input
        TargetDistance = input.targetDistance;
        TargetDistance = Mathf.Clamp(TargetDistance, input.minDistance, MaxDistance);

        // apply orthographic
        Camera.orthographicSize = input.orthographicSize;
        Camera.orthographic = input.orthographic;
        Camera.fieldOfView = input.fieldOfView;

        // apply rotation
        if (input.snapTo) {
            Transform.rotation = input.rotation;
            Transform.position = input.targetPosition;
            if (state == CameraState.normal) {
                // Find the smoothed camera orbit position
                Transform.position = input.targetPosition - ((input.rotation * Vector3.forward) * input.targetDistance);
            }
            _currentFollowPosition = input.targetPosition;
            _currentDistance = input.targetDistance;
            targetRotation = input.rotation;

        } else {
            idealRotation = input.rotation;
            targetRotation = Quaternion.Slerp(targetRotation, input.rotation, 1f - Mathf.Exp(-input.rotationSharpness * input.deltaTime));

            Transform.rotation = targetRotation;

            // apply distance
            _currentDistance = Mathf.Lerp(_currentDistance, TargetDistance, 1 - Mathf.Exp(-input.distanceMovementSharpness * input.deltaTime));

            // Find the smoothed follow position
            Vector3 followPosition = input.targetPosition + (UnityEngine.Random.insideUnitSphere * shakeJitter);
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, followPosition, 1f - Mathf.Exp(-input.followingSharpness * input.deltaTime));

            // Handle obstructions
            if (state == CameraState.aim) {
                RaycastHit closestHit = new RaycastHit();
                closestHit.distance = Mathf.Infinity;
                _obstructionCount = Physics.SphereCastNonAlloc(_currentFollowPosition, ObstructionCheckRadius, -Transform.forward, _obstructions, TargetDistance, LayerUtil.GetLayerMask(Layer.obj, Layer.def), QueryTriggerInteraction.Ignore);
                for (int i = 0; i < _obstructionCount; i++) {
                    bool isIgnored = false;
                    for (int j = 0; j < IgnoredColliders.Count; j++) {
                        if (IgnoredColliders[j] == _obstructions[i].collider) {
                            isIgnored = true;
                            break;
                        }
                    }

                    if (!isIgnored && _obstructions[i].distance < closestHit.distance && _obstructions[i].distance >= 0) {
                        closestHit = _obstructions[i];
                    }
                }
                if (closestHit.distance < Mathf.Infinity) {
                    _currentDistance = Mathf.Lerp(_currentDistance, closestHit.distance, 1 - Mathf.Exp(-ObstructionSharpness * input.deltaTime));
                } else {
                    // _currentDistance = Mathf.Lerp(_currentDistance, TargetDistance, 1 - Mathf.Exp(-input.distanceMovementSharpness * input.deltaTime));
                    _currentDistance = TargetDistance;
                }
            }

            // Find the smoothed camera orbit position
            Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);

            // Handle framing
            targetPosition += Transform.right * FollowPointFraming.x;
            targetPosition += Vector3.up * FollowPointFraming.y;

            // Apply position
            Transform.position = targetPosition;
            isometricRotation = Transform.rotation;
        }

        Debug.DrawRay(transform.position, 100f * transform.forward, Color.yellow);
    }

    public CursorData GetTargetData(Vector2 cursorPosition, InputMode inputMode) {
        if (inputMode == InputMode.aim || inputMode == InputMode.wallpressAim) {
            return AimToTarget(cursorPosition);
        } else {
            return CursorToTarget(cursorPosition);
        }
    }
    private CursorData CursorToTarget(Vector2 cursorPosition) {
        Vector3 cursorPoint = new Vector3(cursorPosition.x, cursorPosition.y, Camera.nearClipPlane);
        Ray projection = Camera.ScreenPointToRay(cursorPoint);
        Ray clickRay = new Ray(projection.origin, transform.forward);
        Plane playerPlane = new Plane(Vector3.up, GameManager.I.playerPosition);
        RaycastHit[] hits = Physics.RaycastAll(clickRay, 1000, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive, Layer.bulletOnly));
        Vector3 targetPoint = Vector3.zero;

        TagSystemData priorityData = null;
        bool prioritySet = false;
        Collider targetCollider = null;
        HashSet<InteractorTargetData> targetDatas = new HashSet<InteractorTargetData>();

        HashSet<AttackSurface> attackSurfaces = new HashSet<AttackSurface>();

        float closestAttackSurfaceDistance = float.MaxValue;
        AttackSurface currentAttackSurface = null;

        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            foreach (Interactive interactive in hit.collider.transform.root
            .GetComponentsInChildren<Interactive>()
            .Where(interactive => {
                if (interactive.dontRequireRaycast) {
                    return true;
                } else {
                    Vector3 origin = hit.collider.ClosestPoint(GameManager.I.playerPosition);
                    Vector3 displacement = (GameManager.I.playerPosition - origin);
                    Vector3 direction = displacement.normalized;

                    float distance = (displacement.magnitude * 0.95f) - 0.01f;

                    Ray testRay = new Ray(origin + (direction * 0.01f), direction);

                    // Debug.DrawRay(testRay.origin, testRay.direction * distance, Color.cyan);
                    return !Physics.Raycast(testRay, distance, LayerUtil.GetLayerMask(Layer.def));
                }
            })
            .Where(interactive => interactive.AllowInteraction())) {
                targetDatas.Add(new InteractorTargetData(interactive, hit.collider, GameManager.I.playerPosition));
            }

            hit.collider.transform.root.GetComponentsInChildren<AttackSurface>()
            .Where(attackSurface => {
                if (attackSurface.usePlayerPlane) {
                    Plane plane = new Plane(attackSurface.attackElementRoot.TransformDirection(attackSurface.playerPlaneNormal), attackSurface.attackElementRoot.position);
                    return plane.GetSide(GameManager.I.playerPosition);
                } else return true;
            })
            .Where(attackSurface => {
                Vector3 origin = hit.collider.ClosestPoint(GameManager.I.playerPosition);
                Vector3 displacement = (GameManager.I.playerPosition - origin);
                Vector3 direction = displacement.normalized;

                float distance = (displacement.magnitude * 0.95f) - 0.01f;

                Ray testRay = new Ray(origin + (direction * 0.01f), direction);

                // Debug.DrawRay(testRay.origin, testRay.direction * distance, Color.cyan);
                return !Physics.Raycast(testRay, distance, LayerUtil.GetLayerMask(Layer.def));
            })
            .ToList().ForEach(attackSurface => {
                float distanceToCursor = Vector3.Distance(hit.point, attackSurface.transform.position);
                if (distanceToCursor < closestAttackSurfaceDistance) {
                    currentAttackSurface = attackSurface;
                    closestAttackSurfaceDistance = distanceToCursor;
                }
            });

            TagSystemData data = Toolbox.GetTagData(hit.collider.gameObject);
            if (data != null && data.targetPriority > -1) {
                if (priorityData == null || data.targetPriority > priorityData.targetPriority) {
                    priorityData = data;
                    if (data.headTargetPoint != null &&
                         GameManager.I.gameData.playerState.PerkTargetLockOnHead() &&
                         GameManager.I.playerGunHandler.gunInstance != null &&
                         GameManager.I.playerGunHandler.gunInstance.template.type == GunType.pistol) {
                        targetPoint = data.headTargetPoint.position;
                    } else if (data.targetPoint != null) {
                        targetPoint = data.targetPoint.position;
                    } else {
                        targetPoint = hit.collider.bounds.center;
                    }
                    prioritySet = true;
                    targetCollider = hit.collider;
                }
            }

            if (!playerPlane.GetSide(hit.point)) {
                break;
            }
            if (hit.collider.CompareTag("interactBlocker")) {
                break;
            }
        }

        // Debug.DrawLine(transform.position, targetPoint, Color.yellow, 0.1f);
        InteractorTargetData interactorData = Interactive.ClosestTarget(targetDatas);
        if (prioritySet && !disableLockOn) {
            Vector2 pointPosition = Camera.WorldToScreenPoint(targetPoint);
            // TODO: set collider
            return new CursorData {
                type = CursorData.TargetType.objectLock,    // diff
                screenPosition = pointPosition,             // diff
                screenPositionNormalized = normalizeScreenPosition(pointPosition),
                screenPixelDimension = new Vector2(Camera.pixelWidth, Camera.pixelHeight),
                highlightableTargetData = interactorData,
                worldPosition = targetPoint,
                targetCollider = targetCollider,            // diff
                mousePosition = cursorPosition,
                attackSurface = currentAttackSurface
            };
        } else {
            // find the intersection between the ray and a plane whose normal is the player's up, and height is the gun height
            float distance = 0;

            Vector3 origin = Vector3.zero;
            Vector3 groundOrigin = Vector3.zero;
            if (GameManager.I.playerObject != null) {
                origin = GameManager.I.playerObject.transform.position + new Vector3(0f, 1.35f, 0f); // TODO: fix this hack!
                groundOrigin = GameManager.I.playerObject.transform.position;
            }
            Plane plane = new Plane(Vector3.up, origin);
            if (plane.Raycast(clickRay, out distance)) {
                targetPoint = clickRay.GetPoint(distance);
            }

            Plane groundPlane = new Plane(Vector3.up, groundOrigin);
            Vector3 groundPoint = Vector3.zero;
            if (groundPlane.Raycast(clickRay, out distance)) {
                groundPoint = clickRay.GetPoint(distance);
            }

            return new CursorData {
                type = CursorData.TargetType.direction,
                screenPosition = cursorPosition,
                screenPixelDimension = new Vector2(Camera.pixelWidth, Camera.pixelHeight),
                screenPositionNormalized = normalizeScreenPosition(cursorPosition),
                highlightableTargetData = interactorData,
                worldPosition = targetPoint,
                mousePosition = cursorPosition,
                groundPosition = groundPoint,
                attackSurface = currentAttackSurface
                // targetCollider = interactorData.collider
            };
        }
    }

    public Vector2 normalizeScreenPosition(Vector2 cursorPosition) {
        float horizontalPixels = (Camera.scaledPixelHeight * Camera.aspect);
        float verticalPixels = Camera.scaledPixelHeight;
        return new Vector2(cursorPosition.x / horizontalPixels, cursorPosition.y / verticalPixels);
    }


    private CursorData AimToTarget(Vector2 cursorPosition) {
        Vector3 cursorPoint = Vector3.zero;
        if (GameManager.I.inputMode == InputMode.aim) {
            cursorPoint = new Vector3(Camera.pixelWidth / 2f, Camera.pixelHeight / 2f, Camera.nearClipPlane);
        } else {
            cursorPoint = new Vector3(cursorPosition.x, cursorPosition.y, Camera.nearClipPlane);
        }
        Ray projection = Camera.ScreenPointToRay(cursorPoint);

        // TODO: nonalloc
        RaycastHit[] hits = Physics.RaycastAll(projection, 100, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive, Layer.bulletOnly), QueryTriggerInteraction.Ignore);
        Vector3 targetPoint = projection.GetPoint(100f);

        TagSystemData priorityData = null;
        bool targetSet = false;
        bool prioritySet = false;
        HashSet<InteractorTargetData> targetDatas = new HashSet<InteractorTargetData>();
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {

            if (hit.collider.transform.IsChildOf(GameManager.I.playerObject.transform.root)) {
                continue;
            }
            // Interactive interactive = hit.collider.GetComponent<Interactive>();
            foreach (Interactive interactive in hit.collider.transform.root.GetComponentsInChildren<Interactive>())
                if (interactive != null) {
                    targetDatas.Add(new InteractorTargetData(interactive, hit.collider, GameManager.I.playerPosition));
                }
            TagSystemData data = Toolbox.GetTagData(hit.collider.gameObject);
            if (data == null || data.targetPriority == -1) {
                if (!prioritySet && !targetSet) {
                    targetSet = true;
                }
                continue;
            }
            if (priorityData == null || data.targetPriority > priorityData.targetPriority) {
                priorityData = data;
                prioritySet = true;
            }
            targetPoint = hit.point;
        }
        // Debug.DrawLine(transform.position, targetPoint, Color.yellow, 0.1f);

        InteractorTargetData interactorData = Interactive.ClosestTarget(targetDatas);

        if (prioritySet) {
            Vector2 pointPosition = Camera.WorldToScreenPoint(targetPoint);
            return new CursorData {
                type = CursorData.TargetType.objectLock,
                // clickRay = clickRay,
                screenPosition = pointPosition,
                screenPixelDimension = new Vector2(Camera.pixelWidth, Camera.pixelHeight),
                screenPositionNormalized = normalizeScreenPosition(pointPosition),
                highlightableTargetData = interactorData,
                worldPosition = targetPoint
            };
        } else {
            // TODO: aim through transparent objects
            if (hits.Length > 0)
                targetPoint = hits
                    .OrderBy(h => h.distance)
                    .Where(hit => !hit.collider.transform.IsChildOf(GameManager.I.playerObject.transform.root))
                    .FirstOrDefault().point;
            return new CursorData {
                type = CursorData.TargetType.direction,
                screenPosition = cursorPosition,
                screenPixelDimension = new Vector2(Camera.pixelWidth, Camera.pixelHeight),
                screenPositionNormalized = normalizeScreenPosition(cursorPosition),
                highlightableTargetData = null,
                worldPosition = targetPoint
            };
        }

    }

    public static IEnumerator DoShake(float intensity, float lifetime) {
        float timer = 0;
        shakeJitter = intensity;
        while (timer < lifetime) {
            timer += Time.unscaledDeltaTime;
            shakeJitter = (float)PennerDoubleAnimation.CircEaseOut(timer, intensity, -intensity, lifetime);
            yield return null;
        }
        shakeJitter = 0f;
        yield return null;
    }

    // TODO: use a single coroutine here
    public static void Shake(float intensity, float lifetime) {
        GameManager.I.StartCoroutine(DoShake(intensity, lifetime));
    }

    // Turn on the bit using an OR operation:
    private void ShowLasers() {
        Camera.cullingMask |= 1 << LayerMask.NameToLayer("laser");
    }

    // Turn off the bit using an AND operation with the complement of the shifted int:
    private void HideLasers() {
        Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("laser"));
    }

    // Toggle the bit using a XOR operation:
    private void Toggle() {
        Camera.cullingMask ^= 1 << LayerMask.NameToLayer("laser");
    }
}