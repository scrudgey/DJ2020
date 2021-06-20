using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using KinematicCharacterController;

// TODO: don't shoot at cursor position but shoot in direction. ?
public class GunAnimation : MonoBehaviour {
    public enum State { idle, walking, shooting, crouching, racking }
    private State _state;
    private int _frame;
    private bool _isShooting;
    private bool _isRacking;
    private Direction _direction;
    public GunHandler gunHandler;
    public SpriteRenderer spriteRenderer;
    public Animation animator;

    public Material billboardMaterial;
    public Material flatMaterial;

    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    public Skin skin;
    public PlayerCharacterInputs.FireInputs input;
    public bool holstered;

    void Awake() {
        skin = Skin.LoadSkin("generic");
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
        animator.clip = gunHandler.gunInstance.baseGun.shootAnimation;
        animator.Play();
        _isShooting = true;
        UpdateFrame();
    }
    public void StartRack() {
        if (!_isRacking) {
            animator.clip = gunHandler.gunInstance.baseGun.rackAnimation;
            animator.Play();
            _isRacking = true;
            UpdateFrame();
        }
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

    // TODO: why is there separate private state variables and input
    public void UpdateView(AnimationInput input) {
        if (input.wallPressTimer > 0 && !input.wallPress) {
            spriteRenderer.material = flatMaterial;
        } else if (input.wallPress) {
            spriteRenderer.material = billboardMaterial;
        } else {
            spriteRenderer.material = billboardMaterial;
        }

        AnimationClip walkAnimation = unarmedWalkAnimation;

        spriteRenderer.flipX = input.direction == Direction.left || input.direction == Direction.leftUp || input.direction == Direction.leftDown;
        _direction = input.direction;

        // order state settings by priority

        if (_isShooting) { // shooting
            _state = State.shooting;
        } else if (_isRacking) { // racking
            _state = State.racking;
        } else if (input.isMoving) { // walking
            _state = State.walking;
            if (animator.clip != walkAnimation) {
                animator.clip = walkAnimation;
                animator.Play();
            }
        } else if (input.isCrouching) { // crouching
            _state = State.crouching;
        } else { // idle
            _state = State.idle;
            _frame = 0;
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Play();
            }
        }

        UpdateFrame();
    }


    public void UpdateFrame() {
        Octet<Sprite[]> _sprites = null;
        GunType type = GunType.unarmed;
        if (gunHandler != null && gunHandler.gunInstance != null && !holstered) {
            type = gunHandler.gunInstance.baseGun.type;
        }

        switch (_state) {
            case State.racking:
                _sprites = skin.shotgunRack;
                break;
            case State.shooting:
                _sprites = skin.shootSprites(type);
                break;
            case State.walking:
                _sprites = skin.walkSprites(type);
                break;
            default:
            case State.idle:
                _sprites = skin.idleSprites(type);
                break;
        }

        if (_sprites == null)
            return;
        if (_sprites[_direction] == null)
            return;

        int frame = Math.Min(_frame, _sprites[_direction].Length - 1);
        if (_sprites[_direction][frame] == null)
            return;
        spriteRenderer.sprite = _sprites[_direction][frame];
    }

}
