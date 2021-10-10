using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum Direction { left, leftUp, up, rightUp, right, rightDown, down, leftDown }

public struct AnimationInput {
    public Direction orientation;
    public PlayerCharacterInput playerInputs;
    public bool isMoving;
    public bool isCrouching;
    public bool isRunning;
    public bool isJumping;
    public bool isClimbing;
    public float wallPressTimer;
    public CharacterState state;
    public GunType gunType;
}

public class HumanoidView : MonoBehaviour, ISaveable {
    public enum Mode { idle, walk, crawl, crouch, run, jump, climb }
    Mode mode;
    public SpriteRenderer spriteRenderer;
    public Transform shadowCaster;
    public GameObject torso;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public Skin skin;
    public Direction direction;
    private float trailTimer;
    public float trailInterval = 0.05f;

    // used by animation
    public void SetFrame(int frame) {
        Octet<Sprite[]> octet = skin.GetCurrentOctet(mode);
        Sprite[] sprites = octet[direction];
        frame = Math.Min(frame, sprites.Length - 1);
        spriteRenderer.sprite = sprites[frame];
    }
    private void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.GetCurrentOctet(mode);
    }
    public void UpdateView(AnimationInput input) {
        switch (input.state) {
            case CharacterState.superJump:
                trailTimer += Time.deltaTime;
                if (trailTimer > trailInterval) {
                    trailTimer = 0f;
                    SpawnTrail();
                }
                spriteRenderer.flipX = input.orientation == Direction.left || input.orientation == Direction.leftUp || input.orientation == Direction.leftDown;
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
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight < 0;
                }
                break;
        }

        // set direction
        direction = input.orientation;
        Vector3 scale = transform.localScale;
        transform.localScale = scale;

        // set mode and animation
        shadowCaster.localScale = new Vector3(0.25f, 0.8f, 0.25f);
        shadowCaster.localPosition = new Vector3(0f, 0.7f, 0f);
        spriteRenderer.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        if (input.isMoving) { //
            if (input.isJumping) {
                mode = Mode.jump;
            } else if (input.isClimbing) {
                mode = Mode.climb;
            } else if (input.isRunning) {
                mode = Mode.run;
            } else if (input.isCrouching) {
                shadowCaster.localScale = new Vector3(0.5f, 0.1f, 0.5f);
                shadowCaster.localPosition = new Vector3(0f, 0.1f, 0f);
                mode = Mode.crawl;
            } else {
                mode = Mode.walk;
            }
            if (animator.clip != walkAnimation) {
                animator.clip = walkAnimation;
                animator.Play();
            }
        } else { // stopped
            if (input.isJumping) {
                mode = Mode.jump;
                spriteRenderer.sprite = skin.jump[direction][0];
            } else if (input.isClimbing) {
                mode = Mode.climb;
                spriteRenderer.sprite = skin.climb[Direction.up][0];
            } else if (input.isCrouching) {
                shadowCaster.localScale = new Vector3(0.25f, 0.4f, 0.25f);
                // shadowCaster.localPosition = new Vector3(0f, 0.3f, 0f);
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                mode = Mode.crouch;

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

    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.legSkin);
    }
}
