using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class GunAnimation : MonoBehaviour, ISaveable {
    private GunHandler.GunState state;
    private CharacterState characterState;
    private int _frame;
    private Direction _direction;
    public SpriteRenderer spriteRenderer;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    public Skin skin;
    // private float trailTimer;
    public float trailInterval = 0.05f;
    private bool bob;
    private AnimationInput lastInput;

    void SetState(GunHandler.GunState newState) {
        state = newState;
    }
    void OnEnable() {
        animator.Play();
    }
    // used by animator
    public void SetFrame(int frame) {
        _frame = frame;
    }
    // used by animator
    public void SetBob(int bob) {
        this.bob = bob == 1;
    }
    public void UpdateView(AnimationInput input) {
        lastInput = input;

        switch (input.state) {
            // case CharacterState.superJump:
            //     trailTimer += Time.deltaTime;
            //     if (trailTimer > trailInterval) {
            //         trailTimer = 0f;
            //         SpawnTrail();
            //     }
            //     break;
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
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight < 0;
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

        UpdateFrame();
    }
    private void SetAnimation(AnimationClip clip) {
        if (animator.clip != clip) {
            bob = false;
            animator.clip = clip;
            animator.Play();
        }
    }
    public void UpdateFrame() {
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
    public void SpawnTrail() {
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
