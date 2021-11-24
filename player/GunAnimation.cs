using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class GunAnimation : MonoBehaviour, ISaveable {
    public enum State { idle, walking, shooting, crouching, racking, reloading, running, climbing, crawling }
    private State state;
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
    private bool bob;
    public bool HasGun() {
        return gunHandler != null && gunHandler.HasGun();
    }
    void SetState(State newState) {
        if (newState != state) {
            bob = false;
        }
        state = newState;
    }
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
    public void SetBob(int bob) {
        this.bob = bob == 1;
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
        billboard.skin = skin.GetCurrentTorsoOctet(state, GetCurrentGunType());
        // TODO: set direction
        // billboard.direction = _direction;
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
        if (bob) {
            transform.localPosition -= new Vector3(0f, 0.01f, 0f);
        }

        if (_isShooting) { // shooting
            SetState(State.shooting);
            if (gunHandler.HasGun()) {
                SetAnimation(gunHandler.gunInstance.baseGun.shootAnimation);
            }
        } else if (_isReloading) {
            SetState(State.reloading);
            if (gunHandler.HasGun()) {
                SetAnimation(gunHandler.gunInstance.baseGun.reloadAnimation);
            }
        } else if (_isRacking) { // racking
            SetState(State.racking);
            if (gunHandler.HasGun()) {
                SetAnimation(gunHandler.gunInstance.baseGun.rackAnimation);
            }
        } else { // walking
            if (input.isMoving) {
                if (input.isRunning) {
                    SetState(State.running);
                } else {
                    SetState(State.walking);
                }
                if (input.isCrouching) {
                    SetState(State.crawling);
                }
                if (input.isClimbing) {
                    SetState(State.climbing);
                }
                SetAnimation(walkAnimation);
            } else {
                SetState(State.idle);
                _frame = 0;
                if (input.isCrouching) {
                    SetState(State.crouching);
                    FixCrouchOffset();
                }
                if (input.isClimbing) {
                    SetState(State.climbing);
                }
                SetAnimation(idleAnimation);
            }

        }

        UpdateFrame();
    }

    private void FixCrouchOffset() {
        Vector3 offset = Vector3.zero;
        switch (_direction) {
            case Direction.right:
                offset = new Vector3(0f, 0.07f, 0.06f);
                break;
            case Direction.rightUp:
                offset = new Vector3(0.07f, 0.07f, 0f);
                break;
            case Direction.up:
                offset = new Vector3(0.06f, 0.07f, 0f);
                break;
            case Direction.leftUp:
                offset = new Vector3(0f, 0.07f, 0.07f);
                break;
            case Direction.left:
                offset = new Vector3(0f, 0.07f, 0.07f);
                break;
            case Direction.leftDown:
                offset = new Vector3(0.06f, 0.04f, 0f);
                break;
            case Direction.down:
                offset = new Vector3(-0.05f, 0.04f, 0f);
                break;
            case Direction.rightDown:
                offset = new Vector3(0f, 0.05f, 0.06f);
                break;
        }
        transform.localPosition -= offset;
    }
    private void SetAnimation(AnimationClip clip) {
        if (animator.clip != clip) {
            animator.clip = clip;
            animator.Play();
        }
    }


    private GunType GetCurrentGunType() {
        GunType type = GunType.unarmed;
        if (HasGun() && !holstered) { //
            type = gunHandler.gunInstance.baseGun.type;
        }
        return type;
    }
    public void UpdateFrame() {
        GunType type = GetCurrentGunType();
        Octet<Sprite[]> _sprites = skin.GetCurrentTorsoOctet(state, GetCurrentGunType());

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
