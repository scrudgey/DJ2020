using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;

public class GunHandler : MonoBehaviour, IBindable<GunHandler>, IGunHandlerStateLoader, IInputReceiver, IPoolable {
    public enum GunStateEnum {
        idle,
        shooting,
        racking,
        reloading,
    }
    public CharacterCamera characterCamera;
    public GunStateEnum state;
    // public InputMode inputMode;
    public CharacterState characterState;
    public Action<GunHandler> OnValueChanged { get; set; }
    public float height = 0.5f;
    public AudioSource audioSource;
    public Light muzzleFlashLight;
    public KinematicCharacterMotor motor;
    public GunState gunInstance;
    public GunState secondary;
    public GunState primary;
    public GunState third;
    public bool emitShell;
    private float movementInaccuracy;
    private float crouchingInaccuracy;
    private float shootingInaccuracy;
    public CursorData lastShootInput;
    public bool shootRequestedThisFrame;
    public CursorData currentTargetData;
    public bool isShooting;
    public bool isSwitchingWeapon;
    public Action<GunHandler> OnShoot;
    public bool isAimingWeapon;
    Collider[] lockOnColliders;
    public bool nonAnimatedReload;
    static readonly SuspicionRecord BrandishingWeaponRecord = new SuspicionRecord {
        content = "brandishing weapon",
        suspiciousness = Suspiciousness.suspicious
    };
    void Awake() {
        lockOnColliders = new Collider[32];
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    public void ShootCallback() {
        Shoot(lastShootInput);
    }
    // used by animator
    public void AimCallback() {
        Aim();
    }
    // used by animator
    public void RackCallback() {
        Rack();
    }
    public void EndRack() {
        state = GunStateEnum.idle;
    }
    // used by animator
    public void EndShoot() {
        state = GunStateEnum.idle;
        lastShootInput = null;
        shootRequestedThisFrame = false;
    }
    // used by animator
    public void ClipIn() {
        Toolbox.RandomizeOneShot(audioSource, gunInstance.template.clipIn);
        gunInstance.ClipIn();
        OnValueChanged?.Invoke(this);
    }
    // used by animator
    public void ShellIn() {
        Toolbox.RandomizeOneShot(audioSource, gunInstance.template.clipIn);
        gunInstance.ShellIn();
        OnValueChanged?.Invoke(this);
    }
    // used by animator
    public void StopReload() {
        // TODO: change state
        if (gunInstance.ShouldRack()) {
            state = GunStateEnum.racking;
        } else state = GunStateEnum.idle;
    }


    public bool HasGun() => gunInstance != null && gunInstance.template != null;

    public bool CanShoot() => gunInstance.CanShoot() && (state != GunStateEnum.reloading && state != GunStateEnum.racking);

    public void Update() {
        if (gunInstance == null || gunInstance.template == null) {
            state = GunStateEnum.idle;
            return;
        } else {
            gunInstance.Update();
        }

        if (state == GunStateEnum.idle && gunInstance.delta.chamber == 0 && gunInstance.delta.clip > 0) {
            // Rack();
            state = GunStateEnum.racking;
        }

        // TODO: inaccuracy decrease based on weapon weight and skill
        float weightCoefficient = 10f / gunInstance.template.weight;

        if (movementInaccuracy > 0) {
            // lighter weapons recover accuracy faster
            // weight = 1: handgun
            // weight = 10: rifle
            movementInaccuracy -= Time.deltaTime * weightCoefficient;
            movementInaccuracy = Math.Max(0, movementInaccuracy);
        }
        if (shootingInaccuracy > 0) {
            shootingInaccuracy -= Time.deltaTime * weightCoefficient;
            shootingInaccuracy = Math.Max(0, shootingInaccuracy);
        }
    }

    public Vector3 gunPosition() {
        return new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
    }
    public Vector3 gunDirection(CursorData data) {
        if (data == null) return Vector3.zero;
        return data.worldPosition - this.gunPosition();
    }
    public float inaccuracy(CursorData input) {
        float inaccuracy = 0;

        // returns the inaccuracy in world units at the world point of the target data
        if (gunInstance == null || gunInstance.template == null || input == null)
            return 0f;

        // range
        // TODO: change this. use a fixed angular 
        // to do that, inaccuracy must be fixed; when we want to show accuracy at a given distance, apply scale there.
        // inaccuracy should be a quantity that then is applied to a distance.
        // when shooting, it is applied at a fixed distance
        // when displaying, it is shown at that distance
        float distance = Vector3.Distance(input.worldPosition, this.gunPosition());
        // inaccuracy += gunInstance.template.spread * (distance / 10f);
        inaccuracy += gunInstance.template.spread;

        // movement
        inaccuracy += movementInaccuracy;

        // crouching
        inaccuracy += crouchingInaccuracy;

        // shooting
        inaccuracy += shootingInaccuracy;

        // skills
        // TODO: this doesn't work for enemies
        int skillLevel = GameManager.I.gameData.playerState.gunSkillLevel[gunInstance.template.type];
        float skillBonus = (1 - skillLevel) * (0.1f);
        inaccuracy += skillBonus;

        // TODO: weapon mods

        inaccuracy = Math.Max(0.05f, inaccuracy);

        return inaccuracy;
    }
    public Bullet EmitBullet(CursorData input) {
        Vector3 gunPosition = this.gunPosition();

        Vector3 trueDirection = gunDirection(input);
        Debug.DrawRay(gunPosition, trueDirection * 10f, Color.green, 10f);

        Ray sightline = new Ray(gunPosition, trueDirection);
        Vector3 aimpoint = sightline.GetPoint(10f); // a fixed distance from the gun

        // Vector3 jitter = UnityEngine.Random.insideUnitSphere * gunInstance.template.spread;
        Vector3 jitter = UnityEngine.Random.insideUnitSphere * inaccuracy(input);
        Vector3 jitterPoint = aimpoint + jitter;

        Vector3 direction = jitterPoint - gunPosition;
        Vector3 endPosition = gunPosition + (gunInstance.template.range * direction);

        Bullet bullet = new Bullet(new Ray(gunPosition, direction)) {
            damage = gunInstance.template.getBaseDamage(),
            range = gunInstance.template.range,
            gunPosition = gunPosition,
            source = transform.position
        };

        bullet.DoImpacts(transform.root);

        Debug.DrawLine(gunPosition, endPosition, Color.green, 10f);
        return bullet;
    }
    public bool IsClearShot(CursorData input) {
        Vector3 targetPosition = input.worldPosition;
        Transform root = transform.root;
        Vector3 gunPosition = this.gunPosition();
        Vector3 trueDirection = gunDirection(input);
        Ray ray = new Ray(gunPosition, trueDirection);
        // TODO: nonalloc
        RaycastHit[] hits = Physics.RaycastAll(ray, 3f, LayerUtil.GetMask(Layer.obj));
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.collider.transform.IsChildOf(root))
                continue;
            // TODO: better ally rejection here
            SphereRobotAI otherAI = hit.collider.transform.root.GetComponentInChildren<SphereRobotAI>();
            if (otherAI != null) {
                return false;
            }
        }
        return true;
    }
    public void ShootImmediately(CursorData input) {
        Shoot(input);
    }
    void Shoot(CursorData input) {
        if (!HasGun()) {
            return;
        }
        if (!gunInstance.CanShoot()) {
            return;
        }
        isShooting = true;

        // update state
        gunInstance.Shoot();

        // shoot bullet
        int numberBullets = gunInstance.template.type == GunType.shotgun ? 5 : 1;
        Bullet bullet = null;
        for (int i = 0; i < numberBullets; i++) {
            bullet = EmitBullet(input);
        }

        // play sound
        NoiseData noiseData = gunInstance.GetShootNoise() with {
            ray = bullet.ray
        };
        noiseData.player = transform.IsChildOf(GameManager.I.playerObject.transform);
        audioSource.pitch = UnityEngine.Random.Range(noiseData.pitch - 0.1f, noiseData.pitch + 0.1f);
        audioSource.PlayOneShot(Toolbox.RandomFromList(gunInstance.template.GetShootSounds()));
        Toolbox.Noise(gunPosition(), noiseData, transform.root.gameObject);

        // flash
        if (!gunInstance.template.silencer) {
            muzzleFlashLight.enabled = true;
            StartCoroutine(Toolbox.RunAfterTime(0.1f, () => {
                muzzleFlashLight.enabled = false;
            }));
        }

        // muzzleflash obj
        GameObject muzzleFlashObj = PoolManager.I
            .GetPool(gunInstance.template.muzzleFlash)
            .GetObject(gunPosition() + (0.5f * motor.CharacterForward) - new Vector3(0, 0.1f, 0));
        if (gunInstance.template.silencer) {
            muzzleFlashObj.transform.localScale = gunInstance.template.muzzleflashSize * Vector3.one * 0.1f;
        } else {
            muzzleFlashObj.transform.localScale = gunInstance.template.muzzleflashSize * Vector3.one;
        }
        muzzleFlashObj.transform.rotation = Quaternion.LookRotation(transform.up, gunDirection(input));
        GameObject.Destroy(muzzleFlashObj, 0.05f);


        // shell casing
        if (gunInstance.template.type != GunType.shotgun) {
            EmitShell();
        }

        // accuracy effect
        shootingInaccuracy += gunInstance.template.shootInaccuracy / 2f;

        // state change callbacks
        OnValueChanged?.Invoke(this);

        if (transform.IsChildOf(GameManager.I.playerObject.transform)) {
            CharacterCamera.Shake(gunInstance.template.noise / 50f, 0.1f);
            if (!gunInstance.template.silencer) {
                SuspicionRecord record = new SuspicionRecord {
                    content = "shooting gun",
                    suspiciousness = Suspiciousness.aggressive,
                    maxLifetime = 1f,
                    lifetime = 1f
                };
                GameManager.I.AddSuspicionRecord(record);
            }
        }

        // callback
        OnShoot?.Invoke(this);
    }
    public void EmitShell() {
        emitShell = true;
    }
    public void DoEmitShell() {
        Vector3 targetPosition = gunPosition() + 0.2f * transform.right + 0.2f * transform.forward;
        GameObject shell = PoolManager.I.GetPool(gunInstance.template.shellCasing).GetObject();
        Rigidbody body = shell.GetComponent<Rigidbody>();
        if (body != null) {
            body.MovePosition(targetPosition);
            body.MoveRotation(Quaternion.identity);
            body.velocity = Vector3.zero;
            body.AddRelativeForce(
                        UnityEngine.Random.Range(0.5f, 1.5f) * transform.up +
                        UnityEngine.Random.Range(0.1f, 1f) * transform.right +
                        UnityEngine.Random.Range(-0.3f, 0.3f) * transform.forward,
                        ForceMode.Impulse);
            body.AddRelativeTorque(UnityEngine.Random.Range(100f, 600f) * shell.transform.forward);
        }

    }
    void FixedUpdate() {
        if (emitShell) {
            emitShell = false;
            DoEmitShell();
        }
        isShooting = false;
        isSwitchingWeapon = false;
    }
    public void EmitMagazine() {
        GameObject mag = GameObject.Instantiate(
                gunInstance.template.magazine,
                gunPosition() + 0.2f * transform.right + 0.2f * transform.forward - 0.2f * transform.up,
                Quaternion.identity
            );
        Rigidbody body = mag.GetComponent<Rigidbody>();

        body.AddRelativeForce(
            UnityEngine.Random.Range(-0.1f, 0.1f) * transform.right +
            UnityEngine.Random.Range(-0.1f, 0.1f) * transform.forward,
            ForceMode.Impulse);
        body.AddRelativeTorque(UnityEngine.Random.Range(2f, 3f) * mag.transform.forward);
    }
    public void Aim() {
        Toolbox.RandomizeOneShot(audioSource, gunInstance.template.aimSounds);
    }
    public void Rack() {
        if (gunInstance == null || gunInstance.template == null) {
            return;
        }
        if (gunInstance.template.cycle == CycleType.manual) {
            EmitShell();
        }
        Toolbox.RandomizeOneShot(audioSource, gunInstance.template.rackSounds);
        gunInstance.Rack();
    }
    public void Reload() {
        if (state == GunStateEnum.reloading)
            return;

        state = GunStateEnum.reloading;

        gunInstance.ClipOut();

        // drop clip
        EmitMagazine();

        // play sound
        Toolbox.RandomizeOneShot(audioSource, gunInstance.template.clipOut);

        if (nonAnimatedReload) {
            ClipIn();
            StopReload();
            // state = GunStateEnum.idle;
            Rack();
            EndRack();
        }

        OnValueChanged?.Invoke(this);
    }
    public void ReloadShell() {
        if (state == GunStateEnum.reloading)
            return;
        state = GunStateEnum.reloading;
    }
    private void SwitchGun(GunState instance) {
        if (instance == null || instance == gunInstance)
            return;
        isSwitchingWeapon = true;

        gunInstance = instance;

        if (gunInstance != null && gunInstance.template != null) {
            Toolbox.RandomizeOneShot(audioSource, gunInstance.template.unholster);
            PoolManager.I?.RegisterPool(gunInstance.template.shellCasing);
            PoolManager.I?.RegisterPool(gunInstance.template.muzzleFlash);
            if (GameManager.I.playerObject != null && transform.IsChildOf(GameManager.I.playerObject.transform)) {
                GameManager.I.AddSuspicionRecord(BrandishingWeaponRecord);
            }
        }
        OnValueChanged?.Invoke(this);
    }
    public void Holster() {
        isSwitchingWeapon = true;
        gunInstance = null;
        OnValueChanged?.Invoke(this);
        if (GameManager.I.playerObject != null && transform.IsChildOf(GameManager.I.playerObject.transform)) {
            GameManager.I.RemoveSuspicionRecord(BrandishingWeaponRecord);
        }
    }
    public void SwitchToGun(int idn) {
        switch (idn) {
            case 0:
                break;
            case -1:
                Holster();
                break;
            case 1:
                SwitchGun(primary);
                break;
            case 2:
                SwitchGun(secondary);
                break;
            case 3:
                SwitchGun(third);
                break;
        }
    }

    public void ProcessGunSwitch(PlayerInput input) {
        SwitchToGun(input.selectgun);
        if (input.reload) {
            DoReload();
        }
    }
    public void DoReload() {
        if (gunInstance == null || gunInstance.template == null)
            return;
        if (gunInstance.template.type == GunType.shotgun && gunInstance.delta.clip < gunInstance.template.clipSize) {
            ReloadShell();
        } else if (gunInstance.template.type != GunType.shotgun) {
            Reload();
        }
    }
    public void SetInputs(PlayerInput input) {
        // inputMode = input.inputMode;
        currentTargetData = input.Fire.cursorData;
        shootRequestedThisFrame = false;
        isAimingWeapon = input.aimWeapon;

        if (HasGun()) {
            Vector3 targetPoint = input.Fire.cursorData.worldPosition;
            // TODO: if priority is not set, try lock 
            float lockRadius = gunInstance.template.lockOnSize;
            int numColliders = Physics.OverlapSphereNonAlloc(targetPoint, lockRadius, lockOnColliders, LayerUtil.GetMask(Layer.obj));
            Collider nearestOther = null;
            for (int i = 0; i < numColliders; i++) {
                Collider collider = lockOnColliders[i];
                if (collider.transform.IsChildOf(transform.root)) {
                    continue;
                }
                if (nearestOther == null || (Vector3.Distance(targetPoint, collider.bounds.center)) < Vector3.Distance(targetPoint, nearestOther.bounds.center)) {
                    nearestOther = collider;
                }
            }
            if (nearestOther != null) {
                nearestOther = nearestOther.transform.root.GetComponentInChildren<Collider>();
                targetPoint = nearestOther.bounds.center;
                TagSystemData tagData = Toolbox.GetTagData(nearestOther.gameObject);
                if (tagData.targetPriority > -1) {
                    if (tagData != null && tagData.targetPoint != null && tagData.targetPriority > -1) {
                        targetPoint = tagData.targetPoint.position;
                    }
                    Vector2 pointPosition = characterCamera.Camera.WorldToScreenPoint(targetPoint);
                    input.Fire.cursorData.type = CursorData.TargetType.objectLock;
                    input.Fire.cursorData.screenPosition = pointPosition;
                    input.Fire.cursorData.targetCollider = nearestOther;


                    // TODO: is this a hack? what about NPCs?
                    if (GameManager.I.inputMode == InputMode.aim) {

                    } else {
                        input.Fire.cursorData.worldPosition = targetPoint;
                    }
                }
            }

            if (CanShoot()) {
                if (gunInstance.template.cycle == CycleType.automatic) {
                    if (input.Fire.FirePressed && state != GunStateEnum.shooting) {
                        lastShootInput = input.Fire.cursorData;
                        state = GunStateEnum.shooting;
                    } else if (input.Fire.FireHeld) {
                        state = GunStateEnum.shooting;
                        lastShootInput = input.Fire.cursorData;
                    } else if (state == GunStateEnum.shooting && !input.Fire.FireHeld) {
                        EndShoot();
                    }
                } else { // semiautomatic
                    if (input.Fire.FirePressed) {//&& !shooting) {
                        lastShootInput = input.Fire.cursorData;
                        state = GunStateEnum.shooting;
                        shootRequestedThisFrame = true;
                    }
                }
            } else {
                if (gunInstance.template.cycle == CycleType.automatic) {
                    if (state == GunStateEnum.shooting) {
                        state = GunStateEnum.idle;
                    }
                }
            }
        }

        if (input.MoveAxisForward != 0 || input.MoveAxisRight != 0) {
            // TODO: move inaccuracy based on weapon weight and skill
            // float movement = 1;
            // movementInaccuracy = Math.Max(movement, movementInaccuracy);
            movementInaccuracy += motor.Velocity.magnitude * Time.deltaTime;
            movementInaccuracy = Math.Min(1f, movementInaccuracy);
            // inaccuracy increases faster for heavy weapons, decreases faster for lighter weapons
        }
        if (input.CrouchDown) {
            crouchingInaccuracy = -0.15f;
        } else {
            crouchingInaccuracy = 0f;
        }
        OnValueChanged?.Invoke(this);

        if (input.Fire.skipAnimation && (input.Fire.FireHeld || input.Fire.FirePressed) && gunInstance.delta.cooldownTimer <= 0)
            ShootImmediately(input.Fire.cursorData);
    }

    public AnimationInput.GunAnimationInput BuildAnimationInput() {
        GunType gunType = GunType.unarmed;
        GunTemplate baseGun = null;
        if (HasGun()) {
            gunType = gunInstance.template.type;
            baseGun = gunInstance.template;
        }
        return new AnimationInput.GunAnimationInput {
            gunType = gunType,
            gunState = state,
            hasGun = gunInstance != null && HasGun(),
            holstered = gunInstance == null,
            baseGun = baseGun,
            shootRequestedThisFrame = shootRequestedThisFrame,
            aimWeapon = isAimingWeapon
        };
    }

    // TODO: save method
    public void LoadGunHandlerState(IGunHandlerState state) {
        // TODO: here, we would instantiate from template with mutable state applied
        primary = state.primaryGun;
        secondary = state.secondaryGun;
        third = state.tertiaryGun;
        SwitchToGun(state.activeGun);
    }

    public void OnPoolActivate() {

    }
    public void OnPoolDectivate() {
        if (gunInstance != null && gunInstance.template != null)
            gunInstance = GunState.Instantiate(gunInstance.template);
    }
}