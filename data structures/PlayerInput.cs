using Items;
using UnityEngine;
public struct PlayerInput {
    public struct FireInputs {
        public bool FirePressed;
        public bool AimPressed;
        public bool FireHeld;
        public CursorData cursorData;
        public bool skipAnimation;
        public static FireInputs none = new FireInputs {
            cursorData = CursorData.none
        };
    }
    // public InputMode inputMode;
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Vector2 mouseDelta;
    // public Vector2 unsmoothedMouseDelta;
    public Quaternion CameraRotation;
    public bool JumpDown;
    public bool jumpHeld;
    public bool jumpReleased;
    public bool CrouchDown;
    public bool unCrawl;
    public bool runDown;
    public FireInputs Fire;
    public bool reload;
    public int selectgun;
    public bool actionButtonPressed;
    public int incrementItem;
    public ItemTemplate selectItem;
    public bool useItem;
    public int incrementOverlay;
    public bool rotateCameraRightPressedThisFrame;
    public bool rotateCameraLeftPressedThisFrame;
    public Vector3 moveDirection;
    public Vector3 orientTowardPoint; // used only by sphere controller and taskshoot
    public Vector3 orientTowardDirection; // used only by sphere
    public Vector3 lookAtDirection;
    public Vector3 lookAtPosition;
    public bool snapToLook;
    public bool snapFreeLook;
    public bool preventWallPress;
    public Vector2 zoomInput;
    public bool aimWeapon;
    public bool mouseDown;
    public bool rightMouseDown;
    public bool mouseClicked;
    public bool escapePressed;
    public Vector2 mousePosition;
    public Vector2 viewPortPoint;
    public bool armsRaised;
    public bool revealWeaponWheel;
    public static PlayerInput none = new PlayerInput {
        Fire = FireInputs.none
    };
    public Vector2 MoveAxis() => new Vector2(MoveAxisRight, MoveAxisForward);
    public Vector3 MoveInputVector() => Vector3.ClampMagnitude(new Vector3(MoveAxisRight, 0f, MoveAxisForward), 1f);


    public void ApplyInputMask(InputProfile profile) {
        if (!profile.allowPlayerFireInput) {
            Fire = FireInputs.none;
            useItem = false;
        }
        if (!profile.allowBurglarButton) {
            Fire.cursorData.attackSurface = null;
        }
        if (!profile.allowPlayerMovement) {
            MoveAxisForward = 0;
            MoveAxisRight = 0;
            JumpDown = false;
            jumpHeld = false;
            jumpReleased = false;
            CrouchDown = false;
            unCrawl = false;
            runDown = false;
            reload = false;
            selectgun = 0;
            actionButtonPressed = false;
            moveDirection = Vector3.zero;
            orientTowardPoint = Vector3.zero;
            orientTowardDirection = Vector3.zero;
            aimWeapon = false;
            mouseDown = false;
            rightMouseDown = false;
            mouseClicked = false;

            // public Vector3 lookAtDirection= Vector3.zero;
            // public Vector3 lookAtPosition;
            // public bool snapToLook;

            // public bool preventWallPress;
            // public Vector2 zoomInput
            // public bool escapePressed = false
            // public Vector2 mousePosition;
            // public Vector2 viewPortPoint;
            // public bool armsRaised;
            // public bool revealWeaponWheel;
        }
        if (!profile.allowCameraControl) {
            rotateCameraRightPressedThisFrame = false;
            rotateCameraLeftPressedThisFrame = false;
            CameraRotation = Quaternion.identity;
            // mouseDelta = Vector2.zero;
        }
        if (!profile.allowePlayerItemSelect) {
            incrementItem = 0;
            selectItem = null;
            revealWeaponWheel = false;
        }
        if (!profile.allowOverlayControl) {
            incrementOverlay = 0;
        }
    }
}

public struct OverrideInput {

}