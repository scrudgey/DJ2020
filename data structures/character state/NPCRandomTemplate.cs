using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/NPCRandomTemplate")]
public class NPCRandomTemplate : ScriptableObject {
    public string dialogueName;
    // skin
    public string[] legSkins;
    public string[] bodySkins;
    public string[] headSkins;
    // gun
    public GunTemplate[] primaryGuns;
    public GunTemplate[] secondaryGuns;
    public GunTemplate[] tertiaryGuns;

    // health
    public LoHi fullHealthAmountRange;
    public LoHi armorLevel;


    // speech
    public Sprite portrait;
    public List<string> grammarFiles;
    public NPCDialogueParameters dialogueParameters;

    public Alertness alertness;
    public bool radioChatter;

    public NPCTemplate toTemplate() {
        NPCTemplate template = ScriptableObject.CreateInstance<NPCTemplate>();
        template.dialogueName = dialogueName;
        template.legSkin = Toolbox.RandomFromList(legSkins);
        template.bodySkin = Toolbox.RandomFromList(bodySkins);
        template.headSkin = Toolbox.RandomFromList(headSkins);

        template.primaryGun = Toolbox.RandomFromList(primaryGuns);
        template.secondaryGun = Toolbox.RandomFromList(secondaryGuns);
        template.tertiaryGun = Toolbox.RandomFromList(tertiaryGuns);

        template.fullHealthAmount = fullHealthAmountRange.GetRandomInsideBound();
        template.health = template.fullHealthAmount;

        template.armorLevel = (int)armorLevel.GetRandomInsideBound();

        template.etiquettes = new SpeechEtiquette[0];

        template.portrait = portrait;

        template.grammarFiles = grammarFiles;

        template.dialogueParameters = dialogueParameters;

        template.physicalKeys = new List<int>();

        template.alertness = alertness;

        template.radioChatter = radioChatter;

        return template;
    }
}