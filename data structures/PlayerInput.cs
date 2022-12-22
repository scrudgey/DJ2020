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
    public bool preventWallPress;
    public Vector2 zoomInput;
    public bool aimWeapon;
    public bool mouseDown;
    public static PlayerInput none = new PlayerInput {
        Fire = FireInputs.none
    };
    public Vector2 MoveAxis() {
        return new Vector2(MoveAxisRight, MoveAxisForward);
    }
}