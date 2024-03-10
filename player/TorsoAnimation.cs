using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using KinematicCharacterController;
using UnityEngine;
public class TorsoAnimation : MonoBehaviour, ISkinStateLoader {
    private GunHandler.GunStateEnum state;
    private CharacterState characterState;
    private int _frame;
    private Direction direction;
    public HeadAnimation headAnimation;
    public SpriteRenderer spriteRenderer;
    public RocketLauncher rocketLauncher;
    // public Animation animator;
    public CustomAnimator animator;
    public AnimationClip idleAnimation;
    public AnimationClip unarmedWalkAnimation;
    public AnimationClip unarmedWalkSlowAnimation;
    public AnimationClip crawlAnimation;
    public AnimationClip unholsterAnimation;
    public AnimationClip holsterAnimation;
    [Header("sword")]
    public AnimationClip swordSwingAnimation;
    public AnimationClip swordUnholsterAnimation;
    public AnimationClip swordHolsterAnimation;
    public Skin skin;
    public float trailInterval = 0.05f;
    private bool bob;
    private AnimationInput lastInput;
    bool isMoving;
    bool isCrouching;
    GunType gunType;


    void SetState(GunHandler.GunStateEnum newState) {
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
        spriteRenderer.color = input.lightProbeColor;

        switch (input.state) {
            case CharacterState.dead:
                break;
            default:
            case CharacterState.popout:
            case CharacterState.normal:
                if (input.wallPressTimer > 0) {
                    spriteRenderer.material.DisableKeyword("_BILLBOARD");
                } else {
                    spriteRenderer.material.EnableKeyword("_BILLBOARD");
                }
                spriteRenderer.flipX = input.orientation == Direction.left || input.orientation == Direction.leftUp || input.orientation == Direction.leftDown;
                break;
            case CharacterState.wallPress:
                transform.localRotation = Quaternion.identity;
                spriteRenderer.material.DisableKeyword("_BILLBOARD");
                if (input.playerInputs.MoveAxisRight != 0) {
                    spriteRenderer.flipX = input.playerInputs.MoveAxisRight > 0;
                }
                break;
        }

        AnimationClip walkAnimation = (input.velocity.magnitude < 2f) ? unarmedWalkSlowAnimation : unarmedWalkAnimation;

        direction = input.orientation;

        transform.localPosition = Vector3.zero;
        if (bob && !input.isProne) {
            transform.localPosition -= new Vector3(0f, 0.02f, 0f); // TODO: ?
        }

        SetState(input.gunInput.gunState);
        characterState = input.state;

        isMoving = input.isMoving;
        isCrouching = input.isCrouching;
        if (!input.isProne && isMoving && isCrouching) {
            input.isProne = true;
        }
        if (input.isProne && (!isCrouching || input.wallPressTimer > 0 || input.state == CharacterState.wallPress)) {
            input.isProne = false;
        }
        gunType = input.gunInput.gunType;
        if (input.hitState == HitState.dead) {
            animator.Stop();
        } else if (input.gunInput.gunType == GunType.sword || input.gunInput.fromGunType == GunType.sword) {
            switch (state) {
                case GunHandler.GunStateEnum.holstering:
                    if (input.gunInput.toGunType == GunType.unarmed) {
                        Debug.Log("set animation sword holster");
                        SetAnimation(swordHolsterAnimation);
                    } else {
                        Debug.Log("set animation sword unholster");
                        SetAnimation(swordUnholsterAnimation);
                    }
                    break;
                case GunHandler.GunStateEnum.shooting:
                    SetAnimation(swordSwingAnimation);
                    break;
                default:
                    _frame = 0;
                    SetAnimation(idleAnimation);
                    break;
            }
        } else if (input.gunInput.hasGun) {
            switch (state) {
                case GunHandler.GunStateEnum.holstering:
                    if (input.gunInput.toGunType == GunType.unarmed) {
                        SetAnimation(holsterAnimation);
                    } else {
                        SetAnimation(unholsterAnimation);
                    }
                    animator.playbackSpeed = 0.8f;
                    break;
                case GunHandler.GunStateEnum.shooting:
                    SetAnimation(input.gunInput.baseGun.shootAnimation, forcePlay: input.gunInput.shootRequestedThisFrame);
                    animator.playbackSpeed = input.gunInput.baseGun.cycle switch {
                        CycleType.automatic => 0.1f / input.gunInput.baseGun.GetGunStats().shootInterval,
                        _ => 1f
                    };
                    // Debug.Log($"speed: {animator.playbackSpeed}");
                    break;
                case GunHandler.GunStateEnum.reloading:
                    SetAnimation(input.gunInput.baseGun.reloadAnimation);
                    break;
                case GunHandler.GunStateEnum.racking:
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
            if (input.armsRaised) {
                _frame = 0;
                SetAnimation(idleAnimation);
            } else if (input.isMoving) {
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
        SetItemSprite(input);
        return ApplyTorsoSpriteData(input);
    }
    SpriteData ApplyTorsoSpriteData(AnimationInput input) {
        // int sheetIndex = int.Parse(spriteRenderer.sprite.name.Split("_").Last());
        // SpriteData[] torsoSpriteDatas = input.gunInput.gunType switch {
        //     GunType.unarmed => skin.unarmedSpriteData,
        //     GunType.pistol => skin.pistolSpriteData,
        //     GunType.smg => skin.smgSpriteData,
        //     GunType.rifle => input.isRunning ? skin.smgSpriteData : skin.rifleSpriteData,
        //     GunType.shotgun => input.isRunning ? skin.smgSpriteData : skin.shotgunSpriteData,
        //     _ => skin.unarmedSpriteData
        // };
        // if (input.isProne || input.hitState == HitState.dead || input.wavingArm || input.isJumping) { // crawling
        //     torsoSpriteDatas = skin.unarmedSpriteData;
        // }
        try {
            // SpriteData torsoSpriteData = torsoSpriteDatas[sheetIndex];
            SpriteData spriteData = skin.allSpriteData[spriteRenderer.sprite.name];
            Vector3 headOffset = new Vector3(spriteData.headOffset.x / 100f, spriteData.headOffset.y / 100f, 0f);
            headAnimation.transform.localPosition = headOffset;
            headAnimation.UpdateView(input, spriteData);
            return spriteData;
        }
        catch (Exception) {
            Debug.LogError($"**** name:{spriteRenderer.sprite.name} prone:{input.isProne} dead:{input.hitState == HitState.dead} guntype:{input.gunInput.gunType}");
            return null;
        }
    }

    void SetItemSprite(AnimationInput input) {
        switch (input.activeItem) {
            case RocketLauncherItem rocketItem:
                rocketLauncher.spritesheet = rocketItem.rocketData.spritesheet;
                rocketLauncher.gameObject.SetActive(true);
                rocketLauncher?.SetSprite(direction);
                break;
            default:
                if (rocketLauncher != null)
                    rocketLauncher.gameObject.SetActive(false);
                break;
        }
    }

    private void SetAnimation(AnimationClip clip, bool forcePlay = false) {
        // Debug.Log($"{transform.root.gameObject} setting animation: {clip}");
        animator.playbackSpeed = 1f;
        if (forcePlay) {
            animator.Stop();
        }
        if (animator.clip != clip || forcePlay) {
            bob = false;
            animator.clip = clip;
            animator.Play();
        }
    }
    public void UpdateFrame(AnimationInput input) {
        if (skin == null)
            return;

        Octet<Sprite[]> _sprites = skin.GetCurrentTorsoOctet(input);

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
    public void LoadSkinState(ISkinState state) {
        skin = Skin.LoadSkin(state.bodySkin);
    }
}
