using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public class InputController : Singleton<InputController> {
    public CharacterCamera OrbitCamera;
    [Header("mouse")]

    public Vector2 sensitivity = new Vector2(2f, 2f);
    public Vector2 smoothing = new Vector2(2f, 2f);
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
    public InputActionReference revealWeaponWheel;
    [Header("Debug")]
    public InputActionReference DebugBreakAction;


    private Vector2 inputVector;
    private bool firePressedHeld;
    private bool rightClickHeld;
    private bool mouseDown;
    private bool mouseClickedThisFrame;
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
    private bool revealWeaponWheelHeld;
    Vector2 _smoothMouse;
    Vector2 previousMouseDelta;
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
    public void HandleRevealWeaponWheel(InputAction.CallbackContext ctx) {
        revealWeaponWheelHeld = ctx.ReadValueAsButton();
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
            rightClickHeld = ctx.ReadValueAsButton();
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
        mouseDown = false;
    }
    public void HandleRightClickActionCanceled(InputAction.CallbackContext ctx) {
        rightClickHeld = false;
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
    public void HandleWeaponWheelCanceled(InputAction.CallbackContext ctx) {
        revealWeaponWheelHeld = false;
    }
    public void Start() {
        RegisterCallbacks();
    }
    override public void OnDestroy() {
        DeregisterCallbacks();
        base.OnDestroy();
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
        actionButton.action.canceled += HandleRightClickActionCanceled;
        CrouchAction.action.canceled += HandleCrouchActionCanceled;
        RunAction.action.canceled += HandleRunActionCanceled;
        MoveAction.action.canceled += HandleMoveActionCanceled;
        JumpAction.action.canceled += HandleJumpActionCanceled;

        // Weapon wheel
        revealWeaponWheel.action.performed += HandleRevealWeaponWheel;
        revealWeaponWheel.action.canceled += HandleWeaponWheelCanceled;
    }
    void DeregisterCallbacks() {
        // Escape
        // escapeAction.action.performed -= HandleEscapeAction;
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
        actionButton.action.canceled -= HandleRightClickActionCanceled;
        CrouchAction.action.canceled -= HandleCrouchActionCanceled;
        RunAction.action.canceled -= HandleRunActionCanceled;
        MoveAction.action.canceled -= HandleMoveActionCanceled;
        JumpAction.action.canceled -= HandleJumpActionCanceled;

        // Weapon wheel
        revealWeaponWheel.action.performed -= HandleRevealWeaponWheel;
        revealWeaponWheel.action.canceled -= HandleWeaponWheelCanceled;
    }

    public PlayerInput HandleCharacterInput(bool pointerOverUIElement, bool escapePressedThisFrame) {
        mouseDown = mouseDown || firePressedThisFrame || firePressedHeld;
        bool mouseClick = false;
        if (mouseDown && !mouseClickedThisFrame) {
            mouseClick = true;
            mouseClickedThisFrame = true;
        }
        if (pointerOverUIElement) {
            firePressedThisFrame = false;
            firePressedHeld = false;
        }
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 viewPortPoint = OrbitCamera.Camera.ScreenToViewportPoint(mousePosition);
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if ((mouseDelta - previousMouseDelta).magnitude > 50f) mouseDelta = Vector2.zero; // HACK

        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        CursorData targetData = OrbitCamera.GetTargetData(mousePosition, GameManager.I.inputMode);
        PlayerInput characterInputs = PlayerInput.none;
        if (GameManager.I.gameData.phase == GamePhase.world) {
            selectGunThisFrame = 0;
            reloadPressedThisFrame = false;
            incrementItemThisFrame = 0;
            useItemThisFrame = false;
            incrementOverlayThisFrame = 0;
            revealWeaponWheelHeld = false;
        }

        Debug.Log(rightClickHeld);

        characterInputs = new PlayerInput() {
            MoveAxisForward = inputVector.y,
            MoveAxisRight = inputVector.x,
            mouseDelta = _smoothMouse,
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
            zoomInput = zoomInput,
            mouseDown = mouseDown,
            rightMouseDown = rightClickHeld,
            mouseClicked = mouseClick,
            escapePressed = escapePressedThisFrame,
            mousePosition = mousePosition,
            viewPortPoint = viewPortPoint,
            revealWeaponWheel = revealWeaponWheelHeld
        };


        if (!mouseDown) {
            mouseClickedThisFrame = false;
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
        escapePressedThisFrame = false;
        previousMouseDelta = mouseDelta;
        return characterInputs;
    }
}
