using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
public class NeoPlayer : MonoBehaviour {
    public NeoCharacterCamera OrbitCamera;
    public ClearSighter sighter;
    public Transform CameraFollowPoint;
    public NeoCharacterController Character;
    public GunHandler gunHandler;
    public JumpIndicatorController jumpIndicatorController;
    public Interactor interactor;
    public JumpIndicatorView jumpIndicatorView;
    public LegsAnimation legsAnimator;
    public GunAnimation torsoAnimator;

    [Header("Inputs")]
    public InputActionReference MoveAction;
    public InputActionReference FireAction;
    public InputActionReference CrouchAction;
    public InputActionReference RunAction;
    public InputActionReference JumpAction;
    public InputActionReference RotateCameraRight;
    public InputActionReference RotateCameraLeft;
    public InputActionReference reload;
    public InputActionReference gunHolster;
    public InputActionReference gunSecondary;
    public InputActionReference gunPrimary;
    public InputActionReference gunThird;
    public InputActionReference actionButton;
    public InputActionReference nextItem;
    public InputActionReference previousItem;
    public InputActionReference useItem;

    private Vector2 inputVector;
    private bool firePressedHeld;
    private bool firePressedThisFrame;
    private bool crouchHeld;
    private bool runHeld;
    private bool rotateCameraRightPressedThisFrame;
    private bool rotateCameraLeftPressedThisFrame;
    private bool jumpPressedThisFrame;
    private bool jumpReleasedThisFrame;
    private bool jumpHeld;
    private bool reloadPressedThisFrame;
    private int selectGunThisFrame;
    private bool actionButtonPressedThisFrame;
    private int incrementItemThisFrame;
    private bool useItemThisFrame;
    private PlayerCharacterInput _lastInput;
    private static List<SphereCollider> attractors;
    // public Action<AimIndicatorHandler> OnAimChanged;
    public void Awake() {
        // Move
        MoveAction.action.performed += ctx => inputVector = ctx.ReadValue<Vector2>();

        // Fire
        FireAction.action.performed += ctx => {
            firePressedThisFrame = ctx.ReadValueAsButton();
            firePressedHeld = ctx.ReadValueAsButton();
        };

        // Crouch
        CrouchAction.action.performed += ctx => {
            crouchHeld = ctx.ReadValueAsButton();
            // Debug.Log(crouchHeld);
        };

        // Run
        RunAction.action.performed += ctx => {
            runHeld = ctx.ReadValueAsButton();
        };

        // Jump
        JumpAction.action.performed += ctx => {
            jumpPressedThisFrame = ctx.ReadValueAsButton();
            jumpHeld = ctx.ReadValueAsButton();
        };

        // Camera rotation
        RotateCameraLeft.action.performed += ctx => {
            rotateCameraLeftPressedThisFrame = ctx.ReadValueAsButton();
        };
        RotateCameraRight.action.performed += ctx => {
            rotateCameraRightPressedThisFrame = ctx.ReadValueAsButton();
        };

        // Reload
        reload.action.performed += ctx => {
            reloadPressedThisFrame = ctx.ReadValueAsButton();
        };

        // Gun switch
        gunHolster.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                selectGunThisFrame = 0;
            }
        };
        gunSecondary.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                selectGunThisFrame = 2;
            }
        };
        gunPrimary.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                selectGunThisFrame = 1;
            }
        };
        gunThird.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                selectGunThisFrame = 3;
            }
        };
        actionButton.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                actionButtonPressedThisFrame = ctx.ReadValueAsButton();
            }
        };

        // Item
        nextItem.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                incrementItemThisFrame = 1;
            }
        };

        previousItem.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                incrementItemThisFrame = -1;
            }
        };

        useItem.action.performed += ctx => {
            useItemThisFrame = ctx.ReadValueAsButton();
        };

        // Button up
        FireAction.action.canceled += _ => firePressedHeld = false;
        CrouchAction.action.canceled += _ => {
            crouchHeld = false;
            // Debug.Log("uncrouch");
        };
        RunAction.action.canceled += _ => runHeld = false;
        MoveAction.action.canceled += _ => inputVector = Vector2.zero;
        JumpAction.action.canceled += _ => {
            jumpHeld = false;
            jumpReleasedThisFrame = true;
        };

        crouchHeld = false;
        selectGunThisFrame = -1;

        // TODO: move into on level load
        attractors = new List<SphereCollider>();
        foreach (GameObject attractor in GameObject.FindGameObjectsWithTag("cameraAttractor")) {
            SphereCollider collider = attractor.GetComponent<SphereCollider>();
            if (collider != null)
                attractors.Add(collider);
        }
    }
    private void Start() {
        // Tell camera to follow transform
        OrbitCamera.SetFollowTransform(CameraFollowPoint);

        // Ignore the character's collider(s) for camera obstruction checks
        OrbitCamera.IgnoredColliders.Clear();
        OrbitCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());


    }

    private void Update() {
        HandleCharacterInput();
    }

    private void LateUpdate() {
        HandleCameraInput();

        // update view
        Vector2 camDir = new Vector2(OrbitCamera.Transform.forward.x, OrbitCamera.Transform.forward.z);
        Vector2 playerDir = new Vector2(Character.direction.x, Character.direction.z);
        float angle = Vector2.SignedAngle(camDir, playerDir);
        if (GameManager.I.showDebugRays)
            Debug.DrawRay(OrbitCamera.Transform.position, OrbitCamera.Transform.forward, Color.blue, 1f);

        GunType gunType = GunType.unarmed;
        if (torsoAnimator.gunHandler.HasGun()) {
            gunType = torsoAnimator.gunHandler.gunInstance.baseGun.type;
        }

        AnimationInput animationInput = new AnimationInput {
            orientation = Toolbox.DirectionFromAngle(angle),
            isMoving = Character.Motor.Velocity.magnitude > 0.1 && (Character.Motor.GroundingStatus.IsStableOnGround || Character.state == CharacterState.climbing),
            isCrouching = Character.isCrouching,
            // isCrouching = true,
            isRunning = Character.isRunning,
            isJumping = Character.state == CharacterState.superJump,
            isClimbing = Character.state == CharacterState.climbing,
            wallPressTimer = Character.wallPressTimer,
            state = Character.state,
            playerInputs = _lastInput,
            gunType = gunType
        };

        legsAnimator.UpdateView(animationInput);
        torsoAnimator.UpdateView(animationInput);
        jumpIndicatorView.UpdateView(animationInput);
    }

    private void HandleCameraInput() {
        CameraInput.RotateInput rotation = CameraInput.RotateInput.none;
        if (rotateCameraRightPressedThisFrame) {
            rotation = CameraInput.RotateInput.right;
        } else if (rotateCameraLeftPressedThisFrame) {
            rotation = CameraInput.RotateInput.left;
        }

        CameraState state = CameraState.normal;
        SphereCollider currentAttractor = null;
        if (Character.state == CharacterState.wallPress) {
            state = CameraState.wallPress;
        } else {
            // check / update Attractor
            foreach (SphereCollider attractor in attractors) {
                if (attractor.bounds.Contains(Character.transform.position)) {
                    currentAttractor = attractor;
                    state = CameraState.attractor;
                    break;
                }
            }
        }

        Vector2 lastWallInput = Character.lastWallInput;
        if (!Character.wallPressRatchet) {
            lastWallInput = Vector2.zero;
        }

        CameraInput input = new CameraInput {
            deltaTime = Time.deltaTime,
            rotation = rotation,
            state = state,
            wallNormal = Character.wallNormal,
            lastWallInput = lastWallInput,
            crouchHeld = crouchHeld,
            attractor = currentAttractor
        };

        // Apply inputs to the camera
        OrbitCamera.UpdateWithInput(input);

        rotateCameraLeftPressedThisFrame = false;
        rotateCameraRightPressedThisFrame = false;
    }

    public TargetData CursorToTarget(Vector2 cursorPosition) {
        Vector3 gunPoint = gunHandler.gunPosition();
        Plane plane = new Plane(Vector3.up, gunPoint);

        Vector3 cursorPoint = new Vector3(cursorPosition.x, cursorPosition.y, OrbitCamera.Camera.nearClipPlane);
        Ray projection = OrbitCamera.Camera.ScreenPointToRay(cursorPoint);
        Vector3 direction = OrbitCamera.transform.forward;
        if (GameManager.I.showDebugRays)
            Debug.DrawRay(projection.origin, direction * 100, Color.magenta, 0.1f);
        Ray clickRay = new Ray(projection.origin, direction);

        // 1. determine if the click ray is hovering over a targetable object
        //      if so, determine the priority targetable and highlight it
        //      this can aim up or down as necessary
        // 2. if not, shoot in the direction indicated by the mouse
        //      this will be in the player's gun's height plane.


        RaycastHit[] hits = Physics.RaycastAll(clickRay, 100, LayerUtil.GetMask(Layer.obj, Layer.interactive));
        TagSystemData priorityData = null;
        RaycastHit priorityHit = new RaycastHit();
        bool prioritySet = false;
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            TagSystemData data = Toolbox.GetTagData(hit.collider.gameObject);
            if (data == null)
                continue;
            if (data.targetPriority == -1)
                continue;
            if (priorityData == null || data.targetPriority > priorityData.targetPriority) {
                priorityData = data;
                priorityHit = hit;
                prioritySet = true;
            }
        }
        if (prioritySet) {
            return new TargetData {
                type = TargetData.TargetType.objectLock,
                position = priorityHit.collider.bounds.center,
                screenPosition = OrbitCamera.Camera.WorldToScreenPoint(priorityHit.collider.bounds.center)
            };
        }


        // find the intersection between the ray and a plane whose normal is the player's up, and height is the gun height
        float distance = 0;
        Vector3 targetPoint = Vector3.zero;
        if (plane.Raycast(clickRay, out distance)) {
            targetPoint = clickRay.GetPoint(distance);
        }
        return new TargetData {
            type = TargetData.TargetType.direction,
            position = targetPoint,
            screenPosition = cursorPosition
        };
    }
    private void HandleCharacterInput() {

        TargetData targetData = CursorToTarget(Mouse.current.position.ReadValue());

        PlayerCharacterInput characterInputs = new PlayerCharacterInput() {
            state = Character.state,
            MoveAxisForward = inputVector.y,
            MoveAxisRight = inputVector.x,
            CameraRotation = OrbitCamera.isometricRotation,
            JumpDown = jumpPressedThisFrame,
            jumpHeld = jumpHeld,
            jumpReleased = jumpReleasedThisFrame,
            CrouchDown = crouchHeld,
            runDown = runHeld,
            Fire = new PlayerCharacterInput.FireInputs() {
                FirePressed = firePressedThisFrame,
                FireHeld = firePressedHeld,
                targetData = targetData
            },
            reload = reloadPressedThisFrame,
            switchToGun = selectGunThisFrame,
            actionButtonPressed = actionButtonPressedThisFrame,
            incrementItem = incrementItemThisFrame,
            useItem = useItemThisFrame,
        };
        // Apply inputs to character
        Character.SetInputs(ref characterInputs);

        // apply inputs to jump indicator
        jumpIndicatorController.SetInputs(ref characterInputs, Character);

        // apply inputs to interactor
        interactor.SetInputs(ref characterInputs);

        firePressedThisFrame = false;
        jumpPressedThisFrame = false;
        reloadPressedThisFrame = false;
        jumpReleasedThisFrame = false;
        selectGunThisFrame = -1;
        actionButtonPressedThisFrame = false;
        incrementItemThisFrame = 0;
        useItemThisFrame = false;

        _lastInput = characterInputs;
    }

}
