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
    public float wallPressTimer;
    public bool wallPress;
}

public class DirectionalBillboard : MonoBehaviour {
    enum Mode { idle, walk, crawl, crouch }
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
                // spriteRenderer.sprite = _crawlSprites[frame];
                break;
            case Mode.crouch:
                spriteRenderer.sprite = skin.legsCrouch[direction][0];
                break;
            default:
            case Mode.idle:
                spriteRenderer.sprite = skin.legsIdle[direction][0];
                break;
        }
    }

    public void UpdateView(AnimationInput input) {
        direction = input.direction;

        if (input.wallPressTimer > 0 && !input.wallPress) {
            spriteRenderer.material = flatMaterial;
        } else if (input.wallPress) {
            spriteRenderer.material = billboardMaterial;
        } else {
            spriteRenderer.material = billboardMaterial;
        }

        spriteRenderer.flipX = input.direction == Direction.left || input.direction == Direction.leftUp || input.direction == Direction.leftDown;

        if (input.isMoving) {
            if (input.isCrouching) {
                mode = Mode.crawl;
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            } else {
                mode = Mode.walk;
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }
            if (animator.clip != walkAnimation) {
                animator.clip = walkAnimation;
                animator.Play();
            }
        } else {
            spriteRenderer.transform.localPosition = new Vector3(0f, 0.75f, 0f);
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
