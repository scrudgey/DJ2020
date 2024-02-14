using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCState : ICharacterHurtableState { //IGunHandlerState
    public NPCTemplate template;
    // gun
    public GunState primaryGun { get; set; }
    public GunState secondaryGun { get; set; }
    public GunState tertiaryGun { get; set; }
    public int activeGun { get; set; }
    public int numberOfShellsPerReload { get { return 1; } }

    // health
    public float health { get; set; }
    public float fullHealthAmount { get; set; }
    public HitState hitState { get; set; }

    public static NPCState Instantiate(NPCTemplate template) => new NPCState {
        template = template,
        primaryGun = GunState.Instantiate(template.primaryGun),
        secondaryGun = GunState.Instantiate(template.secondaryGun),
        tertiaryGun = GunState.Instantiate(template.tertiaryGun),
        activeGun = 0,
        health = template.fullHealthAmount,
        fullHealthAmount = template.fullHealthAmount,
        hitState = template.hitState
    };

    public void ApplyState(GameObject npcObject) {
        // this.health = fullHealthAmount;
        // ((IGunHandlerState)this).ApplyGunState(npcObject);
        ((ISkinState)template).ApplySkinState(npcObject);
        ((ICharacterHurtableState)this).ApplyHurtableState(npcObject);

        CharacterController controller = npcObject.GetComponent<CharacterController>();
        if (controller != null) {
            WeaponState weapon1 = template.primaryGun != null ? new WeaponState(GunState.Instantiate(template.primaryGun)) : null;
            WeaponState weapon2 = template.secondaryGun != null ? new WeaponState(GunState.Instantiate(template.secondaryGun)) : null;
            WeaponState weapon3 = template.tertiaryGun != null ? new WeaponState(GunState.Instantiate(template.tertiaryGun)) : null;

            controller.primaryWeapon = weapon1;
            controller.secondaryWeapon = weapon2;
            controller.tertiaryWeapon = weapon3;

            if (weapon1 != null) {
                controller.HandleSwitchWeapon(1);
            } else {
                controller.HandleSwitchWeapon(0);
            }
        }

        SphereRobotAI ai = npcObject.GetComponent<SphereRobotAI>();
        if (ai != null) {
            ai.physicalKeys = new HashSet<int>(template.physicalKeys);
            ai.alertness = template.alertness;
            ai.etiquettes = template.etiquettes;
            // ai.portrait = template.portrait;
            ai.dialogueName = template.dialogueName;

            ai.speechTextController.portrait = template.portrait;
            ai.speechTextController.grammarFiles = template.grammarFiles;
        }

        WorkerNPCAI workerNPCAI = npcObject.GetComponent<WorkerNPCAI>();
        if (workerNPCAI != null) {
            // workerNPCAI.physicalKeys = new HashSet<int>(template.physicalKeys);
            workerNPCAI.alertness = template.alertness;
            workerNPCAI.etiquettes = template.etiquettes;
            // workerNPCAI.portrait = template.portrait;
            workerNPCAI.dialogueName = template.dialogueName;
            workerNPCAI.speechTextController.portrait = template.portrait;
            workerNPCAI.speechTextController.grammarFiles = template.grammarFiles;

        }

        LevelRandomSound randomSound = npcObject.GetComponent<LevelRandomSound>();
        if (randomSound != null) {
            randomSound.enabled = template.radioChatter;
        }
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