using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GunState : IGunStatProvider {
    public GunDelta delta;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<GunTemplate>))]
    public GunTemplate template;

    public GunState() {
        // needed for serialization
    }

    public void Shoot() {
        if (!CanShoot()) {
            return;
        }
        if (template.cycle == CycleType.manual || template.cycle == CycleType.semiautomatic)
            delta.cooldownTimer = getShootInterval();
        delta.chamber = 0;
        if (ShouldRack()) {
            Rack();
        }
    }
    public int GetCost() => template.baseCost + delta.perks.Select(perk => perk.cost).Sum();
    public int MaxAmmo() => getClipSize();

    public bool CanShoot() => delta.CanShoot();

    public int TotalAmmo() => delta.TotalAmmo();

    public void Update() => delta.Update();

    public bool IsEmpty() => delta.clip <= 0 && delta.chamber <= 0;

    public bool ShouldRack() {
        if (template.cycle == CycleType.semiautomatic || template.cycle == CycleType.automatic)
            return delta.CanRack();
        else return false;
    }

    public void Rack() => delta.Rack();
    public void ClipOut() => delta.ClipOut();

    public void ClipIn() {
        delta.clip = getClipSize();
    }
    public void ShellIn(int number) {
        delta.clip += number;
        delta.clip = Mathf.Min(delta.clip, getClipSize());
    }
    public NoiseData GetShootNoise() => shootNoise();

    public static GunState Instantiate(GunTemplate template, bool applyRandomPerks = false) => new GunState {
        template = template,
        delta = GunDelta.From(template, applyRandomPerks)
    };
    // public static GunState Instantiate(GunTemplate template, GunDelta delta) {
    //     GunState state = Instantiate(template);
    //     state.ApplyDelta(delta);
    //     return state;
    // }
    public void ApplyDelta(GunDelta newDelta) {
        this.delta = newDelta;
    }
    public GunStats GetGunStats() {
        GunStats templateStats = template.GetGunStats();
        foreach (GunMod mod in delta.activeMods) {
            templateStats += mod.GetGunStats();
        }
        foreach (GunPerk perk in delta.perks.Concat(template.intrinsicPerks)) {
            templateStats += perk.GetGunStats();
        }
        return templateStats;
    }

    public GunState Copy() => new GunState() {
        template = template,
        delta = delta with { activeMods = new System.Collections.Generic.List<GunMod>(delta.activeMods) }
    };

    public NoiseData shootNoise() {
        if (getSilencer()) {
            return new NoiseData() {
                volume = getNoise() / 10f,
                suspiciousness = Suspiciousness.suspicious,
                pitch = getPitch(),
                isGunshot = true
            };
        } else {
            return new NoiseData() {
                volume = getNoise(),
                suspiciousness = Suspiciousness.aggressive,
                pitch = getPitch(),
                isGunshot = true
            };
        }
    }

    public float getBaseDamage() => (delta.activeMods.Select(mod => mod.baseDamage))
                    .Concat(delta.perks.Select(perk => perk.baseDamage))
                    .Aggregate(template.baseDamage, (current, next) => current + next)
                    .GetRandomInsideBound();

    public bool getSilencer() =>
        (template.silencer || delta.activeMods.Any(mod => mod.type == GunModType.silencer));

    public float getNoise() => template.noise + delta.activeMods.Select(mod => mod.noise).Sum() + delta.perks.Select(perk => perk.noise).Sum();

    public int getClipSize() => template.clipSize + delta.activeMods.Select(mod => mod.clipSize).Sum() + delta.perks.Select(perk => perk.clipSize).Sum();

    public float getPitch() {
        // TODO:: apply mods
        return template.pitch;
    }
    public float getShootInterval() => template.shootInterval + delta.activeMods.Select(mod => mod.shootInterval).Sum() + delta.perks.Select(perk => perk.shootInterval).Sum();

    public float getMuzzleFlashSize() {
        return template.muzzleflashSize;
    }
    public float getWeight() {
        return template.weight + delta.activeMods.Select(mod => mod.weight).Sum() + delta.perks.Select(perk => perk.weight).Sum();
    }
    public float getSpread() => template.spread + delta.activeMods.Select(mod => mod.spread).Sum() + delta.perks.Select(perk => perk.spread).Sum();

    public float getRange() {
        return template.range;
    }
    public int getPiercing() {
        return template.piercing + delta.activeMods.Select(mod => mod.armorPiercing).Sum() + delta.perks.Select(perk => perk.armorPiercing).Sum();
    }
    public float getLockOnSize() {
        return template.lockOnSize + delta.activeMods.Select(mod => mod.lockOnSize).Sum() + delta.perks.Select(perk => perk.lockOnSize).Sum();
    }
    public LoHi getRecoil() {
        return template.recoil;
    }
    public AudioClip[] GetShootSounds() {
        return getSilencer() ? template.silencedSounds : template.shootSounds;
    }

    public Sprite GetSprite() {
        List<string> allRequiredSuffixes = delta.activeMods
            .Select(mod => mod.requiredSpriteSuffix)
            .Where(suffix => suffix != null && !suffix.Equals(""))
            .ToList();

        // Debug.Log($"all required suffixes: {string.Join(",", allRequiredSuffixes)}");
        Dictionary<string, Sprite> sprites = template.images.ToDictionary(sprite => sprite.name, sprite => sprite);

        // Debug.Log($"all sprites: {string.Join(",", sprites.Keys)}");
        string shortestKey = sprites.Keys
            .Where(name => allRequiredSuffixes.All(suffix => name.Contains(suffix)))
            .OrderBy(c => c.Length)
            .FirstOrDefault();

        // Debug.Log($"shortest key: {shortestKey}");
        if (shortestKey == null) {
            Debug.LogWarning($"could not find gunsprite with required suffixes: {string.Join(",", allRequiredSuffixes)}");
            return template.images[0];
        } else {
            return sprites[shortestKey];
        }
    }

    public string getShortName() {
        int numberOfMods = delta.activeMods.Count;
        string modSuffix = new string('+', numberOfMods);
        return $"{template.shortName}{modSuffix}";
    }
    public string getName() {
        int numberOfMods = delta.activeMods.Count;
        string modSuffix = new string('+', numberOfMods);
        return $"{template.name}{modSuffix}";
    }

}