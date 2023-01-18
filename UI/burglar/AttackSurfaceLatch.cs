using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceLatch : AttackSurfaceElement {
    public Door door;
    public bool vulnerable;
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.probe) {
            return HandleProbe(data);
        }
        return BurglarAttackResult.None;
    }

    BurglarAttackResult HandleProbe(BurgleTargetData data) {
        if (vulnerable) {
            door.Unlatch();
            bool doPush = door.state == Door.DoorState.closed;
            if (doPush) {
                door.PushOpenSlightly(data.burglar.transform);
                return new BurglarAttackResult {
                    success = true,
                    feedbackText = "Door latch bypassed"
                };
            } else return BurglarAttackResult.None;
        } else {
            return new BurglarAttackResult {
                success = false,
                feedbackText = "Door latch hardened"
            };
        }
    }
}
