using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/NPCDialogueParameters")]
public class NPCDialogueParameters : ScriptableObject {
    public SuspicionDialogueParameters IdentitySuspicionParams;
    public SuspicionDialogueParameters GunshotHeardParams;
    public SuspicionDialogueParameters CrawlingParams;
    public SuspicionDialogueParameters PlacedSuspiciousObjectParams;
    public SuspicionDialogueParameters RobbedRegisterParams;
    public SuspicionDialogueParameters BodyFoundParams;
    public SuspicionDialogueParameters FledQuestioningParams;
    public SuspicionDialogueParameters SomeoneWasShotParams;
    public SuspicionDialogueParameters SuspiciousNoiseParams;
    public SuspicionDialogueParameters ExplosionParams;
    public SuspicionDialogueParameters BrandishingWeaponParams;
    public SuspicionDialogueParameters TrippedAlarmParams;
    public SuspicionDialogueParameters ShotsFiredParams;
    public SuspicionDialogueParameters TamperingParams;
    public SuspicionDialogueParameters TamperEvidenceParams;
    public SuspicionDialogueParameters LootParams;
}