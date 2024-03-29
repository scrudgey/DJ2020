using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/NPCTemplate")]
public class NPCTemplate : ScriptableObject, IGunHandlerTemplate, ISkinState, ICharacterHurtableState {
    public string dialogueName;
    // skin
    [field: SerializeField]
    public string legSkin { get; set; }
    [field: SerializeField]
    public string bodySkin { get; set; }
    [field: SerializeField]
    public string headSkin { get; set; }

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
    [field: SerializeField]
    public int armorLevel { get; set; }


    // speech
    [field: SerializeField]
    public SpeechEtiquette[] etiquettes;
    [field: SerializeField]
    public Sprite portrait;
    public List<string> grammarFiles;
    public NPCDialogueParameters dialogueParameters;

    public List<int> physicalKeys;

    public Alertness alertness;
    public bool radioChatter;
}