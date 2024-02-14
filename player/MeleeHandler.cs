using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

public class MeleeHandler : MonoBehaviour {
    public AudioSource audioSource;
    // public WeaponState weaponState;
    public MeleeWeaponTemplate meleeWeapon;
    public GameObject swordImpactPrefab;
    PrefabPool swordImpactPool;
    public GunHandler.GunStateEnum state;
    public Transform swordLine;
    public ParticleSystem swordParticles;
    public LineRenderer swordLineRenderer;
    public Collider swordTrigger;
    GunType fromGunType;
    GunType toGunType;
    bool swingRequestedThisFrame;
    Coroutine swingRoutine;
    void Start() {
        swordImpactPool = PoolManager.I.GetPool(swordImpactPrefab);
        ShowSwordLine(false);
    }
    public void Holster() {
        if (meleeWeapon == null) return;
        if (swingRoutine != null) {
            StopCoroutine(swingRoutine);
            ShowSwordLine(false);
        }
        state = GunHandler.GunStateEnum.holstering;

        fromGunType = GunType.sword;
        toGunType = GunType.unarmed;

        Toolbox.RandomizeOneShot(audioSource, meleeWeapon.swordHolsterSound);

        meleeWeapon = null;
        // fromGunType = gunInstance == null ? GunType.unarmed : gunInstance.template.type;
        // toGunType = GunType.unarmed;
        // isSwitchingWeapon = true;
        // state = GunStateEnum.holstering;

        // gunInstance = null;
        // OnValueChanged?.Invoke(this);
        // if (isPlayerCharacter) {
        //     GameManager.I.RemoveSuspicionRecord(SuspicionRecord.brandishingSuspicion());
        // }
        // Debug.Break();
    }
    public void StopHolster() {
        state = GunHandler.GunStateEnum.idle;
        // OnValueChanged?.Invoke(this);
        // OnHolsterFinish?.Invoke(this);
    }
    public void EndSwing() {
        state = GunHandler.GunStateEnum.idle;
    }

    public void SetInputs(PlayerInput input, Vector3 direction) {
        if (meleeWeapon == null) return;
        if (input.Fire.FirePressed) {
            Toolbox.RandomizeOneShot(audioSource, meleeWeapon.swordSwingSound);
            state = GunHandler.GunStateEnum.shooting;
            DoSwingLine(direction);
        }
        // Debug.DrawRay(transform.position, direction, Color.white);
    }
    public void SwitchWeapon(WeaponState weaponState) {
        if (weaponState.type == WeaponType.gun)
            return;
        if (weaponState == null || weaponState.meleeWeapon == this.meleeWeapon)
            return;
        // fromGunType = gunInstance == null ? GunType.unarmed : gunInstance.template.type;
        // toGunType = instance == null ? GunType.unarmed : instance.template.type;
        // isSwitchingWeapon = true;
        state = GunHandler.GunStateEnum.holstering;
        // this.weaponState = weaponState;
        meleeWeapon = weaponState.meleeWeapon;

        fromGunType = GunType.unarmed;
        toGunType = GunType.sword;
        Toolbox.RandomizeOneShot(audioSource, meleeWeapon.swordUnholsterSound);

        // gunInstance = instance;

        // SetGunAppearanceSuspicion();
        // OnValueChanged?.Invoke(this);
        // OnHolsterFinish?.Invoke(this);
    }

    public AnimationInput.GunAnimationInput BuildAnimationInput() {
        GunType gunType = GunType.unarmed;
        if (meleeWeapon != null) {
            gunType = GunType.sword;
        }
        return new AnimationInput.GunAnimationInput {
            gunType = gunType,
            gunState = state,
            hasGun = meleeWeapon != null || state == GunHandler.GunStateEnum.holstering,
            holstered = meleeWeapon == null,
            baseGun = null,
            shootRequestedThisFrame = swingRequestedThisFrame,
            aimWeapon = false,
            fromGunType = fromGunType,
            toGunType = toGunType,
        };
    }
    void DoSwingLine(Vector3 direction) {
        if (swingRoutine != null) {
            StopCoroutine(swingRoutine);
        }
        swingRoutine = StartCoroutine(SwingSword(direction));
    }
    IEnumerator SwingSword(Vector3 direction) {
        ShowSwordLine(true);
        Quaternion forward = Quaternion.LookRotation(direction, Vector3.up);
        // Debug.Log($"swinging at {direction}");
        float parity = Random.Range(0f, 1f) < 0.5f ? 1f : -1f;

        return Toolbox.ChainCoroutines(
            Toolbox.Ease(null, 0.08f, parity * -90, parity * 45, PennerDoubleAnimation.Linear, (amount) => {
                Quaternion rotation = Quaternion.Euler(0, amount, 0);
                swordLine.rotation = forward * rotation;
            }),
            Toolbox.CoroutineFunc(() => {
                ShowSwordLine(false);
            })
        );
    }

    void ShowSwordLine(bool value) {
        swordLineRenderer.enabled = value;
        if (value) {
            swordParticles.Play();
        } else {
            swordParticles.Stop();
        }
        swordTrigger.enabled = value;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.transform.IsChildOf(transform)) return;
        if (other.isTrigger) return;
        Impact(other);
    }

    void Impact(Collider other) {
        if (swingRoutine != null) {
            StopCoroutine(swingRoutine);
        }
        Vector3 collisionPoint = other.ClosestPoint(transform.position);
        Debug.Log($"sword impact: {other.tag}");

        MeleeDamage damage = new MeleeDamage(15f, transform.position, collisionPoint);

        DamageResult result = DamageResult.NONE;
        foreach (IDamageReceiver receiver in other.transform.GetComponentsInChildren<IDamageReceiver>()) {
            if (receiver is Damageable damageable) {
                result = result.Add(damageable.TakeDamage(damage));
            } else {
                receiver.TakeDamage(damage);
            }
        }

        if (other.CompareTag("actor")) {
            Toolbox.RandomizeOneShot(audioSource, meleeWeapon.swordImpactHurtableSound);
        } else {
            Toolbox.RandomizeOneShot(audioSource, meleeWeapon.swordImpactNormalSound);
            swordImpactPool.GetObject(collisionPoint);
        }
        ShowSwordLine(false);
    }
}
