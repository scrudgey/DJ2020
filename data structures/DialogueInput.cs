using System;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueInput {
    public GameObject playerObject;
    public GameObject npcObject;
    public DialogueCharacterInput npcCharacter;
    public PlayerState playerState;
    public LevelState levelState;
    public Dictionary<String, SuspicionRecord> suspicionRecords;
    public Suspiciousness playerSuspiciousness;
    public bool playerInDisguise;
    public bool playerHasID;
    public bool alarmActive;
    public int playerSpeechSkill;
}

public struct DialogueCharacterInput {
    public Sprite portrait;
    public Alertness alertness;
    public SpeechEtiquette[] etiquettes;
}