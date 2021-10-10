using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.InputSystem;
using System;

public class NeoPlayer : MonoBehaviour {
    public NeoCharacterCamera OrbitCamera;
    public ClearSighter sighter;
    public Transform CameraFollowPoint;
    public NeoCharacterController Character;
    public JumpIndicatorController jumpIndicatorController;
    public JumpIndicatorView jumpIndicatorView;
    public HumanoidView legsAnimator;
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
            Debug.Log(crouchHeld);
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
            Debug.Log("uncrouch");
        };
        RunAction.action.canceled += _ => runHeld = false;
        MoveAction.action.canceled += _ => inputVector = Vector2.zero;
        JumpAction.action.canceled += _ => {
            jumpHeld = false;
            jumpReleasedThisFrame = true;
        };

        crouchHeld = false;
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
        Vector2 camDir = new Vector2(OrbitCamera.PlanarDirection.x, OrbitCamera.PlanarDirection.z);
        Vector2 playerDir = new Vector2(Character.direction.x, Character.direction.z);
        float angle = Vector2.SignedAngle(camDir, playerDir);

        GunType gunType = GunType.unarmed;
        if (torsoAnimator.gunHandler.gunInstance != null && torsoAnimator.gunHandler.gunInstance.baseGun != null) {
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
        if (Character.state == CharacterState.wallPress) {
            state = CameraState.wallPress;
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
            crouchHeld = crouchHeld
        };

        // Apply inputs to the camera
        OrbitCamera.UpdateWithInput(input);

        rotateCameraLeftPressedThisFrame = false;
        rotateCameraRightPressedThisFrame = false;
    }

    private void HandleCharacterInput() {

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
                cursorPosition = Mouse.current.position.ReadValue()
            },
            reload = reloadPressedThisFrame,
            switchToGun = selectGunThisFrame,
            climbLadder = actionButtonPressedThisFrame,
            incrementItem = incrementItemThisFrame,
            useItem = useItemThisFrame,
        };
        // Apply inputs to character
        Character.SetInputs(ref characterInputs);

        // apply inputs to jump indicator
        jumpIndicatorController.SetInputs(ref characterInputs, Character);

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
