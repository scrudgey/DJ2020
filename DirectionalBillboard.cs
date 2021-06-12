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

[Serializable]
public class Octet<T> {
    Dictionary<Direction, T> items;
    public T down;
    public T rightDown;
    public T right;
    public T rightUp;
    public T up;

    public T this[Direction key] {
        get {
            switch (key) {
                default:
                case Direction.left:
                    return right;       // sprite flipped
                case Direction.leftUp:
                    return rightUp;       // sprite flipped
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
                    return rightDown;       // sprite flipped
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
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public Octet<Sprite> idle;
    public Octet<Sprite[]> walk;
    public Octet<Sprite[]> crawl;
    public Octet<Sprite> crouch;
    private Sprite _idleSprite;
    private Sprite _crouchSprite;
    private Sprite[] _walkSprites;
    private Sprite[] _crawlSprites;

    // used by animation
    public void SetFrame(int frame) {
        // Debug.Log($"{mode} {frame}");
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

    public void UpdateView(AnimationInput input) {

        if (input.wallPressTimer > 0 && !input.wallPress) {
            spriteRenderer.material = flatMaterial;
        } else if (input.wallPress) {
            spriteRenderer.material = billboardMaterial;
        } else {
            spriteRenderer.material = billboardMaterial;
        }

        spriteRenderer.flipX = input.direction == Direction.left || input.direction == Direction.leftUp || input.direction == Direction.leftDown;

        _idleSprite = idle[input.direction];
        _walkSprites = walk[input.direction];
        _crawlSprites = crawl[input.direction];
        _crouchSprite = crouch[input.direction];

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
                spriteRenderer.sprite = _crouchSprite;

            } else {
                mode = Mode.idle;
                spriteRenderer.sprite = _idleSprite;
            }
            if (animator.clip != idleAnimation) {
                animator.clip = idleAnimation;
                animator.Stop();
            }
        }

    }
}
