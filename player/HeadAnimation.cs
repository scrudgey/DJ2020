using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimation : IBinder<CharacterController>, ISaveable {
    // public CharacterController target { get; set; }
    public SpriteRenderer spriteRenderer;
    public Skin skin;
    public Direction direction;
    private int frame;

    void Start() {
        Bind(target.gameObject);
    }
    override public void HandleValueChanged(CharacterController controller) {
        AnimationInput input = controller.BuildAnimationInput();
        UpdateView(input);
    }
    public void UpdateView(AnimationInput input) {
        // adjust visibility
        switch (input.state) {
            case CharacterState.wallPress:
                spriteRenderer.material.DisableKeyword("_BILLBOARD");
                if (input.isMoving && input.isCrouching) { // crawling
                    spriteRenderer.enabled = false;
                } else if (input.isCrouching) { // crouching
                    if (input.gunInput.hasGun) {
                        spriteRenderer.enabled = true;
                    } else {
                        spriteRenderer.enabled = false;
                    }
                    if (direction == Direction.right || direction == Direction.down) {
                        direction = Direction.rightDown;
                    } else if (direction == Direction.left) {
                        direction = Direction.leftDown;
                    }
                    if (input.playerInputs.MoveAxisRight > 0) {
                        direction = Direction.rightDown;
                    } else if (input.playerInputs.MoveAxisRight < 0) {
                        direction = Direction.leftDown;
                    }
                } else { // standing
                    direction = Direction.down;
                    spriteRenderer.enabled = true;
                    if (input.playerInputs.MoveAxisRight < 0) {
                        direction = Direction.right;
                    } else if (input.playerInputs.MoveAxisRight > 0) {
                        direction = Direction.left;
                    }
                }
                break;
            case CharacterState.climbing:
                spriteRenderer.enabled = false;
                break;
            case CharacterState.superJump:
                spriteRenderer.enabled = true;
                break;
            case CharacterState.landStun:
            case CharacterState.jumpPrep:
                if (input.gunInput.hasGun) {
                    spriteRenderer.enabled = true;
                } else {
                    spriteRenderer.enabled = false;
                }
                break;
            case CharacterState.normal:
                Vector3 headDirection = (input.targetData.targetPoint(transform.position) - transform.position).normalized;
                Vector2 headDir = new Vector2(headDirection.x, headDirection.z);
                float headAngle = Vector2.SignedAngle(input.camDir, headDir);
                Direction headOrientation = Toolbox.DirectionFromAngle(headAngle);
                direction = Toolbox.ClampDirection(headOrientation, input.orientation);
                if (input.isMoving) {
                    if (input.isCrouching) {
                        spriteRenderer.enabled = false;
                    } else if (input.isRunning) {
                        // if unarmed, disabled
                        // if armed and idle, disabled
                        // if armed and not idle, enable
                        if (input.gunInput.hasGun) {
                            if (input.gunInput.gunState == GunHandler.GunState.idle) {
                                spriteRenderer.enabled = false;
                            } else {
                                spriteRenderer.enabled = true;
                            }
                        } else {
                            spriteRenderer.enabled = false;
                        }
                    } else {
                        spriteRenderer.enabled = true;
                    }
                } else {
                    // not moving
                    if (input.isCrouching) {
                        // if unarmed, disable
                        // if armed, enable
                        if (input.gunInput.hasGun) {
                            spriteRenderer.enabled = true;
                        } else {
                            spriteRenderer.enabled = false;
                        }
                    } else {
                        spriteRenderer.enabled = true;
                    }
                }
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material.DisableKeyword("_BILLBOARD");
                } else {
                    spriteRenderer.material.EnableKeyword("_BILLBOARD");
                }
                break;
            default:
                spriteRenderer.enabled = true;
                break;
        }

        spriteRenderer.flipX = direction == Direction.left || direction == Direction.leftUp || direction == Direction.leftDown;
        UpdateFrame();
    }
    public void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.headIdle;
    }

    public void UpdateFrame() {
        if (skin != null) {
            Octet<Sprite[]> octet = skin.headIdle;
            Sprite[] sprites = octet[direction];
            frame = Math.Min(frame, sprites.Length - 1);
            spriteRenderer.sprite = sprites[frame];
        }
    }

    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.bodySkin);
    }

}
