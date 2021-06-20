using UnityEngine;

[System.Serializable]
public class GunInstance {
    public Gun baseGun;
    public int clip;
    public int chamber;
    public float cooldownTimer;
    public GunInstance(Gun baseGun) {
        this.baseGun = baseGun;
        this.clip = baseGun.clipSize; // TODO: start empty?
        this.chamber = 0;
    }
    public bool CanShoot() {
        // check cooldown, clip, chamber

        // return chamber != 0;
        return chamber > 0;
    }
    public int TotalAmmo() {
        return clip + chamber;
    }
    public void Shoot() {
        if (!CanShoot()) {
            return;
        }
        cooldownTimer = baseGun.shootInterval;
        chamber = 0;
        if (clip > 0 && baseGun.automatic) { // automatic chambering
            Rack();
        }
    }
    public void Update() {
        if (cooldownTimer > 0) {
            cooldownTimer -= Time.deltaTime;
        }
    }
    public void Rack() {
        if (chamber == 0 && clip > 0) {
            clip--;
            chamber++;
        }
    }
    public void Reload() {
        clip = baseGun.clipSize;
    }
}