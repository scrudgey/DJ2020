using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceDataPort : AttackSurfaceElement {
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.usb) {
            return new BurglarAttackResult {
                success = true,
                feedbackText = "connected dataport",
                element = this
            };
        }
        return BurglarAttackResult.None;
    }
}
