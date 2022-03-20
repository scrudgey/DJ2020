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
    public int MaxAmmo() {
        return baseGun.clipSize;
    }
    public void Shoot() {
        if (!CanShoot()) {
            return;
        }
        cooldownTimer = baseGun.shootInterval;
        chamber = 0;
        if (CheckRack()) {
            Rack();
        }
    }
    public bool CheckRack() {
        return clip > 0 && baseGun.cycle == CycleType.semiautomatic || baseGun.cycle == CycleType.automatic;
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
    public void ClipOut() {
        clip = 0;
    }
    public void ClipIn() {
        clip = baseGun.clipSize;
    }
    public void ShellIn() {
        if (clip < baseGun.clipSize) {
            clip++;
        }
    }
    public NoiseData GetShootNoise() {
        return baseGun.shootNoise();
    }
}