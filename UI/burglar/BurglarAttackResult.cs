using System.Collections.Generic;
using UnityEngine;

public record BurglarAttackResult {
    public bool success;
    public string feedbackText;
    public bool finish;
    public bool makeTamperEvidenceSuspicious;
    public bool revealTamperEvidence;
    public bool hideTamperEvidence;
    public NoiseData noiseData;
    public AttackSurfaceElement element;
    public CyberDataStore attachedDataStore;
    public List<Vector3> lockPositions;
    public AttackSurfaceVentCover panel;
    public ElectricalDamage electricDamage;
    public static BurglarAttackResult None => new BurglarAttackResult {
        success = false,
        feedbackText = "",
        lockPositions = new List<Vector3>()
    };
}