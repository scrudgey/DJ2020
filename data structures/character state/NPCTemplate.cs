using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
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
}