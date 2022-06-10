using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputController : MonoBehaviour {
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
    private bool jumpPressedThisFrame;
    private bool jumpReleasedThisFrame;
    private bool jumpHeld;
    private bool reloadPressedThisFrame;
    private int selectGunThisFrame;
    private bool actionButtonPressedThisFrame;
    private int incrementItemThisFrame;
    private int incrementOverlayThisFrame;
    private bool useItemThisFrame;
    public void Awake() {
        // Move
        MoveAction.action.performed += ctx => inputVector = ctx.ReadValue<Vector2>();

        // Fire
        FireAction.action.performed += ctx => {
            firePressedThisFrame = ctx.ReadValueAsButton();
            firePressedHeld = ctx.ReadValueAsButton();
        };

        // Aim
        AimAction.action.performed += ctx => {
            aimPressedThisFrame = ctx.ReadValueAsButton();
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

        DebugBreakAction.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                Debug.Break();
            }
        };

        // Gun switch
        gunHolster.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                selectGunThisFrame = -1;
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

        // Overlay
        nextOverlay.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                incrementOverlayThisFrame = 1;
            }
        };

        previousOverlay.action.performed += ctx => {
            if (ctx.ReadValueAsButton()) {
                incrementOverlayThisFrame = -1;
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
        TargetData2 targetData = OrbitCamera.GetTargetData();
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
                targetData = targetData,
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
            torque = torque
        };

        foreach (IInputReceiver i in inputReceivers) {
            i.SetInputs(characterInputs);
        }

        if (characterInputs.incrementOverlay != 0) {
            GameManager.I.IncrementOverlay(characterInputs.incrementOverlay);
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
    }

}
