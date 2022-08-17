using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public class InputController : MonoBehaviour {
    // TODO: input mode could belong to me?

    public CharacterCamera OrbitCamera;

    // have to do it this way because unity inspector doesn't know how to expose a list of interfaces
    public List<GameObject> inputTargets;
    public List<IInputReceiver> inputReceivers = new List<IInputReceiver>();

    [Header("Inputs")]
    public InputActionReference MoveAction;
    public InputActionReference FireAction;
    public InputActionReference AimAction;
    public InputActionReference CrouchAction;
    public InputActionReference RunAction;
    public InputActionReference JumpAction;
    public InputActionReference RotateCameraRight;
    public InputActionReference RotateCameraLeft;
    public InputActionReference ZoomCamera;
    public InputActionReference reload;
    public InputActionReference gunHolster;
    public InputActionReference gunSecondary;
    public InputActionReference gunPrimary;
    public InputActionReference gunThird;
    public InputActionReference actionButton;
    public InputActionReference nextItem;
    public InputActionReference previousItem;
    public InputActionReference useItem;
    public InputActionReference nextOverlay;
    public InputActionReference previousOverlay;
    [Header("Debug")]
    public InputActionReference DebugBreakAction;


    private Vector2 inputVector;
    private bool firePressedHeld;
    private bool firePressedThisFrame;
    private bool aimPressedThisFrame;
    private bool crouchHeld;
    private bool runHeld;
    private bool rotateCameraRightPressedThisFrame;
    private bool rotateCameraLeftPressedThisFrame;
    private Vector2 zoomInput;
    private bool jumpPressedThisFrame;
    private bool jumpReleasedThisFrame;
    private bool jumpHeld;
    private bool reloadPressedThisFrame;
    private int selectGunThisFrame;
    private bool actionButtonPressedThisFrame;
    private int incrementItemThisFrame;
    private int incrementOverlayThisFrame;
    private bool useItemThisFrame;
    public static Vector2 mouseCursorOffset;

    public void HandleMoveAction(InputAction.CallbackContext ctx) {
        inputVector = ctx.ReadValue<Vector2>();
    }
    public void HandleFireAction(InputAction.CallbackContext ctx) {
        firePressedThisFrame = ctx.ReadValueAsButton();
        firePressedHeld = ctx.ReadValueAsButton();
    }
    public void HandleAimAction(InputAction.CallbackContext ctx) {
        aimPressedThisFrame = ctx.ReadValueAsButton();
    }
    public void HandleCrouchAction(InputAction.CallbackContext ctx) {
        crouchHeld = ctx.ReadValueAsButton();
    }
    public void HandleRunAction(InputAction.CallbackContext ctx) {
        runHeld = ctx.ReadValueAsButton();
    }
    public void HandleJumpAction(InputAction.CallbackContext ctx) {
        jumpPressedThisFrame = ctx.ReadValueAsButton();
        jumpHeld = ctx.ReadValueAsButton();
    }
    public void HandleRotateCameraLeftAction(InputAction.CallbackContext ctx) {
        rotateCameraLeftPressedThisFrame = ctx.ReadValueAsButton();
    }
    public void HandleRotateCameraRightAction(InputAction.CallbackContext ctx) {
        rotateCameraRightPressedThisFrame = ctx.ReadValueAsButton();
    }
    public void HandleZoomCameraAction(InputAction.CallbackContext ctx) {
        zoomInput = ctx.ReadValue<Vector2>();
    }
    public void HandleReloadAction(InputAction.CallbackContext ctx) {
        reloadPressedThisFrame = ctx.ReadValueAsButton();
    }
    public void HandleDebugBreakAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            Debug.Break();
        }
    }
    public void HandleGunHolsterAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            selectGunThisFrame = -1;
        }
    }
    public void HandleGunPrimaryAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            selectGunThisFrame = 1;
        }
    }
    public void HandleGunSecondaryAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            selectGunThisFrame = 2;
        }
    }
    public void HandleGunThirdAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            selectGunThisFrame = 3;
        }
    }
    public void HandleActionButtonAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            actionButtonPressedThisFrame = ctx.ReadValueAsButton();
        }
    }
    public void HandleNextItemAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            incrementItemThisFrame = 1;
        }
    }
    public void HandlePrevItemAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            incrementItemThisFrame = -1;
        }
    }
    public void HandleNextOverlayAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            incrementOverlayThisFrame = 1;
        }
    }
    public void HandlePrevOverlayAction(InputAction.CallbackContext ctx) {
        if (ctx.ReadValueAsButton()) {
            incrementOverlayThisFrame = -1;
        }
    }
    public void HandleUseItemAction(InputAction.CallbackContext ctx) {
        useItemThisFrame = ctx.ReadValueAsButton();
    }
    public void HandleFireActionCanceled(InputAction.CallbackContext ctx) {
        firePressedHeld = false;
    }
    public void HandleCrouchActionCanceled(InputAction.CallbackContext ctx) {
        crouchHeld = false;
    }
    public void HandleRunActionCanceled(InputAction.CallbackContext ctx) {
        runHeld = false;
    }
    public void HandleMoveActionCanceled(InputAction.CallbackContext ctx) {
        inputVector = Vector2.zero;
    }
    public void HandleJumpActionCanceled(InputAction.CallbackContext ctx) {
        jumpHeld = false;
        jumpReleasedThisFrame = true;
    }
    public void Awake() {
        RegisterCallbacks();
    }
    void OnDestroy() {
        DeregisterCallbacks();
    }

    void RegisterCallbacks() {
        // Move
        MoveAction.action.performed += HandleMoveAction;
        // Fire
        FireAction.action.performed += HandleFireAction;
        // Aim
        AimAction.action.performed += HandleAimAction;
        // Crouch
        CrouchAction.action.performed += HandleCrouchAction;
        // Run
        RunAction.action.performed += HandleRunAction;
        // Jump
        JumpAction.action.performed += HandleJumpAction;
        // Camera rotation
        RotateCameraLeft.action.performed += HandleRotateCameraLeftAction;
        RotateCameraRight.action.performed += HandleRotateCameraRightAction;
        ZoomCamera.action.performed += HandleZoomCameraAction;
        // Reload
        reload.action.performed += HandleReloadAction;
        DebugBreakAction.action.performed += HandleDebugBreakAction;
        // Gun switch
        gunHolster.action.performed += HandleGunHolsterAction;
        gunPrimary.action.performed += HandleGunPrimaryAction;
        gunSecondary.action.performed += HandleGunSecondaryAction;
        gunThird.action.performed += HandleGunThirdAction;
        // action buttom
        actionButton.action.performed += HandleActionButtonAction;
        // Item
        nextItem.action.performed += HandleNextItemAction;
        previousItem.action.performed += HandlePrevItemAction;
        // Overlay
        nextOverlay.action.performed += HandleNextOverlayAction;
        previousOverlay.action.performed += HandlePrevOverlayAction;
        useItem.action.performed += HandleUseItemAction;
        // Button up
        FireAction.action.canceled += HandleFireActionCanceled;
        CrouchAction.action.canceled += HandleCrouchActionCanceled;
        RunAction.action.canceled += HandleRunActionCanceled;
        MoveAction.action.canceled += HandleMoveActionCanceled;
        JumpAction.action.canceled += HandleJumpActionCanceled;
    }
    void DeregisterCallbacks() {
        // Move
        MoveAction.action.performed -= HandleMoveAction;
        // Fire
        FireAction.action.performed -= HandleFireAction;
        // Aim
        AimAction.action.performed -= HandleAimAction;
        // Crouch
        CrouchAction.action.performed -= HandleCrouchAction;
        // Run
        RunAction.action.performed -= HandleRunAction;
        // Jump
        JumpAction.action.performed -= HandleJumpAction;
        // Camera rotation
        RotateCameraLeft.action.performed -= HandleRotateCameraLeftAction;
        RotateCameraRight.action.performed -= HandleRotateCameraRightAction;
        ZoomCamera.action.performed -= HandleZoomCameraAction;
        // Reload
        reload.action.performed -= HandleReloadAction;
        DebugBreakAction.action.performed -= HandleDebugBreakAction;
        // Gun switch
        gunHolster.action.performed -= HandleGunHolsterAction;
        gunPrimary.action.performed -= HandleGunPrimaryAction;
        gunSecondary.action.performed -= HandleGunSecondaryAction;
        gunThird.action.performed -= HandleGunThirdAction;
        // action buttom
        actionButton.action.performed -= HandleActionButtonAction;
        // Item
        nextItem.action.performed -= HandleNextItemAction;
        previousItem.action.performed -= HandlePrevItemAction;
        // Overlay
        nextOverlay.action.performed -= HandleNextOverlayAction;
        previousOverlay.action.performed -= HandlePrevOverlayAction;
        useItem.action.performed -= HandleUseItemAction;
        // Button up
        FireAction.action.canceled -= HandleFireActionCanceled;
        CrouchAction.action.canceled -= HandleCrouchActionCanceled;
        RunAction.action.canceled -= HandleRunActionCanceled;
        MoveAction.action.canceled -= HandleMoveActionCanceled;
        JumpAction.action.canceled -= HandleJumpActionCanceled;
    }

    private void Update() {
        HandleCharacterInput();
    }

    void Start() {
        inputReceivers = inputTargets
            .Select(g => g.GetComponent<IInputReceiver>())
            .Where(component => component != null)
            .ToList();
    }

    private void HandleCharacterInput() {
        Vector2 cursorPosition = Mouse.current.position.ReadValue();

        if (aimPressedThisFrame && GameManager.I.inputMode != InputMode.aim) {
            float horizontalPixels = (OrbitCamera.Camera.scaledPixelHeight * OrbitCamera.Camera.aspect);
            float verticalPixels = OrbitCamera.Camera.scaledPixelHeight;
            Vector2 warpedPosition = new Vector2(horizontalPixels * 0.5f, verticalPixels * 0.5f);
            mouseCursorOffset = warpedPosition - cursorPosition;
            // InputSystem.QueueDeltaStateEvent(Mouse.current.position, warpedPosition);
            // Mouse.current.WarpCursorPosition(warpedPosition);
            // InputState.Change(Mouse.current.position, warpedPosition);
        }
        if (GameManager.I.inputMode == InputMode.aim) {
            cursorPosition += mouseCursorOffset;
        }

        CursorData targetData = OrbitCamera.GetTargetData(cursorPosition);
        Vector3 torque = Vector3.zero;
        if (GameManager.I.inputMode == InputMode.aim) {
            if (targetData.screenPositionNormalized.x > 0.9) {
                torque = new Vector3(0f, -0.02f, 0f);
            } else if (targetData.screenPositionNormalized.x < 0.1) {
                torque = new Vector3(0f, 0.02f, 0f);
            }
        }

        if (aimPressedThisFrame) {
            if (GameManager.I.inputMode != InputMode.aim) {
                GameManager.I.TransitionToInputMode(InputMode.aim);
            } else {
                GameManager.I.TransitionToInputMode(InputMode.gun);
            }
        }

        foreach (IInputReceiver i in inputReceivers) {
            Vector3 directionToCursor = (targetData.worldPosition - i.transform.position).normalized;
            PlayerInput characterInputs = new PlayerInput() {
                inputMode = GameManager.I.inputMode,
                MoveAxisForward = inputVector.y,
                MoveAxisRight = inputVector.x,
                CameraRotation = OrbitCamera.isometricRotation,
                JumpDown = jumpPressedThisFrame,
                jumpHeld = jumpHeld,
                jumpReleased = jumpReleasedThisFrame,
                CrouchDown = crouchHeld,
                runDown = runHeld,
                Fire = new PlayerInput.FireInputs() {
                    FirePressed = firePressedThisFrame,
                    FireHeld = firePressedHeld,
                    cursorData = targetData,
                    AimPressed = aimPressedThisFrame
                },
                reload = reloadPressedThisFrame,
                selectgun = selectGunThisFrame,
                actionButtonPressed = actionButtonPressedThisFrame,
                incrementItem = incrementItemThisFrame,
                useItem = useItemThisFrame,
                incrementOverlay = incrementOverlayThisFrame,
                rotateCameraRightPressedThisFrame = rotateCameraRightPressedThisFrame,
                rotateCameraLeftPressedThisFrame = rotateCameraLeftPressedThisFrame,
                torque = torque,
                lookAtDirection = directionToCursor,
                zoomInput = zoomInput
            };
            i.SetInputs(characterInputs);
        }

        if (incrementOverlayThisFrame != 0) {
            GameManager.I.IncrementOverlay(incrementOverlayThisFrame);
        }

        firePressedThisFrame = false;
        aimPressedThisFrame = false;
        jumpPressedThisFrame = false;
        reloadPressedThisFrame = false;
        jumpReleasedThisFrame = false;
        selectGunThisFrame = 0;
        actionButtonPressedThisFrame = false;
        incrementItemThisFrame = 0;
        incrementOverlayThisFrame = 0;
        useItemThisFrame = false;
        rotateCameraLeftPressedThisFrame = false;
        rotateCameraRightPressedThisFrame = false;
        zoomInput = Vector2.zero;
    }

}
