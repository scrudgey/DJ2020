using UnityEngine;

public struct PlayerInput {
    public struct FireInputs {
        public bool FirePressed;
        public bool FireHeld;
        public TargetData2 targetData;
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
    public Vector3 lookAtPoint;
}