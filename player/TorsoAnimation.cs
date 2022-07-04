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
    private Direction _direction;
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

    void Start() {
        // TODO: fix
        // Bind(target.gameObject);
    }
    // override public void HandleValueChanged(CharacterController controller) {
    //     AnimationInput input = controller.BuildAnimationInput();
    //     UpdateView(input);
    //     // TODO: handle case when running with rifle or shotgun
    //     // ApplyTorsoSpriteData(input);
    // }
    void ApplyTorsoSpriteData(AnimationInput input) {
        if (input.movementSticking)
            return;

        int sheetIndex = int.Parse(spriteRenderer.sprite.name.Split("_").Last());
        SpriteData[] torsoSpriteDatas = input.gunInput.gunType switch {
            GunType.unarmed => skin.unarmedSpriteData,
            GunType.pistol => skin.pistolSpriteData,
            GunType.smg => skin.smgSpriteData,
            GunType.rifle => input.isRunning ? skin.smgSpriteData : skin.rifleSpriteData,
            GunType.shotgun => input.isRunning ? skin.smgSpriteData : skin.shotgunSpriteData,
            _ => skin.unarmedSpriteData
        };
        if (input.isCrouching && input.isMoving) { // crawling
            torsoSpriteDatas = skin.unarmedSpriteData;
        }
        SpriteData torsoSpriteData = torsoSpriteDatas[sheetIndex];

        headAnimation.UpdateView(input, torsoSpriteData);
        Vector3 offset = new Vector3(torsoSpriteData.headOffset.x / 100f, torsoSpriteData.headOffset.y / 100f, 0f);
        if (headAnimation.spriteRenderer.flipX) {
            offset.x *= -1f;
        }
        headAnimation.transform.localPosition = offset;
        if (torsoSpriteData.headInFrontOfTorso) {
            headAnimation.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 100;
        } else {
            headAnimation.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 100;
        }
    }
    void SetState(GunHandler.GunState newState) {
        state = newState;
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
    public void UpdateView(AnimationInput input) {
        lastInput = input;

        if (input.movementSticking)
            return;

        switch (input.state) {
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

        AnimationClip walkAnimation = unarmedWalkAnimation;

        _direction = input.orientation;

        transform.localPosition = Vector3.zero;
        if (bob && !isCrawling) {
            transform.localPosition -= new Vector3(0f, 0.02f, 0f);
        }

        SetState(input.gunInput.gunState);
        characterState = input.state;

        isMoving = input.isMoving;
        isCrouching = input.isCrouching;
        if (!isCrawling && isMoving && isCrouching) {
            isCrawling = true;
        }
        if (isCrawling && !isCrouching) {
            isCrawling = false;
        }
        gunType = input.gunInput.gunType;
        if (input.gunInput.hasGun) {
            switch (state) {
                case GunHandler.GunState.shooting:
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
        UpdateFrame();
        ApplyTorsoSpriteData(input);
    }
    private void SetAnimation(AnimationClip clip, bool forcePlay = false) {
        if (forcePlay) {
            animator.Stop();
        }
        if (animator.clip != clip || forcePlay) {
            bob = false;
            animator.clip = clip;
            animator.Play();
        }
    }
    public void UpdateFrame() {
        if (skin == null)
            return;

        Octet<Sprite[]> _sprites = skin.GetCurrentTorsoOctet(lastInput);
        if (isCrouching) {
            if (isMoving) {
                _sprites = skin.unarmedCrawl;
            } else {
                if (isCrawling) {
                    _sprites = skin.unarmedCrawl;
                } else {
                    _sprites = skin.gunCrouchSprites(gunType);
                }
            }
        }

        if (_sprites == null)
            return;
        if (_sprites[_direction] == null)
            return;

        int frame = Math.Min(_frame, _sprites[_direction].Length - 1);
        if (_sprites[_direction][frame] == null)
            return;
        spriteRenderer.sprite = _sprites[_direction][frame];
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
