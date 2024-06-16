using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceInputChip : AttackSurfaceElement {
    public DoorLock doorLock;
    public KeycardReader keycardReader;
    public ElevatorController elevatorController;
    public void Initialize(DoorLock doorLock, ElevatorController elevatorController) {
        this.doorLock = doorLock;
        this.elevatorController = elevatorController;
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.usb) {
            // Toolbox.RandomizeOneShot(audioSource, attachSound);
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = "connected to input chip",
                element = this,
            };
        } else {
            return BurglarAttackResult.None;
        }
    }
}
