using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimation : MonoBehaviour, ISaveable {
    public SpriteRenderer spriteRenderer;
    public Skin skin;
    public Direction direction;
    private int frame;

    public void UpdateView(AnimationInput input, TorsoSpriteData torsoSpriteData) {
        // adjust visibility
        switch (input.state) {
            case CharacterState.wallPress:
                spriteRenderer.material.DisableKeyword("_BILLBOARD");
                if (input.isCrouching) { // crouching
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
                    if (input.playerInputs.MoveAxisRight < 0) {
                        direction = Direction.right;
                    } else if (input.playerInputs.MoveAxisRight > 0) {
                        direction = Direction.left;
                    }
                }
                break;
            case CharacterState.climbing:
            case CharacterState.superJump:
            case CharacterState.landStun:
            case CharacterState.jumpPrep:
            case CharacterState.normal:
                // TODO: adjust for aim mode.
                Vector3 headDirection = (input.targetData.position - transform.position).normalized;
                Vector2 headDir = new Vector2(headDirection.x, headDirection.z);
                float headAngle = Vector2.SignedAngle(input.camDir, headDir);
                Direction headOrientation = Toolbox.DirectionFromAngle(headAngle);
                direction = Toolbox.ClampDirection(headOrientation, input.orientation);
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material.DisableKeyword("_BILLBOARD");
                } else {
                    spriteRenderer.material.EnableKeyword("_BILLBOARD");
                }
                break;
            default:
                break;
        }

        if (torsoSpriteData.overrideHeadDirection) {
            direction = input.orientation;
        }

        spriteRenderer.flipX = direction == Direction.left || direction == Direction.leftUp || direction == Direction.leftDown;
        UpdateFrame(torsoSpriteData);
    }
    public void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.headIdle;
    }

    public void UpdateFrame(TorsoSpriteData torsoSpriteData) {
        if (skin != null) {
            Octet<Sprite[]> octet = skin.headIdle;
            if (torsoSpriteData.overrideHeadDirection) {
                spriteRenderer.sprite = skin.headSprites[torsoSpriteData.headSprite];
            } else {
                Sprite[] sprites = octet[direction];
                frame = Math.Min(frame, sprites.Length - 1);
                spriteRenderer.sprite = sprites[frame];
            }
        }
    }

    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.bodySkin);
    }

}
