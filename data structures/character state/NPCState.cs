using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/NPCState")]
public class NPCState : ScriptableObject, ISkinState, IGunHandlerState, ICharacterHurtableState {

    // skin
    [field: SerializeField]
    public string legSkin { get; set; }
    [field: SerializeField]
    public string bodySkin { get; set; }

    // gun
    [field: SerializeField]
    public GunInstance primaryGun { get; set; }
    [field: SerializeField]
    public GunInstance secondaryGun { get; set; }
    [field: SerializeField]
    public GunInstance tertiaryGun { get; set; }
    [field: SerializeField]
    public int activeGun { get; set; }
    // health
    public float health { get; set; }
    [field: SerializeField]
    public float fullHealthAmount { get; set; }
    public HitState hitState { get; set; }

    public void ApplyState(GameObject npcObject) {
        ((IGunHandlerState)this).ApplyGunState(npcObject);
        ((ISkinState)this).ApplySkinState(npcObject);
        ((ICharacterHurtableState)this).ApplyHurtableState(npcObject);
    }

    public static NPCState DefaultNPCState() {
        Gun gun1 = Gun.Load("rifle");
        Gun gun2 = Gun.Load("pistol");
        Gun gun3 = Gun.Load("shotgun");

        return new NPCState() {
            primaryGun = new GunInstance(gun1),
            secondaryGun = new GunInstance(gun2),
            tertiaryGun = new GunInstance(gun3),
            activeGun = 1,

            // legSkin = "generic64",
            // // legSkin = "cyber",
            legSkin = "Jack",
            bodySkin = "Jack",
            health = 100,
            fullHealthAmount = 100
        };
    }
}