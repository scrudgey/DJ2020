using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEditor;
using UnityEngine;
public class LegsAnimation : IBinder<CharacterController>, ISaveable {
    public enum State {
        idle,
        walk,
        crawl,
        crouch,
        run,
        jump,
        climb
    }
    State state;
    private int frame;
    public SpriteRenderer spriteRenderer;
    public Transform shadowCaster;
    public GameObject torso;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip crawlAnimation;
    public Skin skin;
    public Direction direction;
    private float trailTimer;
    public float trailInterval = 0.05f;
    public TorsoAnimation gunAnimation;
    public HeadAnimation headAnimation;
    bool isMoving;
    bool isCrouching;
    bool isCrawling;
    float crouchTransitionTimer;

    void Start() {
        // TODO: fix
        // GameManager.OnFocusChanged += Bind;
        Bind(target.gameObject);
    }

    override public void HandleValueChanged(CharacterController controller) {
        AnimationInput input = controller.BuildAnimationInput();
        UpdateView(input);
    }

    // used by animation
    public void SetFrame(int frame) {
        this.frame = frame;
    }

    private void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.GetCurrentLegsOctet(state);
        gunAnimation?.SpawnTrail();
        headAnimation?.SpawnTrail();
    }

    public void SetBob(int bob) { }

    public void UpdateView(AnimationInput input) {
        if (skin == null)
            return;
        if (input.movementSticking)
            return;
        crouchTransitionTimer += Time.deltaTime;

        // set direction
        direction = input.orientation;
        isMoving = input.isMoving;
        if (isCrouching != input.isCrouching) {
            isCrouching = input.isCrouching;
            crouchTransitionTimer = 0f;
        }
        if (!isCrawling && isMoving && isCrouching) {
            isCrawling = true;
        }
        if (isCrawling && !isCrouching) {
            isCrawling = false;
        }
        float scaleFactor = 1f;
        if (crouchTransitionTimer < 0.1f) {
            scaleFactor = (float)PennerDoubleAnimation.BounceEaseIn(crouchTransitionTimer, 1.1f, -0.1f, 0.1f);
        }
        Vector3 scale = new Vector3(1f, scaleFactor, 1f) * 2.5f;
        transform.localScale = scale;
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

        // set mode and animation
        shadowCaster.localScale = new Vector3(0.25f, 0.8f, 0.25f);
        shadowCaster.localPosition = new Vector3(0f, 0.7f, 0f);
        spriteRenderer.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        if (input.isMoving) { //
            if (input.isJumping) {
                state = State.jump;
            } else if (input.isClimbing) {
                state = State.climb;
            } else if (input.isRunning) {
                state = State.run;
            } else if (input.isCrouching) {
                shadowCaster.localScale = new Vector3(0.5f, 0.1f, 0.5f);
                shadowCaster.localPosition = new Vector3(0f, 0.1f, 0f);
                state = State.crawl;
            } else {
                state = State.walk;
            }

            if (state == State.crawl) {
                if (animator.clip != crawlAnimation) {
                    animator.clip = crawlAnimation;
                    animator.Play();
                }
            } else {
                if (animator.clip != walkAnimation) {
                    animator.clip = walkAnimation;
                    animator.Play();
                }
            }

        } else { // stopped
            if (input.isJumping) {
                state = State.jump;
                spriteRenderer.sprite = skin.legsJump[direction][0];
            } else if (input.isClimbing) {
                state = State.climb;
                spriteRenderer.sprite = skin.legsClimb[Direction.up][0];
            } else if (input.isCrouching) {
                state = State.crouch;
                shadowCaster.localScale = new Vector3(0.25f, 0.4f, 0.25f);
                if (!isCrawling)
                    spriteRenderer.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            } else {
                state = State.idle;
                spriteRenderer.sprite = skin.legsIdle[direction][0];
            }
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Play();
            }
        }

        // TODO: this is what makes the head offset work. but it causes weird clipping and leaning. we should set this, apply offset, then set it back after applying offset
        transform.rotation = input.cameraRotation;
        UpdateFrame();
        gunAnimation.UpdateView(input);
        Vector3 absoluteWorldPosition = headAnimation.transform.position;
        transform.localRotation = Quaternion.identity;
        headAnimation.transform.position = absoluteWorldPosition;
    }

    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.legSkin);
    }

    public void UpdateFrame() {
        Octet<Sprite[]> octet = null;
        octet = skin.GetCurrentLegsOctet(state);
        if (isCrawling) {
            octet = skin.legsCrawl;
        }
        Sprite[] sprites = octet[direction];
        frame = Math.Min(frame, sprites.Length - 1);
        spriteRenderer.sprite = sprites[frame];
    }
}
