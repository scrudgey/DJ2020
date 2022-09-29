using UnityEngine;

public class GunState {
    public GunDelta delta;
    public GunTemplate template;

    public void Shoot() {
        if (!CanShoot()) {
            return;
        }
        delta.cooldownTimer = template.shootInterval;
        delta.chamber = 0;
        if (ShouldRack()) {
            Rack();
        }
    }
    public int MaxAmmo() => template.clipSize;

    public bool CanShoot() => delta.CanShoot();

    public int TotalAmmo() => delta.TotalAmmo();

    public void Update() => delta.Update();

    public bool ShouldRack() {
        if (template.cycle == CycleType.semiautomatic || template.cycle == CycleType.automatic)
            return delta.CanRack();
        else return false;
    }

    public void Rack() => delta.Rack();
    public void ClipOut() => delta.ClipOut();

    public void ClipIn() {
        delta.clip = template.clipSize;
    }
    public void ShellIn() {
        if (delta.clip < template.clipSize) {
            delta.clip++;
        }
    }
    public NoiseData GetShootNoise() => template.shootNoise();

    public static GunState Instantiate(GunTemplate template) => new GunState {
        template = template,
        delta = GunDelta.From(template)
    };
    public static GunState Instantiate(GunTemplate template, GunDelta delta) {
        GunState state = Instantiate(template);
        state.ApplyDelta(delta);
        return state;
    }
    public void ApplyDelta(GunDelta newDelta) {
        this.delta = newDelta;
    }
    // public void Save() {
    //  save template path
    //  GunDelta.Save()
    // }
    // public static GunState Load() {
    //     // load template
    //     // load delta
    //     // return instantiate
    // }
}