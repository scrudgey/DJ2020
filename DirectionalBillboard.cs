using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum Direction { left, leftUp, up, rightUp, right, rightDown, down, leftDown }

[Serializable]
public class Octet<T> {
    Dictionary<Direction, T> items;
    public T right;
    public T rightUp;
    public T up;
    public T leftUp;
    public T left;
    public T leftDown;
    public T down;
    public T rightDown;

    public T this[Direction key] {
        get {
            switch (key) {
                default:
                case Direction.left:
                    return left;
                case Direction.leftUp:
                    return leftUp;
                case Direction.up:
                    return up;
                case Direction.rightUp:
                    return rightUp;
                case Direction.right:
                    return right;
                case Direction.rightDown:
                    return rightDown;
                case Direction.down:
                    return down;
                case Direction.leftDown:
                    return leftDown;
            }
        }
    }
}

public class DirectionalBillboard : MonoBehaviour {
    enum Mode { idle, walk, crawl, crouch }
    Mode mode;
    public SpriteRenderer spriteRenderer;
    public Material billboardMaterial;
    public Material flatMaterial;
    // public SpriteRenderer gunRenderer;
    public GunAnimation gunAnimation;
    public NeoCharacterController controller;
    public NeoCharacterCamera cam;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public Octet<Sprite> idle;
    public Octet<Sprite[]> walk;
    public Octet<Sprite[]> crawl;
    public Octet<Sprite> crouch;
    // public Octet<Sprite> gun;
    // public Octet<Sprite[]> gunShoot;

    private Sprite _idleSprite;
    private Sprite _crouchSprite;
    private Sprite[] _walkSprites;
    private Sprite[] _crawlSprites;

    bool IsMoving() {
        return controller.Motor.Velocity.magnitude > 0.1;
    }
    void Update() {

        if (controller.wallPressTimer > 0 && !controller.wallPress) {
            spriteRenderer.material = flatMaterial;
            gunAnimation.spriteRenderer.material = flatMaterial;
        } else if (controller.wallPress) {
            spriteRenderer.material = billboardMaterial;
            gunAnimation.spriteRenderer.material = billboardMaterial;
        } else {
            spriteRenderer.material = billboardMaterial;
            gunAnimation.spriteRenderer.material = billboardMaterial;
        }

        Vector2 camDir = new Vector2(cam.PlanarDirection.x, cam.PlanarDirection.z);
        Vector2 playerDir = new Vector2(controller.direction.x, controller.direction.z);
        float angle = Vector2.SignedAngle(camDir, playerDir);

        if (angle < 22.5 && angle >= -22.5) {
            // up
            _idleSprite = idle[Direction.up];
            _walkSprites = walk[Direction.up];
            gunAnimation.SetDirection(Direction.up);
        } else if (angle >= 22.5 && angle < 67.5) {
            // left up
            _idleSprite = idle[Direction.leftUp];
            _walkSprites = walk[Direction.leftUp];
            _crawlSprites = crawl[Direction.leftUp];
            _crouchSprite = crouch[Direction.leftUp];
            gunAnimation.SetDirection(Direction.leftUp);

        } else if (angle >= 67.5 && angle < 112.5) {
            // left
            _idleSprite = idle[Direction.left];
            _walkSprites = walk[Direction.left];
            gunAnimation.SetDirection(Direction.left);
        } else if (angle >= 112.5 && angle < 157.5) {
            // left down
            _idleSprite = idle[Direction.leftDown];
            _walkSprites = walk[Direction.leftDown];
            _crawlSprites = crawl[Direction.leftDown];
            _crouchSprite = crouch[Direction.leftDown];
            gunAnimation.SetDirection(Direction.leftDown);
        } else if (angle > 157.5 || angle < -157.5) {
            // down
            _idleSprite = idle[Direction.down];
            _walkSprites = walk[Direction.down];
            gunAnimation.SetDirection(Direction.down);
        } else if (angle >= -157.5 && angle < -112.5) {
            // right down
            _idleSprite = idle[Direction.rightDown];
            _walkSprites = walk[Direction.rightDown];

            _crawlSprites = crawl[Direction.rightDown];
            _crouchSprite = crouch[Direction.rightDown];

            gunAnimation.SetDirection(Direction.rightDown);
        } else if (angle >= -112.5 && angle < -67.5) {
            // right
            _idleSprite = idle[Direction.right];
            _walkSprites = walk[Direction.right];
            gunAnimation.SetDirection(Direction.right);
        } else if (angle >= -67.5 && angle < -22.5) {
            // right up
            _idleSprite = idle[Direction.rightUp];
            _walkSprites = walk[Direction.rightUp];
            _crawlSprites = crawl[Direction.rightUp];
            _crouchSprite = crouch[Direction.rightUp];
            gunAnimation.SetDirection(Direction.rightUp);
        }

        if (IsMoving()) { // walking
            if (controller.isCrouching) {
                mode = Mode.crawl;
                gunAnimation.gameObject.SetActive(false);
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            } else {
                mode = Mode.walk;
                gunAnimation.gameObject.SetActive(true);
                spriteRenderer.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }
            if (animator.clip != walkAnimation) {
                animator.clip = walkAnimation;
                animator.Play();
            }
        } else {    // idle
            spriteRenderer.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            if (controller.isCrouching) {
                gunAnimation.gameObject.SetActive(false);
                mode = Mode.crouch;
            } else {
                gunAnimation.gameObject.SetActive(true);
                mode = Mode.idle;
            }
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Stop();
            }
        }
        UpdateSprite();
    }
    public void UpdateSprite() {
        if (!IsMoving()) {
            if (controller.isCrouching) {
                spriteRenderer.sprite = _crouchSprite;
            } else {
                spriteRenderer.sprite = _idleSprite;
            }
        }
    }

    public void SetFrame(int frame) {
        switch (mode) {
            case Mode.walk:
                spriteRenderer.sprite = _walkSprites[frame];
                break;
            case Mode.crawl:
                spriteRenderer.sprite = _crawlSprites[frame];
                break;
            case Mode.crouch:
                spriteRenderer.sprite = _crouchSprite;
                break;
            default:
            case Mode.idle:
                spriteRenderer.sprite = _idleSprite;
                break;
        }
    }
}
