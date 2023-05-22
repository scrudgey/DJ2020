using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceDataPort : AttackSurfaceElement {
    public AudioSource audioSource;
    public AudioClip attachSound;
    public CyberDataStore dataStore;
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.usb) {
            Toolbox.RandomizeOneShot(audioSource, attachSound);
            return new BurglarAttackResult {
                success = true,
                feedbackText = "connected dataport",
                element = this,
                attachedDataStore = dataStore
            };
        }
        return BurglarAttackResult.None;
    }
}
