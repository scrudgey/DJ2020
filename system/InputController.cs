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

    public static TargetData2 CursorToTarget(CharacterCamera OrbitCamera) {
        Vector2 cursorPosition = Mouse.current.position.ReadValue();

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
        Vector3 targetPoint = Vector3.zero;
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
            return new TargetData2 {
                type = TargetData2.TargetType.objectLock,
                clickRay = clickRay,
                screenPosition = OrbitCamera.Camera.WorldToScreenPoint(targetPoint),
                highlightableTargetData = interactorData,
                position = targetPoint
            };
        }

        return new TargetData2 {
            type = TargetData2.TargetType.direction,
            screenPosition = cursorPosition,
            highlightableTargetData = interactorData,
            clickRay = clickRay,
            position = targetPoint
        };
    }
    private void HandleCharacterInput() {

        TargetData2 targetData = CursorToTarget(OrbitCamera);

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
                targetData = targetData
            },
            reload = reloadPressedThisFrame,
            selectgun = selectGunThisFrame,
            actionButtonPressed = actionButtonPressedThisFrame,
            incrementItem = incrementItemThisFrame,
            useItem = useItemThisFrame,
            incrementOverlay = incrementOverlayThisFrame,
            rotateCameraRightPressedThisFrame = rotateCameraRightPressedThisFrame,
            rotateCameraLeftPressedThisFrame = rotateCameraLeftPressedThisFrame
        };

        foreach (IInputReceiver i in inputReceivers) {
            i.SetInputs(characterInputs);
        }

        if (characterInputs.incrementOverlay != 0) {
            GameManager.I.IncrementOverlay(characterInputs.incrementOverlay);
        }

        firePressedThisFrame = false;
        jumpPressedThisFrame = false;
        reloadPressedThisFrame = false;
        jumpReleasedThisFrame = false;
        selectGunThisFrame = -1;
        actionButtonPressedThisFrame = false;
        incrementItemThisFrame = 0;
        incrementOverlayThisFrame = 0;
        useItemThisFrame = false;
        rotateCameraLeftPressedThisFrame = false;
        rotateCameraRightPressedThisFrame = false;
    }

}
