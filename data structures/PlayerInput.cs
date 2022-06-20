using UnityEngine;

public struct PlayerInput {
    public struct FireInputs {
        public bool FirePressed;
        public bool AimPressed;
        public bool FireHeld;
        public CursorData cursorData;
        public static FireInputs none = new FireInputs {
            cursorData = CursorData.none
        };
    }
    public InputMode inputMode;
    public float MoveAxisForward;
    public float MoveAxisRight;
    public Quaternion CameraRotation;
    public bool JumpDown;
    public bool jumpHeld;
    public bool jumpReleased;
    public bool CrouchDown;
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
    public Vector3 torque;
    public static PlayerInput none = new PlayerInput {
        Fire = FireInputs.none
    };
}