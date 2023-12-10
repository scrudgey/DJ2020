using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceDataPort : AttackSurfaceElement, INodeBinder<CyberNode> {
    public CyberNode node { get; set; }
    public AudioSource audioSource;
    public AudioClip attachSound;
    public CyberDataStore dataStore;

    public void HandleNodeChange() { }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.usb) {
            Toolbox.RandomizeOneShot(audioSource, attachSound);
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = "connected dataport",
                element = this,
                attachedDataStore = dataStore,
                attachedCyberNode = node
            };
        }
        return BurglarAttackResult.None;
    }
}
