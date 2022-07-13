using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHurtable : Destructible {
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

    public IEnumerator Shake(float intensity, float interval) {
        float blinkTimer = 0;
        while (blinkTimer < interval) {
            blinkTimer += Time.unscaledDeltaTime;
            legsAnimation.offset = Random.insideUnitSphere * intensity;
            legsAnimation.scaleOffset = Random.insideUnitSphere * intensity * 10f;
            yield return null;
        }
        legsAnimation.offset = Vector3.zero;
        legsAnimation.scaleOffset = Vector3.zero;
    }
}
