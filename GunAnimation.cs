using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using KinematicCharacterController;

// TODO: don't shoot at cursor position but shoot in direction. ?
public class GunAnimation : MonoBehaviour {
    public enum State { idle, walking, shooting, crouching }
    private State _state;
    private int _frame;
    private bool _isShooting;
    private Direction _direction;
    public GunHandler gunHandler;
    public SpriteRenderer spriteRenderer;
    public Animation animator;

    public Material billboardMaterial;
    public Material flatMaterial;

    public AnimationClip idleAnimation;
    private Octet<Sprite> _idleSprites;
    private Octet<Sprite[]> _walkSprites;
    private Octet<Sprite[]> _shootSprites;
    public PlayerCharacterInputs.FireInputs input;

    // used by animator
    public void ShootCallback() {
        gunHandler.Shoot(input);
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
    public void StartShooting() {
        if (animator.clip != gunHandler.gunInstance.baseGun.shootAnimation) {
            animator.clip = gunHandler.gunInstance.baseGun.shootAnimation;
        }
        if (!animator.isPlaying)
            animator.Play();
        _isShooting = true;
        UpdateFrame(_frame);
    }

    public void Holster() {
        EndShoot();
        spriteRenderer.enabled = false;
    }
    public void Unholster() {
        spriteRenderer.enabled = true;
    }

    public void UpdateView(AnimationInput input) {
        if (input.wallPressTimer > 0 && !input.wallPress) {
            spriteRenderer.material = flatMaterial;
        } else if (input.wallPress) {
            spriteRenderer.material = billboardMaterial;
        } else {
            spriteRenderer.material = billboardMaterial;
        }

        if (gunHandler != null && gunHandler.gunInstance != null) {
            _idleSprites = gunHandler.gunInstance.baseGun.idle;
            _walkSprites = gunHandler.gunInstance.baseGun.walk;
            _shootSprites = gunHandler.gunInstance.baseGun.shoot;
        }

        spriteRenderer.flipX = input.direction == Direction.left || input.direction == Direction.leftUp || input.direction == Direction.leftDown;
        _direction = input.direction;

        if (_isShooting) {
            _state = State.shooting;
        } else if (input.isMoving && gunHandler.gunInstance != null && gunHandler.gunInstance.baseGun.walkAnimation != null) {
            _state = State.walking;
            if (animator.clip != gunHandler.gunInstance.baseGun.walkAnimation) {
                animator.clip = gunHandler.gunInstance.baseGun.walkAnimation;
                animator.Play();
            }
        } else if (input.isCrouching) {
            _state = State.crouching;
        } else {
            _state = State.idle;
            _frame = 0;
            if (gunHandler.gunInstance != null)
                spriteRenderer.sprite = gunHandler.gunInstance.baseGun.idle[input.direction];
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Stop();
            }
        }

        UpdateFrame(_frame);
    }


    public void UpdateFrame(int frame) {
        Octet<Sprite[]> _sprites = null;
        switch (_state) {
            default:
            case State.idle:
                if (gunHandler.gunInstance != null)
                    spriteRenderer.sprite = gunHandler.gunInstance.baseGun.idle[_direction];
                return;
            case State.walking:
                _sprites = _walkSprites;
                break;
            case State.shooting:
                _sprites = _shootSprites;
                break;
        }

        if (_sprites == null)
            return;
        if (_sprites[_direction] == null)
            return;

        frame = Math.Min(frame, _sprites[_direction].Length - 1);
        if (_sprites[_direction][frame] == null)
            return;
        spriteRenderer.sprite = _sprites[_direction][frame];
    }

}
