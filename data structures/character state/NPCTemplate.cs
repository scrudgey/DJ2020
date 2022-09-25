using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/NPCTemplate")]
public class NPCTemplate : ScriptableObject, IGunHandlerTemplate, ISkinState, ICharacterHurtableState {

    // skin
    [field: SerializeField]
    public string legSkin { get; set; }
    [field: SerializeField]
    public string bodySkin { get; set; }

    // gun
    [field: SerializeField]
    public GunTemplate primaryGun { get; set; }
    [field: SerializeField]
    public GunTemplate secondaryGun { get; set; }
    [field: SerializeField]
    public GunTemplate tertiaryGun { get; set; }


    // health
    public float health { get; set; }

    [field: SerializeField]
    public float fullHealthAmount { get; set; }
    public HitState hitState { get; set; }

    // public static NPCTemplate Default() => new NPCTemplate() {
    //     primaryGun = GunTemplate.Load("r1"),
    //     secondaryGun = GunTemplate.Load("p1"),
    //     tertiaryGun = GunTemplate.Load("sh1"),
    //     // legSkin = "generic64",
    //     // // legSkin = "cyber",
    //     legSkin = "Jack",
    //     bodySkin = "Jack",
    //     health = 100,
    //     fullHealthAmount = 100
    // };
}