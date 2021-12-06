using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class GunAnimation : MonoBehaviour, ISaveable {
    private GunHandler.GunState state;
    private CharacterState characterState;
    private int _frame;
    // private bool _isShooting;
    // private bool _isRacking;
    // private bool _isReloading;
    private Direction _direction;
    // public GunHandler gunHandler;
    public SpriteRenderer spriteRenderer;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    public Skin skin;
    // public PlayerCharacterInput.FireInputs input;
    // public bool holstered;

    private float trailTimer;
    public float trailInterval = 0.05f;
    private bool bob;
    // private GunType currentGunType;
    private AnimationInput lastInput;
    // public bool HasGun() {
    //     return gunHandler != null && gunHandler.HasGun();
    // }
    void SetState(GunHandler.GunState newState) {
        if (newState != state) {
            bob = false;
        }
        state = newState;
    }
    void OnEnable() {
        animator.Play();
    }

    // used by animator
    // public void ShootCallback() {
    // gunHandler.Shoot(input);
    // }
    // // used by animator
    // public void AimCallback() {
    //     gunHandler.Aim();
    // }
    // // used by animator
    // public void RackCallback() {
    //     gunHandler.Rack();
    // }
    // // used by animator
    // public void EndShoot() {
    //     if (gunHandler.gunInstance == null)
    //         return;
    //     gunHandler.shooting = false;
    //     _isShooting = false;
    // }


    // used by animator
    public void SetFrame(int frame) {
        _frame = frame;
    }
    public void SetBob(int bob) {
        this.bob = bob == 1;
    }

    // // used by animator
    // public void EndRack() {
    //     _isRacking = false;
    // }

    // public void StartShooting() {
    //     if (!_isShooting) {
    //         // TODO: what is the point of _isShooting, _state, and why is animation set at this point? two ways of handling animations?
    //         _isShooting = true;
    //         _isRacking = false;
    //         _isReloading = false;
    //         UpdateFrame();
    //     }
    // }
    // public void StartRack() {
    //     if (!_isRacking) {
    //         _isRacking = true;
    //         _isReloading = false;
    //         _isShooting = false;
    //         UpdateFrame();
    //     }
    // }
    // public void StartReload() {
    //     if (!_isReloading) {
    //         _isReloading = true;
    //         _isRacking = false;
    //         _isShooting = false;
    //         UpdateFrame();
    //     }
    // }
    // public void StopReload() {
    //     _isReloading = false; // TODO: delete this?s
    // }
    // public void ClipIn() {
    //     gunHandler.ClipIn();
    // }

    // public void Holster() {
    //     holstered = true;
    //     // EndShoot();
    // }
    // public void Unholster() {
    //     holstered = false;
    //     _isShooting = false;
    //     _isRacking = false;
    // }



    // TODO: why is there separate private state variables and input
    public void UpdateView(AnimationInput input) {
        // currentGunType = input.gunInput.gunType;
        lastInput = input;

        switch (input.state) {
            case CharacterState.superJump:
                trailTimer += Time.deltaTime;
                if (trailTimer > trailInterval) {
                    trailTimer = 0f;
                    SpawnTrail();
                }
                break;
            default:
            case CharacterState.normal:
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material.DisableKeyword("_BILLBOARD");
                } else {
                    spriteRenderer.material.EnableKeyword("_BILLBOARD");
                }
                spriteRenderer.flipX = input.orientation == Direction.left || input.orientation == Direction.leftUp || input.orientation == Direction.leftDown;
                break;
            case CharacterState.wallPress:
                spriteRenderer.material.DisableKeyword("_BILLBOARD");
                if (input.playerInputs.MoveAxisRight != 0) {
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight > 0;
                }
                break;
        }

        AnimationClip walkAnimation = unarmedWalkAnimation;

        _direction = input.orientation;

        transform.localPosition = Vector3.zero;
        if (bob) {
            transform.localPosition -= new Vector3(0f, 0.02f, 0f);
        }

        SetState(input.gunInput.gunState);
        characterState = input.state;

        if (input.gunInput.hasGun) {
            switch (state) {
                case GunHandler.GunState.shooting:
                    SetAnimation(input.gunInput.baseGun.shootAnimation);
                    break;
                case GunHandler.GunState.reloading:
                    SetAnimation(input.gunInput.baseGun.reloadAnimation);
                    break;
                case GunHandler.GunState.racking:
                    SetAnimation(input.gunInput.baseGun.rackAnimation);
                    break;
                default:
                    if (input.isMoving) {
                        SetAnimation(walkAnimation);
                    } else {
                        _frame = 0;
                        SetAnimation(idleAnimation);
                    }
                    break;
            }
        } else {
            if (input.isMoving) {
                SetAnimation(walkAnimation);
            } else {
                _frame = 0;
                SetAnimation(idleAnimation);
            }
        }

        // if (state == GunHandler.State.shooting) { // shooting
        //                                           // SetState(State.shooting);
        //     if (gunHandler.HasGun()) { // should be input
        //         SetAnimation(gunHandler.gunInstance.baseGun.shootAnimation);
        //     }
        // } else if (_isReloading) {
        //     // SetState(State.reloading);
        //     if (gunHandler.HasGun()) {
        //         SetAnimation(gunHandler.gunInstance.baseGun.reloadAnimation);
        //     }
        // } else 
        // if (_isRacking) { // racking
        //                   // SetState(State.racking);
        //     if (gunHandler.HasGun()) {
        //         SetAnimation(gunHandler.gunInstance.baseGun.rackAnimation);
        //     }
        // } else { // walking


        // }

        UpdateFrame();
    }

    // private void FixCrouchOffset() {
    //     if (gunHandler.gunInstance == null)
    //         return;
    //     Vector3 offset = Vector3.zero;
    //     // switch (_direction) {
    //     //     case Direction.right:
    //     //         offset = new Vector3(0f, 0.07f, 0.06f);
    //     //         break;
    //     //     case Direction.rightUp:
    //     //         offset = new Vector3(0.07f, 0.07f, 0f);
    //     //         break;
    //     //     case Direction.up:
    //     //         offset = new Vector3(0.06f, 0.07f, 0f);
    //     //         break;
    //     //     case Direction.leftUp:
    //     //         offset = new Vector3(0f, 0.07f, 0.07f);
    //     //         break;
    //     //     case Direction.left:
    //     //         offset = new Vector3(0f, 0.07f, 0.07f);
    //     //         break;
    //     //     case Direction.leftDown:
    //     //         offset = new Vector3(0.06f, 0.04f, 0f);
    //     //         break;
    //     //     case Direction.down:
    //     //         offset = new Vector3(-0.05f, 0.04f, 0f);
    //     //         break;
    //     //     case Direction.rightDown:
    //     //         offset = new Vector3(0f, 0.05f, 0.06f);
    //     //         break;
    //     // }
    //     transform.localPosition -= offset;
    // }
    private void SetAnimation(AnimationClip clip) {
        if (animator.clip != clip) {
            animator.clip = clip;
            animator.Play();
        }
    }


    // private GunType GetCurrentGunType() {
    //     GunType type = GunType.unarmed;
    //     if (HasGun() && !holstered) { //
    //         type = gunHandler.gunInstance.baseGun.type; // this should be part of input!!! 
    //     }
    //     return type;
    // }
    public void UpdateFrame() {
        // GunType type = GetCurrentGunType();
        Octet<Sprite[]> _sprites = skin.GetCurrentTorsoOctet(lastInput);

        if (_sprites == null)
            return;
        if (_sprites[_direction] == null)
            return;

        int frame = Math.Min(_frame, _sprites[_direction].Length - 1);
        if (_sprites[_direction][frame] == null)
            return;
        spriteRenderer.sprite = _sprites[_direction][frame];
    }
    private void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.GetCurrentTorsoOctet(lastInput);
        // TODO: set direction
        // billboard.direction = _direction;
    }
    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.bodySkin);
    }
}
