using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public record GunDelta {
    public int clip;
    public int chamber;
    public float cooldownTimer;
    public List<GunMod> activeMods;
    public List<GunPerk> perks;

    public static GunDelta From(GunTemplate template, bool applyRandomPerks = false) {
        if (template != null) {
            List<GunPerk> perks = new List<GunPerk>();
            if (applyRandomPerks) {
                foreach (GunPerk perk in template.possiblePerks) {
                    if (Random.Range(0f, 1f) < perk.probability) {
                        perks.Add(perk);
                    }
                }
            }
            return GunDelta.Empty() with {
                clip = template.clipSize,
                perks = perks
            };
        } else {
            return GunDelta.Empty();
        }
    }
    public bool CanShoot() {
        // check cooldown, clip, chamber
        return chamber > 0 && cooldownTimer <= 0;
    }
    public int TotalAmmo() {
        return clip + chamber;
    }

    public bool CanRack() {
        return clip > 0 && chamber == 0;
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

    public static GunDelta Empty() => new GunDelta {
        clip = 0,
        chamber = 0,
        cooldownTimer = 0,
        activeMods = new List<GunMod>(),
        perks = new List<GunPerk>()
    };

    // Save

    // Load
}