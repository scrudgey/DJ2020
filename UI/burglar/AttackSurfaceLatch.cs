using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceLatch : AttackSurfaceElement {
    public Door door;
    public override void HandleAttack(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleAttack(activeTool, data);
        if (activeTool == BurglarToolType.probe) {
            bool doPush = door.state == Door.DoorState.closed;
            door.Unlatch();
            if (doPush) door.PushOpenSlightly(data.burglar.transform);
        }
    }
}
