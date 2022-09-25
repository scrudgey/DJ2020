using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCState : ICharacterHurtableState, IGunHandlerState {

    public NPCTemplate template;
    // gun
    public GunState primaryGun { get; set; }
    public GunState secondaryGun { get; set; }
    public GunState tertiaryGun { get; set; }
    public int activeGun { get; set; }

    // health
    public float health { get; set; }
    public float fullHealthAmount { get; set; }
    public HitState hitState { get; set; }

    public static NPCState Instantiate(NPCTemplate template) => new NPCState {
        template = template,
        primaryGun = GunState.Instantiate(template.primaryGun),
        secondaryGun = GunState.Instantiate(template.secondaryGun),
        tertiaryGun = GunState.Instantiate(template.tertiaryGun),
        activeGun = 1,
        health = template.fullHealthAmount,
        fullHealthAmount = template.fullHealthAmount,
        hitState = template.hitState
    };

    public void ApplyState(GameObject npcObject) {
        // this.health = fullHealthAmount;
        ((IGunHandlerState)this).ApplyGunState(npcObject);
        ((ISkinState)template).ApplySkinState(npcObject);
        ((ICharacterHurtableState)this).ApplyHurtableState(npcObject);
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