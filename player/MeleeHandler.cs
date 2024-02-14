using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

public class MeleeHandler : MonoBehaviour {
    public AudioSource audioSource;
    public WeaponState weaponState;
    public AudioClip[] swordUnholsterSound;
    public AudioClip[] swordHolsterSound;
    public AudioClip[] swordSwingSound;
    public AudioClip[] swordImpactNormalSound;
    public AudioClip[] swordImpactHurtableSound;
    public GameObject swordImpactPrefab;
    public GameObject swordImpactHurtablePrefab;
    PrefabPool swordImpactPool;
    PrefabPool swordImpactHurtablePool;
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
        swordImpactHurtablePool = PoolManager.I.GetPool(swordImpactHurtablePrefab);
        // swordLine.gameObject.SetActive(false);
        ShowSwordLine(false);
    }
    public void Holster() {
        if (weaponState == null) return;

        weaponState = null;
        state = GunHandler.GunStateEnum.holstering;

        fromGunType = GunType.sword;
        toGunType = GunType.unarmed;

        Toolbox.RandomizeOneShot(audioSource, swordHolsterSound);
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
        if (weaponState == null) return;
        if (input.Fire.FirePressed) {
            Toolbox.RandomizeOneShot(audioSource, swordSwingSound);
            state = GunHandler.GunStateEnum.shooting;
            DoSwingLine(direction);
        }
        // Debug.DrawRay(transform.position, direction, Color.white);
    }
    public void SwitchWeapon(WeaponState weaponState) {

        if (weaponState == null || weaponState == this.weaponState)
            return;

        // fromGunType = gunInstance == null ? GunType.unarmed : gunInstance.template.type;
        // toGunType = instance == null ? GunType.unarmed : instance.template.type;
        // isSwitchingWeapon = true;
        state = GunHandler.GunStateEnum.holstering;
        this.weaponState = weaponState;

        fromGunType = GunType.unarmed;
        toGunType = GunType.sword;
        Toolbox.RandomizeOneShot(audioSource, swordUnholsterSound);

        // gunInstance = instance;

        // SetGunAppearanceSuspicion();
        // OnValueChanged?.Invoke(this);
        // OnHolsterFinish?.Invoke(this);
    }

    public AnimationInput.GunAnimationInput BuildAnimationInput() {
        GunType gunType = GunType.unarmed;
        if (weaponState != null) {
            gunType = GunType.sword;
        }
        return new AnimationInput.GunAnimationInput {
            gunType = gunType,
            gunState = state,
            hasGun = weaponState != null || state == GunHandler.GunStateEnum.holstering,
            holstered = weaponState == null,
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
        swordTrigger.enabled = true;
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
                swordTrigger.enabled = false;
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
            Toolbox.RandomizeOneShot(audioSource, swordImpactHurtableSound);
        } else {
            Toolbox.RandomizeOneShot(audioSource, swordImpactNormalSound);
            swordImpactPool.GetObject(collisionPoint);
        }
        ShowSwordLine(false);
        swordTrigger.enabled = false;
    }
}
