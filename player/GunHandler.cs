using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;

public class GunHandler : MonoBehaviour, IBindable<GunHandler>, IInputReceiver, IPoolable { // IGunHandlerStateLoader,
    public enum GunStateEnum {
        idle,
        shooting,
        racking,
        reloading,
        holstering,
    }
    public ItemHandler itemHandler;
    public CharacterCamera characterCamera;
    public GunStateEnum state;
    public Action<GunHandler> OnValueChanged { get; set; }
    public float height = 0.5f;
    public AudioSource audioSource;
    public Light muzzleFlashLight;
    public KinematicCharacterMotor motor;
    public GunState gunInstance;
    public bool emitShell;
    private float movementInaccuracy;
    private float crouchingInaccuracy;
    private float shootingInaccuracy;
    public CursorData lastShootInput;
    public bool shootRequestedThisFrame;
    public CursorData currentTargetData;
    public bool isShooting;

    public Action<GunHandler> OnShoot;
    public Action<GunHandler> OnHolsterFinish;
    public bool isAimingWeapon;
    public bool isPlayerCharacter;
    Collider[] lockOnColliders;
    public bool nonAnimatedReload;
    public GameObject tamperEvidenceObject;
    public AudioClip[] reachForHolsterSound;
    // int numberOfShellsPerReload;
    public bool isSwitchingWeapon;
    GunType fromGunType;
    GunType toGunType;
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
        int numberOfShellsPerReload = isPlayerCharacter ? GameManager.I.gameData.playerState.numberOfShellsPerReload() : 1;
        Toolbox.RandomizeOneShot(audioSource, gunInstance.template.clipIn);
        gunInstance.ShellIn(numberOfShellsPerReload);
        OnValueChanged?.Invoke(this);
    }
    // used by animator
    public void StopReload() {
        if (gunInstance.ShouldRack()) {
            state = GunStateEnum.racking;
        } else state = GunStateEnum.idle;
    }
    // used by animator
    public void StopHolster() {
        // Debug.Log($"{transform.root.gameObject} stop holster");
        state = GunStateEnum.idle;
        OnValueChanged?.Invoke(this);
        OnHolsterFinish?.Invoke(this);
    }


    public bool HasGun() => (gunInstance != null && gunInstance.template != null);

    public bool CanShoot() => gunInstance.CanShoot() && (state != GunStateEnum.reloading && state != GunStateEnum.racking);

    public void Update() {
        if (gunInstance == null || gunInstance.template == null) {
            // state = GunStateEnum.idle;
            return;
        } else {
            gunInstance.Update();
        }

        if (state == GunStateEnum.idle && gunInstance.delta.chamber == 0 && gunInstance.delta.clip > 0) {
            // Rack();
            state = GunStateEnum.racking;
        }

        if (movementInaccuracy > 0) {
            movementInaccuracy = CalculateInaccuracyRecovery(gunInstance, movementInaccuracy);
        }

        if (shootingInaccuracy > 0) {
            shootingInaccuracy = CalculateInaccuracyRecovery(gunInstance, shootingInaccuracy);
        }
    }

    float CalculateInaccuracyRecovery(GunState gunState, float value) {
        if (gunState == null)
            return 0f;
        // movement inaccuracy recovery
        //     skill normalized
        //     weight involved

        float weight = gunState.getWeight();
        // lighter weapons recover accuracy faster

        float adjustmentFactor = 1f;
        if (isPlayerCharacter) {
            int skillLevel = GameManager.I.gameData.playerState.PerkGunControlLevel(gunState.template.type);

            adjustmentFactor = skillLevel switch {
                0 => 2f,
                1 => 3f,
                2 => 3.5f,
                3 => 4f,
                _ => 4f,

            };
        }

        float inaccuracy = value - adjustmentFactor * (Time.deltaTime / weight);
        inaccuracy = Math.Max(0, inaccuracy);
        return inaccuracy;
    }

    float CalculateRecoilInaccuracy(GunState gunState, float value) {
        if (gunState == null)
            return 0f;
        // recoil inaccuracy
        //     driven by recoil stat
        //     skill normalized

        float recoil = gunState.getRecoil().GetRandomInsideBound() / 10f;

        float adjustmentFactor = 1f;

        if (isPlayerCharacter) {
            int skillLevel = GameManager.I.gameData.playerState.PerkGunControlLevel(gunState.template.type);
            // 0, 1, 2, 3

            adjustmentFactor = skillLevel switch {
                0 => 2f,
                1 => 1.5f,
                2 => 1.2f,
                3 => 1f,
                _ => 1f,
            };
        }

        // Debug.Log($"{recoil} * {adjustmentFactor} = {recoil * adjustmentFactor}");
        float inaccuracy = recoil * adjustmentFactor;

        float maximum = gunState.template.cycle switch {
            CycleType.automatic => gunState.getRecoil().Average() * 1.5f,
            CycleType.manual => gunState.getRecoil().Average(),
            CycleType.semiautomatic => gunState.getRecoil().Average() * 1.5f,
            _ => gunState.getRecoil().high
        } * adjustmentFactor / 10f;

        inaccuracy += value;
        inaccuracy = Math.Min(inaccuracy, maximum);
        // inaccuracy = Math.Max(shootingInaccuracy, inaccuracy);

        return inaccuracy;
    }
    float CalculateMovementInaccuracy(GunState gunState, float value) {
        if (gunState == null)
            return 0f;
        // movement inaccuracy
        //     driven by weight
        // TODO: skill normalized
        float weight = gunState.getWeight();

        float inaccuracy = value + motor.Velocity.magnitude * (Time.deltaTime * (weight / 5f));

        float minimum = gunState.template.type switch {
            GunType.pistol => 0.75f,
            GunType.smg => 0.8f,
            _ => 1f
        };

        inaccuracy = Math.Min(minimum, inaccuracy);
        return inaccuracy;
    }

    public Vector3 gunPosition() {
        return new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
    }
    public Vector3 gunDirection(CursorData data) {
        if (data == null) return Vector3.zero;
        return data.worldPosition - this.gunPosition();
    }
    public float CalculateInaccuracy(CursorData input) {
        if (gunInstance == null || gunInstance.template == null || input == null)
            return 0f;
        float spread = gunInstance.getSpread();

        // movement
        // inaccuracy += movementInaccuracy;

        // // crouching
        // inaccuracy += crouchingInaccuracy;

        // // shooting
        // inaccuracy += shootingInaccuracy;

        float inaccuracy = (float)Math.Sqrt(Math.Pow(spread, 2) + Math.Pow(movementInaccuracy, 2) + Math.Pow(shootingInaccuracy, 2)) + crouchingInaccuracy;
        // skill
        if (isPlayerCharacter) {
            int skillLevel = GameManager.I.gameData.playerState.PerkGunAccuracyLevel(gunInstance.template.type);
            // 0, 1, 2, 3

            // 0: accuracy -> 80%;      spread -> 1.5
            // 1: accuracy -> 100%; `   spread -> 1.0
            // 2: accuracy -> 120%;     spread -> 0.8
            // 2: accuracy -> 140%;     spread -> 0.5
            float adjustmentFactor = skillLevel switch {
                0 => 2f,
                1 => 1.5f,
                2 => 1f,
                3 => 0.5f,
                _ => 1f,
            };

            // Debug.Log($"{skillLevel} -> {adjustmentFactor} -> {inaccuracy} -> {inaccuracy * adjustmentFactor}");
            inaccuracy *= adjustmentFactor;
        }

        inaccuracy = Math.Max(0.05f, inaccuracy);
        return inaccuracy;
    }
    public float CalculateInaccuracyAtDistance(CursorData input, float distance) {
        return CalculateInaccuracy(input) * distance / 10f;
    }
    public Bullet EmitBullet(CursorData input, int numberOfBullets) {
        Vector3 gunPosition = this.gunPosition();

        Vector3 trueDirection = gunDirection(input);
        // Debug.DrawRay(gunPosition, trueDirection * 10f, Color.green, 10f);

        Ray sightline = new Ray(gunPosition, trueDirection);
        Vector3 baseAimpoint = sightline.GetPoint(10f); // a fixed distance from the gun

        float inaccuracy = CalculateInaccuracy(input);
        Vector3 jitter = UnityEngine.Random.insideUnitSphere * inaccuracy;
        Vector3 aimPoint = baseAimpoint + jitter;

        int bulletIndex = 0;
        float shotgunSpread = 0f;
        Bullet bullet = null;
        while (bulletIndex < numberOfBullets) {
            bulletIndex += 1;

            Vector3 shotJitter = UnityEngine.Random.insideUnitSphere * shotgunSpread;
            Vector3 jitterPoint = aimPoint + shotJitter;

            Vector3 direction = jitterPoint - gunPosition;
            Vector3 endPosition = gunPosition + (gunInstance.getRange() * direction);
            bullet = new Bullet(new Ray(gunPosition, direction)) {
                damage = gunInstance.getBaseDamage(),
                range = gunInstance.getRange(),
                gunPosition = gunPosition,
                source = transform.position,
                piercing = gunInstance.getPiercing()
            };
            bullet.DoImpacts(transform.root);
            // Debug.DrawLine(gunPosition, endPosition, Color.green, 10f);

            shotgunSpread = gunInstance.template.shotgunSpread;
        }


        // shootingInaccuracy = Math.Max(shootingInaccuracy, CalculateRecoilInaccuracy());
        shootingInaccuracy = CalculateRecoilInaccuracy(gunInstance, shootingInaccuracy);
        return bullet;
    }


    public bool IsClearShot(CursorData input) {
        Vector3 targetPosition = input.worldPosition;
        Transform root = transform.root;
        Vector3 gunPosition = this.gunPosition();
        Vector3 trueDirection = gunDirection(input);
        Ray ray = new Ray(gunPosition, trueDirection);

        // TODO: nonalloc
        RaycastHit[] hits = Physics.RaycastAll(ray, 3f, LayerUtil.GetLayerMask(Layer.obj));
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
        bullet = EmitBullet(input, numberBullets);

        // play sound
        NoiseData noiseData = gunInstance.GetShootNoise() with {
            ray = bullet.ray,
            relevantParties = new HashSet<Transform>() { transform.root }
        };
        noiseData.player = isPlayerCharacter;
        audioSource.pitch = UnityEngine.Random.Range(noiseData.pitch - 0.1f, noiseData.pitch + 0.1f);
        audioSource.PlayOneShot(Toolbox.RandomFromList(gunInstance.GetShootSounds()));
        Toolbox.Noise(gunPosition(), noiseData, transform.root.gameObject);

        // flash
        if (!gunInstance.getSilencer()) {
            muzzleFlashLight.enabled = true;
            StartCoroutine(Toolbox.RunAfterTime(0.1f, () => {
                muzzleFlashLight.enabled = false;
            }));
        }

        // muzzleflash obj
        GameObject muzzleFlashObj = PoolManager.I
            .GetPool(gunInstance.template.muzzleFlash)
            .GetObject(gunPosition() + (0.5f * motor.CharacterForward) - new Vector3(0, 0.1f, 0));
        if (gunInstance.getSilencer()) {
            muzzleFlashObj.transform.localScale = gunInstance.getMuzzleFlashSize() * Vector3.one * 0.1f;
        } else {
            muzzleFlashObj.transform.localScale = gunInstance.getMuzzleFlashSize() * Vector3.one;
        }
        muzzleFlashObj.transform.rotation = Quaternion.LookRotation(transform.up, gunDirection(input));
        GameObject.Destroy(muzzleFlashObj, 0.05f);


        // shell casing
        if (gunInstance.template.type != GunType.shotgun) {
            EmitShell();
        }

        // state change callbacks
        OnValueChanged?.Invoke(this);

        if (isPlayerCharacter) {
            CharacterCamera.Shake(gunInstance.getNoise() / 10f, 0.1f);
            // if (!gunInstance.getSilencer()) {
            SuspicionRecord record = SuspicionRecord.shotsFiredSuspicion();
            GameManager.I.AddSuspicionRecord(record);
            // }
        }

        TargetPracticeUIHandler.OnShotFired?.Invoke();

        // callback
        OnShoot?.Invoke(this);
    }
    public void EmitShell() {
        emitShell = true;
    }
    public void DoEmitShell() {
        Vector3 right = transform.root.right;
        Vector3 forward = transform.root.forward;
        Vector3 up = transform.root.up;

        Vector3 targetPosition = gunPosition() + 0.2f * right + 0.2f * forward;
        GameObject shell = PoolManager.I.GetPool(gunInstance.template.shellCasing).GetObject();
        Rigidbody body = shell.GetComponent<Rigidbody>();
        if (body != null) {
            body.MovePosition(targetPosition);
            body.MoveRotation(Quaternion.identity);
            body.velocity = Vector3.zero;
            body.AddRelativeForce(
                        UnityEngine.Random.Range(0.5f, 1.5f) * up +
                        UnityEngine.Random.Range(0.1f, 1f) * right +
                        UnityEngine.Random.Range(-0.3f, 0.3f) * forward,
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
    public void SwitchGun(GunState instance) {
        Debug.Log($"switching gun: {instance}");
        if (instance == null || instance == gunInstance)
            return;
        fromGunType = gunInstance == null ? GunType.unarmed : gunInstance.template.type;
        toGunType = instance == null ? GunType.unarmed : instance.template.type;
        isSwitchingWeapon = true;
        state = GunStateEnum.holstering;
        // Debug.Log($"{transform.root.gameObject} start holster");
        if (gunInstance == null) {
            Toolbox.RandomizeOneShot(audioSource, reachForHolsterSound);
        }
        gunInstance = instance;

        SetGunAppearanceSuspicion();
        OnValueChanged?.Invoke(this);
        OnHolsterFinish?.Invoke(this);
    }
    public void SetGunAppearanceSuspicion() {
        if (gunInstance != null && gunInstance.template != null) {
            Toolbox.RandomizeOneShot(audioSource, gunInstance.template.unholster);
            PoolManager.I?.RegisterPool(gunInstance.template.shellCasing);
            PoolManager.I?.RegisterPool(gunInstance.template.muzzleFlash);
            if (isPlayerCharacter) {
                GameManager.I.AddSuspicionRecord(SuspicionRecord.brandishingSuspicion());
            }
        }
    }
    public void Holster() {
        if (gunInstance == null) return;
        fromGunType = gunInstance == null ? GunType.unarmed : gunInstance.template.type;
        toGunType = GunType.unarmed;
        isSwitchingWeapon = true;
        state = GunStateEnum.holstering;
        // Debug.Log("start holster");
        if (gunInstance != null) {
            Toolbox.RandomizeOneShot(audioSource, reachForHolsterSound);
        }
        gunInstance = null;
        OnValueChanged?.Invoke(this);
        if (isPlayerCharacter) {
            GameManager.I.RemoveSuspicionRecord(SuspicionRecord.brandishingSuspicion());
        }
    }
    public void SwitchToGun(GunState gunInstance) {
        if (gunInstance == null) {
            Holster();
        } else {
            itemHandler?.EvictSubweapon();
            SwitchGun(gunInstance);
        }
    }

    public void DoReload() {
        if (gunInstance == null || gunInstance.template == null)
            return;
        if (gunInstance.template.type == GunType.shotgun && gunInstance.delta.clip < gunInstance.getClipSize()) {
            ReloadShell();
        } else if (gunInstance.template.type != GunType.shotgun) {
            Reload();
        }
    }
    public void SetInputs(PlayerInput input) {
        if (input.reload) {
            DoReload();
        }

        currentTargetData = input.Fire.cursorData;
        shootRequestedThisFrame = false;
        isAimingWeapon = input.aimWeapon;

        if (state == GunStateEnum.holstering) {
            return;
        }

        if (HasGun()) {
            Vector3 targetPoint = input.Fire.cursorData.worldPosition;
            // TODO: if priority is not set, try lock 
            float lockRadius = gunInstance.getLockOnSize();
            int numColliders = Physics.OverlapSphereNonAlloc(targetPoint, lockRadius, lockOnColliders, LayerUtil.GetLayerMask(Layer.obj));
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
                    if (state == GunStateEnum.shooting && gunInstance.delta.clip == 0 && gunInstance.delta.chamber == 0) {
                        state = GunStateEnum.idle;
                    }
                }
            }
        }

        if (input.MoveAxisForward != 0 || input.MoveAxisRight != 0) {
            movementInaccuracy = CalculateMovementInaccuracy(gunInstance, movementInaccuracy);
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
            hasGun = HasGun() || state == GunStateEnum.holstering,
            holstered = gunInstance == null,
            baseGun = baseGun,
            shootRequestedThisFrame = shootRequestedThisFrame,
            aimWeapon = isAimingWeapon,
            fromGunType = fromGunType,
            toGunType = toGunType,
        };
    }
    public void OnPoolActivate() {

    }
    public void OnPoolDectivate() {
        if (gunInstance != null && gunInstance.template != null)
            gunInstance = GunState.Instantiate(gunInstance.template);
    }
}