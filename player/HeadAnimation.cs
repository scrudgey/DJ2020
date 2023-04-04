using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimation : MonoBehaviour, ISkinStateLoader {
    public Transform sensesTransform;
    public SpriteRenderer spriteRenderer;
    public Skin skin;
    public Direction direction;
    // private int frame;
    float timer;
    bool speakParity;
    float speakInterval = 0.1f;

    public void UpdateView(AnimationInput input, SpriteData torsoSpriteData) {
        timer += Time.deltaTime;
        if (timer > speakInterval) {
            timer -= speakInterval;
            speakParity = !speakParity;
            speakInterval = UnityEngine.Random.Range(0.05f, 0.15f);
        }
        if (sensesTransform != null && input.lookAtDirection != Vector3.zero)
            sensesTransform.forward = input.lookAtDirection;

        switch (input.state) {
            case CharacterState.wallPress:
                // TODO: should not belong to animation code
                transform.localRotation = Quaternion.identity;
                spriteRenderer.material.DisableKeyword("_BILLBOARD");
                if (input.isCrouching) { // crouching
                    if (direction == Direction.right || direction == Direction.down) {
                        direction = Direction.rightDown;
                    } else if (direction == Direction.left) {
                        direction = Direction.leftDown;
                    }
                    if (input.playerInputs.MoveAxisRight < 0) {
                        direction = Direction.rightDown;
                    } else if (input.playerInputs.MoveAxisRight > 0) {
                        direction = Direction.leftDown;
                    }
                } else { // standing
                    direction = Direction.down;
                    if (input.playerInputs.MoveAxisRight > 0) {
                        direction = Direction.left;
                    } else if (input.playerInputs.MoveAxisRight < 0) {
                        direction = Direction.right;
                    }
                }
                if (input.playerInputs.MoveAxisRight != 0) {
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight > 0;
                }
                break;
            case CharacterState.climbing:
            case CharacterState.superJump:
            case CharacterState.landStun:
            case CharacterState.jumpPrep:
            case CharacterState.normal:
            case CharacterState.popout:
            case CharacterState.aim:
                Vector3 lookDirection = input.lookAtDirection;
                lookDirection.y = 0;
                Vector2 headDir = new Vector2(lookDirection.x, lookDirection.z);
                float headAngle = Vector2.SignedAngle(input.camDir, headDir);
                Direction headOrientation = Toolbox.DirectionFromAngle(headAngle);
                direction = Toolbox.ClampDirection(headOrientation, input.orientation);
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material.DisableKeyword("_BILLBOARD");
                } else {
                    spriteRenderer.material.EnableKeyword("_BILLBOARD");
                }
                spriteRenderer.flipX = direction == Direction.left || direction == Direction.leftUp || direction == Direction.leftDown;
                break;
            default:
                break;
        }
        if (torsoSpriteData.overrideHeadDirection) {
            direction = input.orientation;
        }
        UpdateFrame(input, torsoSpriteData);
    }
    public void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.headIdle;
    }

    public void UpdateFrame(AnimationInput input, SpriteData torsoSpriteData) {
        if (skin != null) {
            Octet<Sprite[]> octet = skin.headIdle;
            if (torsoSpriteData.overrideHeadDirection) {
                int index = torsoSpriteData.headSprite;
                if (input.isSpeaking)
                    if (speakParity) index += 5;
                spriteRenderer.sprite = skin.headSprites[torsoSpriteData.headSprite];
            } else {
                Sprite[] sprites = octet[direction];
                // frame = Math.Min(frame, sprites.Length - 1);
                int frame = 0;
                if (input.isSpeaking)
                    frame = speakParity ? 0 : 1;
                spriteRenderer.sprite = sprites[frame];
            }
        }
    }
    public void LoadSkinState(ISkinState state) {
        skin = Skin.LoadSkin(state.headSkin);
    }
}
