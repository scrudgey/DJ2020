using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;

public class GunHandler : MonoBehaviour, IBindable<GunHandler>, ISaveable {
    public enum GunState {
        idle,
        // walking,
        shooting,
        // crouching,
        racking,
        reloading,
        // running,
        // climbing,
        // crawling
    }
    public GunState state;
    public Action<GunHandler> OnValueChanged { get; set; }

    static readonly public float height = 0.5f;
    // public GunAnimation gunAnimation;
    public AudioSource audioSource;
    public Light muzzleFlashLight;
    public KinematicCharacterMotor motor;
    public GunInstance gunInstance;
    // public bool shooting;
    // public bool reloading;
    public GunInstance secondary;
    public GunInstance primary;
    public GunInstance third;
    public bool emitShell;
    public TargetData lastTargetData;
    // public Action<GunHandler> OnTargetChanged;
    private float movementInaccuracy;
    private float crouchingInaccuracy;
    private float shootingInaccuracy;
    public PlayerCharacterInput.FireInputs lastInput;

    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }

    public void ShootCallback() {
        Shoot(lastInput);
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
        state = GunState.idle;
    }
    // used by animator
    public void EndShoot() {
        // shooting = false;
        state = GunState.idle;
    }
    // used by animator
    public void ClipIn() {
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.clipIn);
        gunInstance.ClipIn();
        OnValueChanged?.Invoke(this);
    }
    // used by animator
    public void ShellIn() {
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.clipIn);
        gunInstance.ShellIn();
        OnValueChanged?.Invoke(this);
    }
    // used by animator
    public void StopReload() {
        // reloading = false;
        state = GunState.idle;
    }


    public bool HasGun() {
        return gunInstance != null && gunInstance.baseGun != null;
    }
    public void Update() {
        if (gunInstance == null) {
            return;
        } else {
            gunInstance.Update();
        }

        if (state != GunState.shooting && state != GunState.reloading && gunInstance.chamber == 0 && gunInstance.clip > 0) {
            // gunAnimation.StartRack();
            Rack();
        }

        if (movementInaccuracy > 0) {
            // TODO: inaccuracy decrease based on weapon weight
            movementInaccuracy -= Time.deltaTime;
            movementInaccuracy = Math.Max(0, movementInaccuracy);
        }
        if (shootingInaccuracy > 0) {
            shootingInaccuracy -= Time.deltaTime;
            shootingInaccuracy = Math.Max(0, shootingInaccuracy);
        }
    }

    public Vector3 gunPosition() {
        // return new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
        return new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
    }
    public Vector3 gunDirection() {
        return lastTargetData.position - this.gunPosition();
    }
    public float inaccuracy() {
        float accuracy = 0;

        // returns the inaccuracy in world units at the point of the last target data
        if (gunInstance == null || gunInstance.baseGun == null || lastTargetData == null)
            return 0f;

        // range
        float distance = Vector3.Distance(lastTargetData.position, this.gunPosition());
        accuracy += gunInstance.baseGun.spread * (distance / 10f);

        // movement
        accuracy += movementInaccuracy;

        // crouching
        accuracy += crouchingInaccuracy;

        // shooting
        accuracy += shootingInaccuracy;

        // skills
        int skillLevel = GameManager.I.gameData.playerData.gunSkillLevel[gunInstance.baseGun.type];
        float skillBonus = (1 - skillLevel) * (0.1f);
        accuracy += skillBonus;

        // weapon mods

        accuracy = Math.Max(0, accuracy);

        return accuracy;
    }
    public void EmitBullet(PlayerCharacterInput.FireInputs input) {
        Vector3 gunPosition = this.gunPosition();

        // determine the direction to shoot in
        Vector3 trueDirection = gunDirection();
        // Debug.DrawRay(gunPosition, trueDirection * 10f, Color.green, 10f);

        Ray sightline = new Ray(gunPosition, trueDirection);
        Vector3 aimpoint = sightline.GetPoint(10f); // a fixed distance from the gun

        Vector3 jitter = UnityEngine.Random.insideUnitSphere * gunInstance.baseGun.spread;
        Vector3 jitterPoint = aimpoint + jitter;

        Vector3 direction = jitterPoint - gunPosition;
        Vector3 endPosition = gunPosition + (gunInstance.baseGun.range * direction);

        Bullet bullet = new Bullet(new Ray(gunPosition, direction)) {
            damage = gunInstance.baseGun.getBaseDamage(),
            range = gunInstance.baseGun.range,
            gunPosition = gunPosition
        };

        bullet.DoImpacts();
    }

    public void Shoot(PlayerCharacterInput.FireInputs input) {
        if (!gunInstance.CanShoot()) {
            // EndShoot(); maybe?
            return;
        }
        lastInput = input;

        // update state
        gunInstance.Shoot();

        // shoot bullet
        EmitBullet(input);

        // play sound
        // TODO: change depending on silencer
        audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(Toolbox.RandomFromList(gunInstance.baseGun.shootSounds));

        // flash
        // TODO: change depending on silencer
        muzzleFlashLight.enabled = true;
        StartCoroutine(Toolbox.RunAfterTime(0.1f, () => {
            muzzleFlashLight.enabled = false;
        }));

        // muzzleflash obj
        // TODO: change depending on silencer
        GameObject muzzleFlashObj = GameObject.Instantiate(
            gunInstance.baseGun.muzzleFlash,
            gunPosition() + 0.5f * motor.CharacterForward - new Vector3(0, 0.1f, 0),
            Quaternion.LookRotation(transform.up, gunDirection())
            );
        muzzleFlashObj.transform.localScale = gunInstance.baseGun.muzzleflashSize * Vector3.one;
        GameObject.Destroy(muzzleFlashObj, 0.05f);

        // shell casing
        if (gunInstance.baseGun.type != GunType.shotgun) {
            EmitShell();
        }

        // accuracy effect
        shootingInaccuracy = gunInstance.baseGun.shootInaccuracy;

        // state change callbacks
        OnValueChanged?.Invoke(this);
    }
    public void EmitShell() {
        emitShell = true;
    }
    public void DoEmitShell() {
        Vector3 targetPosition = gunPosition() + 0.2f * transform.right + 0.2f * transform.forward;
        GameObject shell = PoolManager.I.GetPool(gunInstance.baseGun.shellCasing).GetObject();
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
    }
    public void EmitMagazine() {
        GameObject mag = GameObject.Instantiate(
                gunInstance.baseGun.magazine,
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
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.aimSounds);
    }
    public void Rack() {
        if (state == GunState.racking)
            return;
        if (gunInstance == null || gunInstance.baseGun == null) {
            return;
        }
        state = GunState.racking;
        if (gunInstance.baseGun.cycle == CycleType.manual) {
            EmitShell();
        }
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.rackSounds);
        gunInstance.Rack();
    }
    public void Reload() {
        if (state == GunState.reloading)
            return;

        // reloading = true;
        // shooting = false;
        state = GunState.reloading;

        // gunInstance.Reload();
        gunInstance.ClipOut();

        // drop clip
        EmitMagazine();

        // play sound
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.clipOut);

        // start animation
        // gunAnimation.StartReload();

        OnValueChanged?.Invoke(this);
    }
    public void ReloadShell() {
        if (state == GunState.reloading)
            return;
        // reloading = true;
        // shooting = false;
        state = GunState.reloading;

        // start animation
        // gunAnimation.StartReload();
    }

    private void SwitchGun(GunInstance instance) {
        if (instance == null || instance == gunInstance)
            return;
        gunInstance = instance;

        // gunAnimation.Unholster();


        // TODO: don't call this! maybe?
        // gunAnimation.EndShoot();
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.unholster);

        PoolManager.I.RegisterPool(gunInstance.baseGun.shellCasing);

        OnValueChanged?.Invoke(this);
    }
    public void Holster() {
        gunInstance = null;
        // gunAnimation.Holster();
        OnValueChanged?.Invoke(this);
    }
    public void SwitchToGun(int idn) {
        switch (idn) {
            default:
            case 0:
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

    public void ProcessGunSwitch(PlayerCharacterInput input) {
        if (input.switchToGun != -1) {
            SwitchToGun(input.switchToGun);
        } else if (input.reload) {
            DoReload();
        }
    }
    void DoReload() {
        if (gunInstance == null)
            return;
        if (gunInstance.baseGun.type == GunType.shotgun && gunInstance.clip < gunInstance.baseGun.clipSize) {
            ReloadShell();
        } else if (gunInstance.baseGun.type != GunType.shotgun) {
            Reload();
        }
    }
    public void ProcessInput(PlayerCharacterInput input) {
        // gunAnimation.input = input.Fire;
        lastTargetData = input.Fire.targetData;
        OnValueChanged?.Invoke(this);
        if (HasGun() && gunInstance.CanShoot()) {
            if (gunInstance.baseGun.cycle == CycleType.automatic) {
                if (input.Fire.FirePressed && state != GunState.shooting) {
                    // gunAnimation.StartShooting();
                    // shooting = true;
                    // reloading = false;
                    state = GunState.shooting;
                } else if (state == GunState.shooting && !input.Fire.FireHeld) {
                    EndShoot();
                    // shooting = false;

                }
            } else { // semiautomatic
                if (input.Fire.FirePressed) {//&& !shooting) {
                    // gunAnimation.StartShooting();
                    // shooting = true;
                    // reloading = false;
                    state = GunState.shooting;
                }
            }
        }
        if (input.MoveAxisForward != 0 || input.MoveAxisRight != 0) {
            // TODO: inaccuracy based on weapon weight
            float movement = 1;
            movementInaccuracy = Math.Max(movement, movementInaccuracy);
        }
        if (input.CrouchDown) {
            crouchingInaccuracy = -0.5f;
        } else {
            crouchingInaccuracy = 0f;
        }
    }

    // TODO: save method
    public void LoadState(PlayerData data) {
        primary = data.primaryGun;
        secondary = data.secondaryGun;
        third = data.tertiaryGun;
        SwitchToGun(data.activeGun);
    }
}