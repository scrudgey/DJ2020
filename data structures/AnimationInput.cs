using Items;
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
        public GunType fromGunType;
        public GunType toGunType;
        public static GunAnimationInput None() => new GunAnimationInput {
            gunType = GunType.unarmed,
            gunState = GunHandler.GunStateEnum.idle,
            hasGun = false,
            holstered = true,
            baseGun = null,
            shootRequestedThisFrame = false,
            aimWeapon = false
        };
    }
    public struct ItemHandlerInput {
        public ItemInstance activeItem;
        public ItemHandler.State state;
        public static ItemHandlerInput None() => new ItemHandlerInput {
            activeItem = null,
            state = ItemHandler.State.idle
        };
    }
    public GunAnimationInput gunInput;
    public ItemHandlerInput itemInput;
    public Direction orientation;
    public PlayerInput playerInputs;
    public bool isMoving;
    public bool isCrouching;
    public bool isProne;
    public bool isRunning;
    public bool isJumping;
    public bool isClimbing;
    public bool isSpeaking;
    public float wallPressTimer;
    public CharacterState state;
    public Quaternion cameraRotation;
    public Vector2 camDir;
    public Vector3 lookAtDirection;
    public bool movementSticking;
    public Vector3 directionToCamera;
    public HitState hitState;
    public Vector3 velocity;
    public bool wavingArm;
    public bool armsRaised;
    public Color lightProbeColor;
}