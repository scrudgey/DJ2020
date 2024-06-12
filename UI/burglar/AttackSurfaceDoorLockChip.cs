using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackSurfaceDoorLockChip : AttackSurfaceElement {
    public DoorLock doorLock;
    public List<int> waveformPermutation;

    public void Initialize(DoorLock doorLock) {
        this.doorLock = doorLock;
        waveformPermutation = Enumerable.Range(0, 9).OrderBy(item => Random.Range(0f, 1f)).ToList();
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.usb) {
            // Toolbox.RandomizeOneShot(audioSource, attachSound);
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = "connected to ram chip",
                element = this,
            };
        } else {
            return BurglarAttackResult.None;
        }
    }

}
