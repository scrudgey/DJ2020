using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HitState { normal, hitstun, dead, invulnerable }

public class CharacterHurtable : Destructible, IBindable<CharacterHurtable>, IPoolable, ICharacterHurtableStateLoader {
    public enum HitStunType { timer, invulnerable }
    public HitStunType hitstunType;
    // private HitState _hitState;
    public float wallDecalDistance = 1f;
    public float wallDecalProbability = 0.2f;
    public CharacterController controller;
    private float hitstunCountdown;
    public float hitstunAmount = 0.1f;
    public Action<CharacterHurtable> OnValueChanged { get; set; }
    public Action<Damage> OnDamageTaken;
    public override void Awake() {
        base.Awake();
        RegisterDamageCallback<BulletDamage>(TakeBulletDamage);
        RegisterDamageCallback<ExplosionDamage>(TakeExplosionDamage);
    }
    public override DamageResult TakeDamage(Damage damage) {
        DamageResult result = DamageResult.NONE;
        if (hitState != HitState.invulnerable) {
            result = base.TakeDamage(damage);
        }
        OnValueChanged?.Invoke(this);
        OnDamageTaken?.Invoke(damage);
        return result;
    }
    public DamageResult TakeBulletDamage(BulletDamage damage) {
        PlayerInput snapInput = new PlayerInput {
            lookAtPosition = damage.bullet.ray.origin,
            snapToLook = true,
            Fire = PlayerInput.FireInputs.none
        };
        controller.SetInputs(snapInput);

        CheckWallDecal(damage);

        if (hitstunType == HitStunType.timer) {
            hitstunCountdown = hitstunAmount;
            hitState = Toolbox.Max(hitState, HitState.hitstun);
        } else if (hitstunType == HitStunType.invulnerable) {
            hitstunCountdown = hitstunAmount * 2f;
            hitState = Toolbox.Max(hitState, HitState.invulnerable);
        }

        if (!transform.IsChildOf(GameManager.I.playerObject.transform)) {
            GameObject impactPrefab = Resources.Load("prefabs/bulletImpact") as GameObject;
            GameObject obj = GameObject.Instantiate(impactPrefab, damage.position, Quaternion.identity);
            BulletImpact impact = obj.GetComponent<BulletImpact>();
            impact.damage = damage;
            Destroy(obj, 0.1f);

            NoiseData noise = new NoiseData {
                player = false,
                suspiciousness = Suspiciousness.suspicious,
                volume = 5,
                isFootsteps = false
            };
            Toolbox.Noise(transform.position, noise, transform.root.gameObject);
        }

        return new DamageResult {
            damageAmount = damage.amount,
            damage = damage
        };
    }
    public DamageResult TakeExplosionDamage(ExplosionDamage explosion) {
        return new DamageResult {
            damageAmount = explosion.amount,
            damage = explosion
        };
    }
    void Update() {
        if (hitstunCountdown > 0f) {
            hitstunCountdown -= Time.deltaTime;
            if (hitstunCountdown <= 0) {
                if (hitState == HitState.hitstun || hitState == HitState.invulnerable) {
                    hitState = HitState.normal;
                }
            }
        }
    }

    protected override void ApplyDamageResult(DamageResult result) {
        base.ApplyDamageResult(result);
    }
    protected override void DoDestruct(Damage damage) {
        hitState = HitState.dead;

        // TODO: this is all pretty much a bad hack
        if (damage is ExplosionDamage) {
            PoolManager.I.GetPool(gameObject).RecallObject(transform.parent.gameObject);
            CharacterController controller = transform.root.GetComponentInChildren<CharacterController>();
            controller.OnCharacterDead?.Invoke(controller);
        }
    }

    public void CheckWallDecal(BulletDamage damage) {
        if (UnityEngine.Random.Range(0f, 1f) > wallDecalProbability)
            return;
        Ray ray = new Ray(damage.position, damage.direction);
        // TODO: nonalloc
        RaycastHit[] hits = Physics.RaycastAll(ray, wallDecalDistance, LayerUtil.GetLayerMask(Layer.def, Layer.obj));
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.transform.IsChildOf(transform.root))
                continue;
            TagSystemData tagData = Toolbox.GetTagData(hit.collider.gameObject);
            if (tagData.bulletPassthrough) continue;
            GameObject decalObject = PoolManager.I.CreateDecal(hit, PoolManager.DecalType.blood);
            break;
        }
    }
    public override void OnPoolDectivate() {
        base.OnPoolDectivate();
        hitstunCountdown = 0;
    }
    public void LoadCharacterState(ICharacterHurtableState state) {
        this.health = state.health;
        this.fullHealthAmount = state.fullHealthAmount;
        this.hitState = state.hitState;
    }
}
