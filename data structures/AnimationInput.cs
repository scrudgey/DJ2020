using UnityEngine;
public struct AnimationInput {
    public struct GunAnimationInput {
        public GunType gunType;
        public GunHandler.GunStateEnum gunState;
        public bool hasGun;
        public bool holstered;
        public GunTemplate baseGun;
        public bool shootRequestedThisFrame;
        public bool aimWeapon;
    }
    public GunAnimationInput gunInput;
    public Direction orientation;
    public PlayerInput playerInputs;
    public bool isMoving;
    public bool isCrouching;
    public bool isProne;
    public bool isRunning;
    public bool isJumping;
    public bool isClimbing;
    public float wallPressTimer;
    public CharacterState state;
    public Quaternion cameraRotation;
    public Vector2 camDir;
    public Vector3 lookAtDirection;
    public bool movementSticking;
    public Vector3 directionToCamera;
    public HitState hitState;
    public Vector3 velocity;
}