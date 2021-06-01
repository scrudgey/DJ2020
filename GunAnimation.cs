using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using KinematicCharacterController;

// TODO: don't shoot at cursor position but shoot in direction. ?
public class GunAnimation : MonoBehaviour {
    public GunHandler gunHandler;
    public SpriteRenderer spriteRenderer;
    public Animation animator;

    public AnimationClip idleAnimation;
    private Octet<Sprite[]> _sprites;
    private int _frame;
    public PlayerCharacterInputs.FireInputs input;
    private Direction direction;


    // these need to be functions so the animation callbacks can work
    public void SetSprites(Octet<Sprite[]> sprites) {
        this._sprites = sprites;
        UpdateFrame();
    }
    public void SetDirection(Direction direction) {
        this.direction = direction;
        UpdateFrame();
    }
    public void SetFrame(int frame) {
        this._frame = frame;
        UpdateFrame();
    }

    public void UpdateFrame() {
        if (_sprites == null)
            return;
        if (_sprites[direction] == null)
            return;
        if (_sprites[direction].Length - 1 < _frame)
            return;
        if (_sprites[direction][_frame] == null)
            return;
        spriteRenderer.sprite = _sprites[direction][_frame];
    }
    public void EndShoot() {
        if (gunHandler.gunInstance == null)
            return;
        _sprites = gunHandler.gunInstance.baseGun.gunIdle;
        _frame = 0;
        UpdateFrame();

        animator.clip = idleAnimation;
        animator.Stop();
        gunHandler.shooting = false;
    }
    public void Holster() {
        EndShoot();
        spriteRenderer.enabled = false;
    }
    public void Unholster() {
        spriteRenderer.enabled = true;
    }
    public void ShootCallback() {
        gunHandler.Shoot(input);
    }
    public void StartShooting() {
        // TODO: replace deep references with class methods
        if (animator.clip != gunHandler.gunInstance.baseGun.shootAnimation || !animator.isPlaying) {
            _sprites = gunHandler.gunInstance.baseGun.gunShoot;
            animator.clip = gunHandler.gunInstance.baseGun.shootAnimation;
            if (!animator.isPlaying)
                animator.Play();
        }
    }


}
