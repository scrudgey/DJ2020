using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceLatch : AttackSurfaceElement {
    public Door door;
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.probe) {
            bool doPush = door.state == Door.DoorState.closed;
            door.Unlatch();
            if (doPush) {
                door.PushOpenSlightly(data.burglar.transform);
                return new BurglarAttackResult {
                    success = true,
                    feedbackText = "Door latch bypassed"
                };
            }
        }
        return BurglarAttackResult.None;
    }
}
