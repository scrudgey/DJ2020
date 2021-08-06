using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum Direction { left, leftUp, up, rightUp, right, rightDown, down, leftDown }

public struct AnimationInput {
    public Direction orientation;
    public PlayerCharacterInputs playerInputs;
    public bool isMoving;
    public bool isCrouching;
    public bool isRunning;
    public float wallPressTimer;
    public CharacterState state;
    public GunType gunType;
}

public class DirectionalBillboard : MonoBehaviour {
    enum Mode { idle, walk, crawl, crouch, run }
    Mode mode;
    public SpriteRenderer spriteRenderer;
    public GameObject torso;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public Skin skin;
    public Direction direction;
    void Awake() {
        skin = Skin.LoadSkin("generic");
    }

    // used by animation
    public void SetFrame(int frame) {
        switch (mode) {
            case Mode.walk:
                spriteRenderer.sprite = skin.legsWalk[direction][frame];
                break;
            case Mode.crawl:
                spriteRenderer.sprite = skin.legsCrawl[direction][frame];
                break;
            case Mode.crouch:
                spriteRenderer.sprite = skin.legsCrouch[direction][0];
                break;
            case Mode.run:
                spriteRenderer.sprite = skin.legsRun[direction][frame];
                break;
            default:
            case Mode.idle:
                spriteRenderer.sprite = skin.legsIdle[direction][0];
                break;
        }
    }

    public void UpdateView(AnimationInput input) {

        switch (input.state) {
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

        // set direction
        direction = input.orientation;
        Vector3 scale = transform.localScale;
        if (spriteRenderer.flipX) {
            scale.x = -1f * Mathf.Abs(scale.x);
        }
        transform.localScale = scale;

        // set mode and animation
        spriteRenderer.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        if (input.isMoving) { //
            if (input.isRunning) {
                mode = Mode.run;
            } else if (input.isCrouching) {
                mode = Mode.crawl;
            } else {
                mode = Mode.walk;
            }
            if (animator.clip != walkAnimation) {
                animator.clip = walkAnimation;
                animator.Play();
            }
        } else { // stopped
            if (input.isCrouching) {
                mode = Mode.crouch;

                // TODO: if unarmed, use unarmedcrouch. otherwise, use legscrouch.
                if (input.gunType == GunType.unarmed) {
                    spriteRenderer.sprite = skin.unarmedCrouch[direction][0];
                } else {
                    spriteRenderer.sprite = skin.legsCrouch[direction][0];
                }
            } else {
                mode = Mode.idle;
                spriteRenderer.sprite = skin.legsIdle[direction][0];
            }
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Play();
            }
        }

    }
}
