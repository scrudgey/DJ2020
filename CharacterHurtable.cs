using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HitState { normal, stun, unconscious, dead }

public class CharacterHurtable : Destructible {
    private HitState _hitState;
    public float wallDecalDistance = 1f;
    public float wallDecalProbability = 0.2f;
    public CharacterController controller;
    public LegsAnimation legsAnimation;
    Coroutine shakeRoutine;
    Transform transformCached;
    public override void Awake() {
        base.Awake();
        RegisterDamageCallback<BulletDamage>(TakeBulletDamage);
        RegisterDamageCallback<ExplosionDamage>(TakeExplosionDamage);
        transformCached = transform;
    }
    public DamageResult TakeBulletDamage(BulletDamage damage) {
        PlayerInput snapInput = new PlayerInput {
            lookAtPosition = damage.bullet.ray.origin,
            snapToLook = true
        };
        controller.SetInputs(snapInput);

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake(0.05f, 0.15f));

        CheckWallDecal(damage);

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

    protected override void ApplyDamageResult(DamageResult result) {
        base.ApplyDamageResult(result);
    }
    protected override void DoDestruct(Damage damage) {
        // TODO: destroy on different damage conditions
        hitState = HitState.dead;
    }

    public IEnumerator Shake(float intensity, float interval) {
        float blinkTimer = 0;
        while (blinkTimer < interval) {
            blinkTimer += Time.unscaledDeltaTime;
            legsAnimation.offset = UnityEngine.Random.insideUnitSphere * intensity;
            legsAnimation.scaleOffset = UnityEngine.Random.insideUnitSphere * intensity * 10f;
            yield return null;
        }
        legsAnimation.offset = Vector3.zero;
        legsAnimation.scaleOffset = Vector3.zero;
    }

    public void CheckWallDecal(BulletDamage damage) {
        if (UnityEngine.Random.Range(0f, 1f) > wallDecalProbability)
            return;
        Ray ray = new Ray(damage.position, damage.direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, wallDecalDistance, LayerUtil.GetMask(Layer.def, Layer.obj));
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.transform.IsChildOf(transform.root))
                continue;
            TagSystemData tagData = Toolbox.GetTagData(hit.collider.gameObject);
            if (tagData.bulletPassthrough) continue;
            GameObject decalObject = PoolManager.I.CreateDecal(hit, PoolManager.DecalType.blood);
            break;
        }
    }
}
