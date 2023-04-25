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
    public List<IInputReceiver> inputReceivers = new List<IInputReceiver>();

    [Header("Inputs")]
    // public InputActionReference escapeAction;
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
    private bool mouseDown;
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
    // private bool escapePressedThisFrame;
    // private bool escapePressConsumed;
    Vector2 previousMouseDelta;
    public void HandleMoveAction(InputAction.CallbackContext ctx) {
        inputVector = ctx.ReadValue<Vector2>();
    }
    public void HandleFireAction(InputAction.CallbackContext ctx) {
        firePressedThisFrame = ctx.ReadValueAsButton();
        firePressedHeld = ctx.ReadValueAsButton();
    }
    // public void HandleEscapeAction(InputAction.CallbackContext ctx) {
    //     escapePressedThisFrame = ctx.ReadValueAsButton();
    // }
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
        mouseDown = false;
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
    public void Start() {
        RegisterCallbacks();
    }
    override public void OnDestroy() {
        DeregisterCallbacks();
        base.OnDestroy();
    }

    void RegisterCallbacks() {
        // Escape
        // escapeAction.action.performed += HandleEscapeAction;
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
        CrouchAction.action.canceled -= HandleCrouchActionCanceled;
        RunAction.action.canceled -= HandleRunActionCanceled;
        MoveAction.action.canceled -= HandleMoveActionCanceled;
        JumpAction.action.canceled -= HandleJumpActionCanceled;
    }

    public PlayerInput HandleCharacterInput(bool pointerOverUIElement, bool escapePressedThisFrame) {
        mouseDown = mouseDown || firePressedThisFrame || firePressedHeld;
        if (pointerOverUIElement) {
            firePressedThisFrame = false;
            firePressedHeld = false;
        }

        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if ((mouseDelta - previousMouseDelta).magnitude > 50f) mouseDelta = Vector2.zero; // HACK

        CursorData targetData = OrbitCamera.GetTargetData(cursorPosition, GameManager.I.inputMode);
        PlayerInput characterInputs = PlayerInput.none;
        foreach (IInputReceiver i in inputReceivers) {
            Vector3 directionToCursor = (targetData.worldPosition - i.transform.position).normalized;
            characterInputs = new PlayerInput() {
                MoveAxisForward = inputVector.y,
                MoveAxisRight = inputVector.x,
                mouseDelta = mouseDelta,
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
                lookAtDirection = directionToCursor,
                zoomInput = zoomInput,
                mouseDown = mouseDown,
                escapePressed = escapePressedThisFrame //&& !escapePressConsumed
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
        escapePressedThisFrame = false;
        // escapePressConsumed = false;
        previousMouseDelta = mouseDelta;
        return characterInputs;
    }

    // TODO: this can belong to gamemanager.
    public void SetInputReceivers(GameObject playerObject) {
        inputReceivers = new List<IInputReceiver>();
        foreach (IInputReceiver inputReceiver in playerObject.GetComponentsInChildren<CharacterController>()) {
            inputReceivers.Add(inputReceiver);
        }
        inputReceivers.Add(GameManager.I.characterCamera);
    }
}
