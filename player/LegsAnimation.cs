using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class LegsAnimation : IBinder<CharacterController>, ISkinStateLoader {
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
    HitState hitState;
    private int frame;
    public SpriteRenderer spriteRenderer;
    public Transform shadowCaster;
    public GameObject torso;
    public Animation animator;
    AnimationInput lastInput;
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip walkSlowAnimation;
    public AnimationClip crawlAnimation;
    public Skin skin;
    public Direction direction;
    private float trailTimer;
    public float trailInterval = 0.05f;
    public TorsoAnimation torsoAnimation;
    public HeadAnimation headAnimation;
    public Vector3 offset;
    public Vector3 scaleOffset;
    bool isMoving;
    bool isCrouching;
    float crouchTransitionTimer;
    float hitstunTimer;
    bool hitstunEffect;
    void Start() {
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
    public IEnumerator Shake(float intensity, float interval) {
        float blinkTimer = 0;
        while (blinkTimer < interval) {
            blinkTimer += Time.unscaledDeltaTime;
            offset = UnityEngine.Random.insideUnitSphere * intensity;
            scaleOffset = UnityEngine.Random.insideUnitSphere * intensity * 10f;
            yield return null;
        }
        offset = Vector3.zero;
        scaleOffset = Vector3.zero;
    }
    void Update() {
        // TODO: replace this with a coroutine initiated by animation input ?
        if (hitState == HitState.hitstun) {
            hitstunTimer += Time.deltaTime;
            if (hitstunTimer >= 0.01f) {
                hitstunTimer -= 0.01f;
                float intensity = 0.05f;
                offset = UnityEngine.Random.insideUnitSphere * intensity;
                scaleOffset = UnityEngine.Random.insideUnitSphere * intensity * 10f;
            }
        } else if (hitState == HitState.invulnerable) {
            hitstunTimer += Time.deltaTime;
            if (hitstunTimer >= 0.05f) {
                hitstunTimer -= 0.05f;
                hitstunEffect = !hitstunEffect;
                if (hitstunEffect) {
                    spriteRenderer.enabled = false;
                    torsoAnimation.spriteRenderer.enabled = false;
                    headAnimation.spriteRenderer.enabled = false;
                } else {
                    spriteRenderer.enabled = true;
                    torsoAnimation.spriteRenderer.enabled = true;
                    headAnimation.spriteRenderer.enabled = true;
                }
            }
        } else {
            spriteRenderer.enabled = true;
            torsoAnimation.spriteRenderer.enabled = true;
            headAnimation.spriteRenderer.enabled = true; // TODO: ok?xs
            offset = Vector3.zero;
            scaleOffset = Vector3.zero;
        }
    }
    private void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.GetCurrentLegsOctet(state, lastInput);
        torsoAnimation?.SpawnTrail();
        headAnimation?.SpawnTrail();
    }

    public void SetBob(int bob) { }

    public void UpdateView(AnimationInput input) {
        if (skin == null)
            return;
        if (input.movementSticking)
            return;
        lastInput = input;
        crouchTransitionTimer += Time.deltaTime;
        hitState = input.hitState;

        // set direction
        direction = input.orientation;
        isMoving = input.isMoving;
        if (isCrouching != input.isCrouching) {
            isCrouching = input.isCrouching;
            crouchTransitionTimer = 0f;
        }
        float scaleFactor = 1f;
        if (crouchTransitionTimer < 0.1f) {
            scaleFactor = (float)PennerDoubleAnimation.BounceEaseIn(crouchTransitionTimer, 1.1f, -0.1f, 0.1f);
        }
        Vector3 scale = new Vector3(1f, scaleFactor, 1f) * 2.5f;
        transform.localScale = scale + scaleOffset;
        switch (input.state) {
            case CharacterState.dead:
                break;
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
            case CharacterState.popout:
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material.DisableKeyword("_BILLBOARD");
                } else {
                    spriteRenderer.material.EnableKeyword("_BILLBOARD");
                }
                spriteRenderer.flipX = input.orientation == Direction.left || input.orientation == Direction.leftUp || input.orientation == Direction.leftDown;
                break;
            case CharacterState.wallPress:
                // spriteRenderer.material.DisableKeyword("_BILLBOARD");
                transform.localRotation = Quaternion.identity;
                spriteRenderer.material.DisableKeyword("_BILLBOARD");

                if (input.playerInputs.MoveAxisRight != 0) {
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight > 0;
                }
                break;
        }

        // set mode and animation
        if (input.isCrouching || input.state == CharacterState.landStun) {
            spriteRenderer.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            if (input.isProne && !(input.state == CharacterState.wallPress || input.wallPressTimer > 0)) {
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }
        } else {
            spriteRenderer.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        }
        shadowCaster.localScale = new Vector3(0.25f, 0.8f, 0.25f);
        if (input.hitState == HitState.dead) {
            animator.Stop();
        } else if (input.state == CharacterState.superJump) {
            state = State.jump;
            spriteRenderer.sprite = skin.legsJump[direction][0];
        } else if (input.state == CharacterState.landStun) {
            spriteRenderer.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            shadowCaster.localScale = new Vector3(1f, 0.3f, 0.5f);
        } else if (input.isMoving) {
            if (input.isJumping) {
                state = State.jump;
            } else if (input.isClimbing) {
                state = State.climb;
            } else if (input.isRunning) {
                state = State.run;
            } else if (input.isCrouching) { // crawling
                state = State.crawl;
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.75f, 0f);
                shadowCaster.localScale = new Vector3(1f, 0.3f, 0.5f);
                shadowCaster.rotation = Quaternion.LookRotation(transform.right, transform.up);
            } else if (true) {
                state = State.walk;
            }

            if (state == State.crawl) {
                if (animator.clip != crawlAnimation) {
                    animator.clip = crawlAnimation;
                    animator.Play();
                }
            } else {
                if (input.velocity.magnitude < 2f) {
                    if (animator.clip != walkSlowAnimation) {
                        animator.clip = walkSlowAnimation;
                        animator.Play();
                    }
                } else {
                    if (animator.clip != walkAnimation) {
                        animator.clip = walkAnimation;
                        animator.Play();
                    }
                }
            }
        } else { // stopped
            if (input.isJumping) {
                state = State.jump;
                spriteRenderer.sprite = skin.legsJump[direction][0];
            } else if (input.isClimbing) {
                state = State.climb;
                spriteRenderer.sprite = skin.legsClimb[Direction.up][0];
            } else if (input.isCrouching || input.state == CharacterState.landStun) { // crouching or crawling
                state = State.crouch;
                if (!input.isProne) { // crouching
                    shadowCaster.localScale = new Vector3(0.5f, 0.4f, 0.5f);
                } else { // crawling
                    shadowCaster.localScale = new Vector3(1f, 0.3f, 0.5f);
                }
            } else {
                state = State.idle;
                spriteRenderer.sprite = skin.legsIdle[direction][0];
            }
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Play();
            }
        }
        shadowCaster.localPosition = new Vector3(0f, shadowCaster.localScale.y - 0.05f, 0f);
        spriteRenderer.transform.localPosition += offset;

        UpdateFrame(input);
        SpriteData torsoSpriteData = torsoAnimation.UpdateView(input);

        // set rotation to be coplanar with the camera plane
        Vector3 directionToCamera = input.directionToCamera;
        directionToCamera.y = 0f;
        Quaternion lookTowardCamera = Quaternion.LookRotation(-1f * directionToCamera, Vector3.up);
        transform.rotation = lookTowardCamera;

        if (spriteRenderer.flipX) {
            Vector3 headPosition = headAnimation.transform.localPosition;
            headPosition.x *= -1f;
            headAnimation.transform.localPosition = headPosition;
        }
        if (input.state == CharacterState.wallPress || input.wallPressTimer > 0f) {
            Vector3 headPosition = headAnimation.transform.localPosition;
            headPosition.x *= -1f;
            headAnimation.transform.localPosition = headPosition;
        }

        // record the rotated position
        Vector3 absoluteWorldPosition = headAnimation.transform.position;

        // set rotation back to identity.
        transform.localRotation = Quaternion.identity;

        // set position back to the rotated position.
        headAnimation.transform.position = absoluteWorldPosition;

        torsoAnimation.transform.position += 0.001f * directionToCamera;
        // TODO: this is a hack/patch.
        if (torsoSpriteData != null && torsoSpriteData.headInFrontOfTorso) {
            headAnimation.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 100;
            headAnimation.transform.position += 0.002f * input.directionToCamera;
        } else {
            headAnimation.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 100;
            headAnimation.transform.position -= 0.002f * input.directionToCamera;
        }
    }

    public void LoadSkinState(ISkinState state) {
        skin = Skin.LoadSkin(state.legSkin);
    }

    public void UpdateFrame(AnimationInput input) {
        Octet<Sprite[]> octet = null;
        octet = skin.GetCurrentLegsOctet(state, input);

        Sprite[] sprites = octet[direction];
        frame = Math.Min(frame, sprites.Length - 1);
        spriteRenderer.sprite = sprites[frame];
    }

}
