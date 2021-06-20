using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System.Linq;

public class GunHandler : MonoBehaviour {
    static readonly float height = 1.2f;

    public GunAnimation gunAnimation;
    public LightmapPixelPicker pixelPicker;
    public AudioSource audioSource;
    public Light muzzleFlashLight;
    public KinematicCharacterMotor motor;
    public GunInstance gunInstance;
    public bool shooting;
    public GunInstance secondary;
    public GunInstance primary;
    public GunInstance third;

    public void Update() {
        if (gunInstance == null) {
            return;
        } else {
            gunInstance.Update();
        }

        // TODO: use gun functional spec instead of type here
        if (!shooting && gunInstance.chamber == 0 && gunInstance.clip > 0) {
            if (gunInstance.baseGun.type != GunType.shotgun) {
                gunInstance.Rack();
            } else {
                gunAnimation.StartRack();
            }
        }

    }
    public Vector3 CursorToTargetPoint(PlayerCharacterInputs.FireInputs input) {
        Vector3 targetPoint = Vector3.zero;
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray projection = Camera.main.ScreenPointToRay(input.cursorPosition);
        float distance = 0;
        if (plane.Raycast(projection, out distance)) {
            Vector3 hitPoint = projection.GetPoint(distance);
            targetPoint = new Vector3(hitPoint.x, height, hitPoint.z);
        }
        return targetPoint;
    }
    public void SpawnBulletRay(Vector3 startPoint, Vector3 endPoint) {
        GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/bulletRay"), transform.position, Quaternion.identity) as GameObject;
        BulletRay ray = obj.GetComponent<BulletRay>();
        ray.SetFadeStyle(BulletRay.FadeStyle.streak);

        ray.lineRenderer.SetPosition(0, startPoint);
        ray.lineRenderer.SetPosition(1, endPoint);
    }
    private Vector3 gunPosition() {
        return new Vector3(transform.position.x, height, transform.position.z);
    }
    private Vector3 gunDirection(PlayerCharacterInputs.FireInputs input) {
        Vector3 targetPoint = CursorToTargetPoint(input);
        return targetPoint - this.gunPosition();
    }
    public void EmitRay(PlayerCharacterInputs.FireInputs input) {
        Vector3 gunPosition = this.gunPosition();

        // determine the direction to shoot in
        Vector3 trueDirection = gunDirection(input);

        Ray sightline = new Ray(gunPosition, trueDirection);
        Vector3 aimpoint = sightline.GetPoint(10f); // a fixed distance from the gun

        Vector3 jitter = UnityEngine.Random.insideUnitSphere * gunInstance.baseGun.spread;
        Vector3 jitterPoint = aimpoint + jitter;

        Vector3 direction = jitterPoint - gunPosition;
        Vector3 endPosition = gunPosition + (gunInstance.baseGun.range * direction);

        Ray bulletRay = new Ray(gunPosition, direction);

        RaycastHit[] hits = Physics.RaycastAll(bulletRay, gunInstance.baseGun.range); // get all hits
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) { // check hits until a valid one is found
            if (hit.collider.tag == "glass") {
                Glass glass = hit.collider.gameObject.GetComponentInParent<Glass>();
                if (glass != null) {
                    glass.BulletHit(hit, bulletRay);
                }
            } else {
                endPosition = hit.point;
                GameObject decalObject = DecalPool.I.SpawnDecal(hit, DecalPool.DecalType.normal);
                var sparks = Resources.Load("prefabs/impactSpark");
                GameObject sparkObject = GameObject.Instantiate(sparks,
                hit.point + (hit.normal * 0.025f),
                Quaternion.LookRotation(hit.normal)) as GameObject;
                break;
            }
        }
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
            SpawnBulletRay(gunPosition, endPosition);
        }
    }

    public void Shoot(PlayerCharacterInputs.FireInputs input) {
        if (!gunInstance.CanShoot())
            return;

        // update state
        gunInstance.Shoot();

        // shoot bullet
        EmitRay(input);

        // play sound
        // TODO: change depending on silencer
        audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(Toolbox.RandomFromList(gunInstance.baseGun.shootSounds));

        // flash
        // TODO: change depending on silencer
        muzzleFlashLight.enabled = true;
        pixelPicker.localLightOverride = true;
        StartCoroutine(Toolbox.RunAfterTime(0.1f, () => {
            muzzleFlashLight.enabled = false;
            pixelPicker.localLightOverride = false;
        }));

        // muzzleflash obj
        // TODO: change depending on silencer
        GameObject muzzleFlashObj = GameObject.Instantiate(
            gunInstance.baseGun.muzzleFlash,
            gunPosition() + 0.5f * motor.CharacterForward - new Vector3(0, 0.1f, 0),
            Quaternion.LookRotation(transform.up, gunDirection(input))
            );
        muzzleFlashObj.transform.localScale = gunInstance.baseGun.muzzleflashSize * Vector3.one;
        GameObject.Destroy(muzzleFlashObj, 0.05f);

        // shell casing
        if (gunInstance.baseGun.type != GunType.shotgun) {
            EmitShell();
        }
    }
    public void EmitShell() {
        // TODO: don't emit shell if the chamber is empty

        GameObject shell = GameObject.Instantiate(
            gunInstance.baseGun.shellCasing,
            gunPosition() + 0.2f * transform.right + 0.2f * transform.forward,
            Quaternion.identity
        );
        Rigidbody body = shell.GetComponent<Rigidbody>();
        body.AddRelativeForce(
            UnityEngine.Random.Range(0.5f, 1.5f) * transform.up +
            UnityEngine.Random.Range(0.1f, 1f) * transform.right +
            UnityEngine.Random.Range(-0.3f, 0.3f) * transform.forward,
            ForceMode.Impulse); // TODO: what does force mode mean?
        body.AddRelativeTorque(UnityEngine.Random.Range(100f, 600f) * shell.transform.forward);
    }
    public void Aim() {
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.aimSounds);
    }
    public void Rack() {
        EmitShell();
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.rackSounds);
        gunInstance.Rack();
    }
    public void Reload() {
        gunInstance.Reload();

        // drop clip

        // play sound
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.clipOut);

        // start animation
    }
    private void SwitchGun(GunInstance instance) {
        if (instance == null || instance == gunInstance)
            return;
        gunInstance = instance;

        // TODO:
        gunAnimation.Unholster();

        // TODO: don't call this! maybe?
        gunAnimation.EndShoot();
        Toolbox.RandomizeOneShot(audioSource, gunInstance.baseGun.unholster);
    }
    public void Holster() {
        gunInstance = null;
        gunAnimation.Holster();
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

    public Vector3 ProcessInput(PlayerCharacterInputs input) {
        gunAnimation.input = input.Fire;

        if (input.switchToGun != -1) {
            SwitchToGun(input.switchToGun);
        } else if (input.reload) {
            Reload();
        } else if (gunInstance != null && gunInstance.CanShoot()) {

            if (gunInstance.baseGun.automatic) {
                if (input.Fire.FirePressed && !shooting) {
                    gunAnimation.StartShooting();
                    shooting = true;
                }
                if (input.Fire.FireHeld) {
                    return CursorToTargetPoint(input.Fire);
                }
                if (shooting && !input.Fire.FireHeld) {
                    gunAnimation.EndShoot();
                    shooting = false;
                    return CursorToTargetPoint(input.Fire);
                }
            } else { // semiautomatic
                if (input.Fire.FirePressed) {//&& !shooting) {
                    gunAnimation.StartShooting();
                    shooting = true;
                    return CursorToTargetPoint(input.Fire);
                }
            }
        } else {

            // cancel out the shoot here for automatics

        }


        return Vector2.zero;
    }
}