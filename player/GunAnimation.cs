using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using KinematicCharacterController;

public class GunAnimation : MonoBehaviour, ISaveable {
    public enum State { idle, walking, shooting, crouching, racking, reloading, running }
    private State _state;
    private int _frame;
    private bool _isShooting;
    private bool _isRacking;
    private bool _isReloading;
    private Direction _direction;
    public GunHandler gunHandler;
    public SpriteRenderer spriteRenderer;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    public Skin skin;
    public PlayerCharacterInput.FireInputs input;
    public bool holstered;

    private float trailTimer;
    public float trailInterval = 0.05f;



    void OnEnable() {
        animator.Play();
    }

    // used by animator
    public void ShootCallback() {
        gunHandler.Shoot(input);
    }
    // used by animator
    public void AimCallback() {
        gunHandler.Aim();
    }
    // used by animator
    public void SetFrame(int frame) {
        _frame = frame;
    }
    // used by animator
    public void EndShoot() {
        if (gunHandler.gunInstance == null)
            return;
        gunHandler.shooting = false;
        _isShooting = false;
    }
    // used by animator
    public void EndRack() {
        _isRacking = false;
    }
    // used by animator
    public void RackCallback() {
        gunHandler.Rack();
    }

    public void StartShooting() {
        if (!_isShooting) {
            // TODO: what is the point of _isShooting, _state, and why is animation set at this point? two ways of handling animations?
            _isShooting = true;
            _isRacking = false;
            _isReloading = false;
            UpdateFrame();
        }
    }
    public void StartRack() {
        if (!_isRacking) {
            _isRacking = true;
            _isReloading = false;
            _isShooting = false;
            UpdateFrame();
        }
    }
    public void StartReload() {
        if (!_isReloading) {
            _isReloading = true;
            _isRacking = false;
            _isShooting = false;
            UpdateFrame();
        }
    }
    public void StopReload() {
        _isReloading = false;
        gunHandler.reloading = false;
    }
    public void ClipIn() {
        gunHandler.ClipIn();
    }

    public void Holster() {
        holstered = true;
        EndShoot();
    }
    public void Unholster() {
        holstered = false;
        _isShooting = false;
        _isRacking = false;
    }

    private void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = GetCurrentOctet(GetCurrentGunType());
    }

    // TODO: why is there separate private state variables and input
    public void UpdateView(AnimationInput input) {
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
        // order state settings by priority
        if (input.isCrouching) { // crouching
            _state = State.crouching;
            if (input.gunType == GunType.unarmed || input.isMoving || input.isJumping || input.isClimbing) {
                spriteRenderer.enabled = false;
            } else {
                spriteRenderer.enabled = true;
            }
        } else if (input.isClimbing || input.isJumping) {
            spriteRenderer.enabled = false;
        } else {
            spriteRenderer.enabled = true;
        }

        if (_isShooting) { // shooting
            _state = State.shooting;
            if (gunHandler.gunInstance != null && gunHandler.gunInstance.baseGun) {
                SetAnimation(gunHandler.gunInstance.baseGun.shootAnimation);
            }
        } else if (_isReloading) {
            _state = State.reloading;
            if (gunHandler.gunInstance != null && gunHandler.gunInstance.baseGun) {
                SetAnimation(gunHandler.gunInstance.baseGun.reloadAnimation);
            }
        } else if (_isRacking) { // racking
            _state = State.racking;
            if (gunHandler.gunInstance != null && gunHandler.gunInstance.baseGun) {
                SetAnimation(gunHandler.gunInstance.baseGun.rackAnimation);
            }
        } else if (input.isMoving) { // walking
            if (input.isRunning) {
                _state = State.running;
            } else {
                _state = State.walking;
            }
            SetAnimation(walkAnimation);
        } else { // idle
            _state = State.idle;
            _frame = 0;
            SetAnimation(idleAnimation);
        }

        UpdateFrame();
    }
    private void SetAnimation(AnimationClip clip) {
        if (animator.clip != clip) {
            animator.clip = clip;
            animator.Play();
        }
    }

    private Octet<Sprite[]> GetCurrentOctet(GunType type) {
        switch (_state) {
            case State.reloading:
                return skin.reloadSprites(type);
            case State.racking:
                return skin.rackSprites(type);
            case State.shooting:
                return skin.shootSprites(type);
            case State.running:
                return skin.runSprites(type);
            case State.walking:
                return skin.walkSprites(type);
            default:
            case State.idle:
                return skin.idleSprites(type);
        }
    }
    private GunType GetCurrentGunType() {
        GunType type = GunType.unarmed;
        if (gunHandler != null && gunHandler.gunInstance != null && !holstered) { //
            type = gunHandler.gunInstance.baseGun.type;
        }
        return type;
    }
    public void UpdateFrame() {
        GunType type = GetCurrentGunType();
        Octet<Sprite[]> _sprites = GetCurrentOctet(type);


        if (_sprites == null)
            return;
        if (_sprites[_direction] == null)
            return;

        int frame = Math.Min(_frame, _sprites[_direction].Length - 1);
        if (_sprites[_direction][frame] == null)
            return;
        spriteRenderer.sprite = _sprites[_direction][frame];
    }
    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.bodySkin);
    }
}
