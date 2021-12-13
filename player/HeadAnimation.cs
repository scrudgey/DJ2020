using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimation : MonoBehaviour, ISaveable {
    public SpriteRenderer spriteRenderer;
    public Skin skin;
    public Direction direction;
    private int frame;
    public void UpdateView(AnimationInput input) {
        direction = Toolbox.ClampDirection(input.headOrientation, input.orientation);
        // adjust visibility
        switch (input.state) {
            case CharacterState.wallPress:
                spriteRenderer.material.DisableKeyword("_BILLBOARD");
                direction = Direction.down;
                if (input.playerInputs.MoveAxisRight > 0) {
                    direction = Direction.right;
                    // spriteRenderer.flipX = input.playerInputs.MoveAxisRight < 0;
                } else if (input.playerInputs.MoveAxisRight < 0) {
                    direction = Direction.left;

                }
                break;
            case CharacterState.climbing:
                spriteRenderer.enabled = false;
                break;
            case CharacterState.normal:
                if (input.isMoving && (input.isCrouching || input.isRunning)) {
                    spriteRenderer.enabled = false;
                } else {
                    spriteRenderer.enabled = true;
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

    public void UpdateFrame() {
        Octet<Sprite[]> octet = skin.headIdle;
        Sprite[] sprites = octet[direction];
        frame = Math.Min(frame, sprites.Length - 1);
        spriteRenderer.sprite = sprites[frame];
    }

    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.bodySkin);
    }

}
