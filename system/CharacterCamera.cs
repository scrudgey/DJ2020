using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

// TODO: eliminate this enum
// a single parameter lerps between states, and there is one transitional state. ...?
public enum CameraState { normal, wallPress, attractor, aim }

public struct CameraInput {
    public enum RotateInput { none, left, right }
    public float deltaTime;
    public Vector3 wallNormal;
    public Vector2 lastWallInput;
    public bool crouchHeld;
    public Vector3 playerPosition;
    public CharacterState state;
    // public Vector3 cursorPosition;
    public TargetData2 targetData;
    public Vector3 playerDirection;
}
public class CharacterCamera : IBinder<CharacterController>, IInputReceiver {
    private CameraState _state;
    public CameraState state {
        get { return _state; }
    }
    public static Quaternion rotationOffset;
    public PostProcessVolume volume;
    public PostProcessProfile isometricProfile;
    public PostProcessProfile wallPressProfile;
    public PostProcessProfile aimProfile;
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
    public Vector3 lastTargetPosition;

    private static List<CameraAttractorZone> attractors = new List<CameraAttractorZone>();
    CameraInput.RotateInput currentRotationInput;
    CameraAttractorZone currentAttractor = null;
    private static float shakeJitter = 0f;
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
        Camera.clearFlags = CameraClearFlags.SolidColor;
        switch (state) {
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
        if (FollowTransform == null)
            return;
        CameraState camState = CameraState.normal;
        currentAttractor = null;

        if (GameManager.I.inputMode == InputMode.aim) {
            camState = CameraState.aim;
        } else if (input.state == CharacterState.wallPress) {
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
            case CameraState.aim:
                RenderSettings.skybox = wallPressSkybox;
                break;
        }
        transitionTime = Mathf.Clamp(transitionTime, 0, 1f);

        if (state == CameraState.attractor) {
            ApplyTargetParameters(AttractorParameters(input));
            volume.profile = isometricProfile;
        } else if (state == CameraState.normal) {
            ApplyTargetParameters(NormalParameters(input));
            volume.profile = isometricProfile;
        } else if (state == CameraState.wallPress) {
            ApplyTargetParameters(WallPressParameters(input));
            volume.profile = wallPressProfile;
        } else if (state == CameraState.aim) {
            ApplyTargetParameters(AimParameters(input));
            volume.profile = aimProfile;
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
        public CharacterState state; // TODO: remove this
    }
    public CameraTargetParameters NormalParameters(CameraInput input) {
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

        if (transitionTime >= 0.1f && input.targetData != null) {
            Vector3 screenOffset = input.targetData.screenPositionNormalized - new Vector2(0.5f, 0.5f);
            Vector3 worldOffset = planarRot * new Vector3(screenOffset.x, 0f, screenOffset.y);
            // TODO: configurable scale, possibly involve aspect ratio
            targetPosition = FollowTransform.position + worldOffset;
            lastTargetPosition = targetPosition;
        }
        return new CameraTargetParameters() {
            fieldOfView = 70f,
            orthographic = true,
            rotation = planarRot * verticalRot,
            deltaTime = input.deltaTime,
            targetDistance = 20,
            targetPosition = targetPosition,
            orthographicSize = 6, // 8 TODO: set by attractor and/or level
            // orthographicSize = 3, // 8 TODO: set by attractor and/or level
            distanceMovementSharpness = currentDistanceMovementSharpness,
            followingSharpness = currentFollowingSharpness,
            distanceMovementSpeed = currentDistanceMovementSpeed,
            state = input.state
        };
    }
    public CameraTargetParameters AttractorParameters(CameraInput input) {
        CameraTargetParameters parameters = NormalParameters(input);
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
    public CameraTargetParameters WallPressParameters(CameraInput input) {
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
    public CameraTargetParameters AimParameters(CameraInput input) {
        Vector3 distOffset = input.playerDirection * -0.5f * TargetDistance;
        Vector3 heightOffset = new Vector3(0f, 1.2f, 0);
        if (input.crouchHeld) {
            heightOffset -= new Vector3(0, 0.5f, 0);
        }

        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        float horizontalPixels = (Camera.scaledPixelHeight * Camera.aspect);
        float parity = cursorPosition.x > horizontalPixels / 2 ? 1f : -1f;
        Vector3 horizontalOffset = Vector3.Cross(Vector3.up, input.playerDirection) * 0.5f * parity;

        Vector3 camDirection = input.playerDirection;
        camDirection = Vector3.Cross(Vector3.up, Vector3.Cross(camDirection, Vector3.up));
        // camDirection -= new Vector3(0, 0.05f, 0);
        Quaternion planarRot = Quaternion.LookRotation(camDirection, Vector3.up);

        return new CameraTargetParameters() {
            fieldOfView = 45f,
            orthographic = false,
            rotation = planarRot,
            deltaTime = input.deltaTime,
            targetDistance = 1.5f,
            targetPosition = FollowTransform.position + distOffset + heightOffset + horizontalOffset,
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

        if (input.state != CharacterState.wallPress && GameManager.I.inputMode != InputMode.aim && transitionTime >= 1) {
            // TODO: move this logic?
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

        // Find the smoothed follow position
        Vector3 followPosition = input.targetPosition + (UnityEngine.Random.insideUnitSphere * shakeJitter);
        _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, followPosition, 1f - Mathf.Exp(-input.followingSharpness * input.deltaTime));

        // Find the smoothed camera orbit position
        Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);

        // Handle framing
        targetPosition += Transform.right * FollowPointFraming.x;
        targetPosition += Vector3.up * FollowPointFraming.y;

        // Apply position
        Transform.position = targetPosition;
        isometricRotation = Transform.rotation;
    }
    public TargetData2 GetTargetData() {
        if (GameManager.I.inputMode == InputMode.aim) {
            return AimToTarget();
        } else {
            return CursorToTarget();
        }
    }
    private TargetData2 CursorToTarget() {
        Vector2 cursorPosition = Mouse.current.position.ReadValue();

        Vector3 cursorPoint = new Vector3(cursorPosition.x, cursorPosition.y, Camera.nearClipPlane);
        Ray projection = Camera.ScreenPointToRay(cursorPoint);
        Vector3 direction = transform.forward;
        if (GameManager.I.showDebugRays)
            Debug.DrawRay(projection.origin, direction * 100, Color.magenta);
        Ray clickRay = new Ray(projection.origin, direction);

        // 1. determine if the click ray is hovering over a targetable object
        //      if so, determine the priority targetable and highlight it
        //      this can aim up or down as necessary
        // 2. if not, shoot in the direction indicated by the mouse
        //      this will be in the player's gun's height plane.

        RaycastHit[] hits = Physics.RaycastAll(clickRay, 100, LayerUtil.GetMask(Layer.obj, Layer.interactive));
        Vector3 targetPoint = Vector3.zero;

        TagSystemData priorityData = null;
        bool prioritySet = false;
        HashSet<HighlightableTargetData> targetDatas = new HashSet<HighlightableTargetData>();
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            Highlightable interactive = hit.collider.GetComponent<Highlightable>();
            if (interactive != null) {
                targetDatas.Add(new HighlightableTargetData(interactive, hit.collider));
            }
            TagSystemData data = Toolbox.GetTagData(hit.collider.gameObject);
            if (data == null)
                continue;
            if (data.targetPriority == -1)
                continue;
            if (priorityData == null || data.targetPriority > priorityData.targetPriority) {
                priorityData = data;
                if (data.targetPoint != null) {
                    targetPoint = data.targetPoint.position;
                } else {
                    targetPoint = hit.collider.bounds.center;
                }
                prioritySet = true;
            }
        }
        HighlightableTargetData interactorData = Interactive.TopTarget(targetDatas);

        if (prioritySet) {
            Vector2 pointPosition = Camera.WorldToScreenPoint(targetPoint);
            return new TargetData2 {
                type = TargetData2.TargetType.objectLock,
                // clickRay = clickRay,
                screenPosition = pointPosition,
                screenPositionNormalized = normalizeScreenPosition(pointPosition),
                highlightableTargetData = interactorData,
                position = targetPoint
            };
        } else {
            // find the intersection between the ray and a plane whose normal is the player's up, and height is the gun height
            float distance = 0;
            Vector3 origin = GameManager.I.playerObject.transform.position + new Vector3(0f, 1f, 0f); // TODO: fix this hack!
            Plane plane = new Plane(Vector3.up, origin);
            if (plane.Raycast(clickRay, out distance)) {
                targetPoint = clickRay.GetPoint(distance);
            }
            return new TargetData2 {
                type = TargetData2.TargetType.direction,
                screenPosition = cursorPosition,
                screenPositionNormalized = normalizeScreenPosition(cursorPosition),
                highlightableTargetData = interactorData,
                // clickRay = clickRay,
                position = targetPoint
            };
        }
    }

    Vector2 normalizeScreenPosition(Vector2 cursorPosition) {
        float horizontalPixels = (Camera.scaledPixelHeight * Camera.aspect);
        float verticalPixels = Camera.scaledPixelHeight;
        return new Vector2(cursorPosition.x / horizontalPixels, cursorPosition.y / verticalPixels);
    }


    private TargetData2 AimToTarget() {
        Vector2 cursorPosition = Mouse.current.position.ReadValue();

        Vector3 cursorPoint = new Vector3(cursorPosition.x, cursorPosition.y, Camera.nearClipPlane);
        Ray projection = Camera.ScreenPointToRay(cursorPoint);
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.RaycastAll(projection, 100, LayerUtil.GetMask(Layer.def, Layer.obj, Layer.interactive));
        Vector3 targetPoint = projection.GetPoint(100f);

        TagSystemData priorityData = null;
        bool prioritySet = false;
        HashSet<HighlightableTargetData> targetDatas = new HashSet<HighlightableTargetData>();
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            Highlightable interactive = hit.collider.GetComponent<Highlightable>();
            if (interactive != null) {
                targetDatas.Add(new HighlightableTargetData(interactive, hit.collider));
            }
            TagSystemData data = Toolbox.GetTagData(hit.collider.gameObject);
            if (data == null)
                continue;
            if (data.targetPriority == -1)
                continue;
            if (priorityData == null || data.targetPriority > priorityData.targetPriority) {
                priorityData = data;
                if (data.targetPoint != null) {
                    targetPoint = data.targetPoint.position;
                } else {
                    targetPoint = hit.collider.bounds.center;
                }
                prioritySet = true;
            }
        }
        HighlightableTargetData interactorData = Interactive.TopTarget(targetDatas);

        if (prioritySet) {
            Vector2 pointPosition = Camera.WorldToScreenPoint(targetPoint);
            return new TargetData2 {
                type = TargetData2.TargetType.objectLock,
                // clickRay = clickRay,
                screenPosition = pointPosition,
                screenPositionNormalized = normalizeScreenPosition(pointPosition),
                highlightableTargetData = interactorData,
                position = targetPoint
            };
        } else {
            // TODO: aim through transparent objects
            if (hits.Length > 0)
                targetPoint = hits.OrderBy(h => h.distance).FirstOrDefault().point;
            return new TargetData2 {
                type = TargetData2.TargetType.direction,
                screenPosition = cursorPosition,
                screenPositionNormalized = normalizeScreenPosition(cursorPosition),
                highlightableTargetData = null,
                position = targetPoint
            };
        }

    }

    public static IEnumerator DoShake(float intensity, float lifetime) {
        float timer = 0;
        shakeJitter = intensity;
        while (timer < lifetime) {
            timer += Time.deltaTime;
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

    private void OnDrawGizmos() {
        string customName = "Relic\\MaskedSpider.png";
        Gizmos.DrawIcon(lastTargetPosition, customName, true);
    }
}