using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public class TorsoAnimation : MonoBehaviour, ISaveable {
    private GunHandler.GunState state;
    private CharacterState characterState;
    private int _frame;
    private Direction direction;
    public HeadAnimation headAnimation;
    public SpriteRenderer spriteRenderer;
    public Animation animator;
    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    public AnimationClip crawlAnimation;
    public Skin skin;
    public float trailInterval = 0.05f;
    private bool bob;
    private AnimationInput lastInput;
    bool isMoving;
    bool isCrouching;
    bool isCrawling;
    GunType gunType;

    SpriteData ApplyTorsoSpriteData(AnimationInput input) {
        int sheetIndex = int.Parse(spriteRenderer.sprite.name.Split("_").Last());
        SpriteData[] torsoSpriteDatas = input.gunInput.gunType switch {
            GunType.unarmed => skin.unarmedSpriteData,
            GunType.pistol => skin.pistolSpriteData,
            GunType.smg => skin.smgSpriteData,
            GunType.rifle => input.isRunning ? skin.smgSpriteData : skin.rifleSpriteData,
            GunType.shotgun => input.isRunning ? skin.smgSpriteData : skin.shotgunSpriteData,
            _ => skin.unarmedSpriteData
        };
        if (isCrawling || input.hitState == HitState.dead) { // crawling
            torsoSpriteDatas = skin.unarmedSpriteData;
        }
        SpriteData torsoSpriteData = torsoSpriteDatas[sheetIndex];

        Vector3 offset = new Vector3(torsoSpriteData.headOffset.x / 100f, torsoSpriteData.headOffset.y / 100f, 0f);
        headAnimation.transform.localPosition = offset;
        headAnimation.UpdateView(input, torsoSpriteData);
        return torsoSpriteData;
    }
    void SetState(GunHandler.GunState newState) {
        state = newState;
        // Debug.Log("set gunstate: " + state);
    }
    void OnEnable() {
        animator.Play();
    }
    // used by animator
    public void SetFrame(int frame) {
        _frame = frame;
    }
    // used by animator
    public void SetBob(int bob) {
        this.bob = bob == 1;
    }
    public SpriteData UpdateView(AnimationInput input) {
        lastInput = input;

        switch (input.state) {
            case CharacterState.dead:
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
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight > 0;
                }
                break;
        }

        AnimationClip walkAnimation = unarmedWalkAnimation;

        direction = input.orientation;

        transform.localPosition = Vector3.zero;
        if (bob && !isCrawling) {
            transform.localPosition -= new Vector3(0f, 0.01f, 0f);
        }

        SetState(input.gunInput.gunState);
        characterState = input.state;

        isMoving = input.isMoving;
        isCrouching = input.isCrouching;
        if (!isCrawling && isMoving && isCrouching) {
            isCrawling = true;
        }
        if (isCrawling && (!isCrouching || input.wallPressTimer > 0 || input.state == CharacterState.wallPress)) {
            isCrawling = false;
        }
        gunType = input.gunInput.gunType;
        // Debug.Log($"hasgun: {input.gunInput.hasGun}");
        if (input.hitState == HitState.dead) {
            animator.Stop();
        } else if (input.gunInput.hasGun) {
            switch (state) {
                case GunHandler.GunState.shooting:
                    Debug.Log($"shoot branch: {input.gunInput.baseGun.shootAnimation}");
                    SetAnimation(input.gunInput.baseGun.shootAnimation, forcePlay: input.gunInput.shootRequestedThisFrame);
                    break;
                case GunHandler.GunState.reloading:
                    SetAnimation(input.gunInput.baseGun.reloadAnimation);
                    break;
                case GunHandler.GunState.racking:
                    SetAnimation(input.gunInput.baseGun.rackAnimation);
                    break;
                default:
                    if (input.isMoving) {
                        SetAnimation(walkAnimation);
                    } else {
                        _frame = 0;
                        SetAnimation(idleAnimation);
                    }
                    break;
            }
        } else {
            if (input.isMoving) {
                if (input.isCrouching) {
                    SetAnimation(crawlAnimation);
                } else {
                    SetAnimation(walkAnimation);
                }
            } else {
                _frame = 0;
                SetAnimation(idleAnimation);
            }
        }
        UpdateFrame(input);
        return ApplyTorsoSpriteData(input);
    }
    private void SetAnimation(AnimationClip clip, bool forcePlay = false) {
        if (forcePlay) {
            animator.Stop();
        }
        if (animator.clip != clip || forcePlay) {
            bob = false;
            animator.clip = clip;
            animator.Play();
            Debug.Log($"play animation: {clip}");
        }
    }
    public void UpdateFrame(AnimationInput input) {
        if (skin == null)
            return;

        Octet<Sprite[]> _sprites = skin.GetCurrentTorsoOctet(lastInput);
        // if (isCrouching) {
        //     if (isMoving) {
        //         _sprites = skin.unarmedCrawl;
        //     } else {
        //         if (isCrawling) {
        //             _sprites = skin.unarmedCrawl;
        //         } else {
        //             _sprites = skin.gunCrouchSprites(gunType);
        //         }
        //     }
        // }

        if (isCrawling) {
            _sprites = skin.unarmedCrawl;
        }
        if (input.hitState == HitState.dead) {
            _sprites = skin.unarmedDead;
        }
        if (_sprites == null)
            return;
        if (_sprites[direction] == null)
            return;

        int frame = Math.Min(_frame, _sprites[direction].Length - 1);
        if (_sprites[direction][frame] == null)
            return;
        spriteRenderer.sprite = _sprites[direction][frame];
    }
    public void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(Resources.Load("prefabs/fx/jumpTrail"), transform.position, transform.rotation) as GameObject;
        DirectionalBillboard billboard = trail.GetComponentInChildren<DirectionalBillboard>();
        billboard.skin = skin.GetCurrentTorsoOctet(lastInput);
        // TODO: set direction
        // billboard.direction = _direction;
    }
    public void LoadState(PlayerData data) {
        skin = Skin.LoadSkin(data.bodySkin);
    }
}
