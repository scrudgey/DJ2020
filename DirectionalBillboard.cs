using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum Direction { left, leftUp, up, rightUp, right, rightDown, down, leftDown }

public struct AnimationInput {
    public Direction direction;
    public bool isMoving;
    public bool isCrouching;
    public bool isRunning;
    public float wallPressTimer;
    // public bool wallPress;
    public CharacterState state;
}

public class DirectionalBillboard : MonoBehaviour {
    enum Mode { idle, walk, crawl, crouch, run }
    Mode mode;
    public SpriteRenderer spriteRenderer;
    public GameObject torso;
    public Material billboardMaterial;
    public Material flatMaterial;
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
        // Debug.Log($"{mode} {frame}");
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
        direction = input.direction;

        switch (input.state) {
            default:
            case CharacterState.normal:
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material = flatMaterial;
                } else {
                    spriteRenderer.material = billboardMaterial;
                }
                break;
            case CharacterState.wallPress:
                spriteRenderer.material = billboardMaterial;
                break;
        }



        spriteRenderer.flipX = input.direction == Direction.left || input.direction == Direction.leftUp || input.direction == Direction.leftDown;
        Debug.Log($"{spriteRenderer.flipX} {input.direction}");
        Vector3 scale = transform.localScale;
        if (spriteRenderer.flipX) {
            scale.x = -1f * Mathf.Abs(scale.x);
        }
        transform.localScale = scale;

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
                spriteRenderer.sprite = skin.legsCrouch[direction][0];

            } else {
                mode = Mode.idle;
                spriteRenderer.sprite = skin.legsIdle[direction][0];
            }
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Play();
            }
        }

        if (input.isCrouching) {
            if (torso.activeInHierarchy) {
                torso.SetActive(false);
            }
        } else {
            if (!torso.activeInHierarchy) {
                torso.SetActive(true);
            }
        }

    }
}
