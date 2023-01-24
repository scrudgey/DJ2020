using System;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueInput {
    public GameObject playerObject;
    public GameObject npcObject;
    public SphereRobotAI NPCAI;
    public PlayerState playerState;
    public LevelState levelState;
    public Dictionary<String, SuspicionRecord> suspicionRecords;
    public Suspiciousness playerSuspiciousness;
    public bool playerInDisguise;
    public bool playerHasID;
    public bool alarmActive;
    public int playerSpeechSkill;
}