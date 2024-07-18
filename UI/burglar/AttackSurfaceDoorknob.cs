using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AttackSurfaceDoorknob : AttackSurfaceElement {
    public Door door;
    public List<DoorLock> doorLocks;
    public AudioSource audioSource;

    public bool IsLocked() => doorLocks.Where(doorLock => doorLock.isActiveAndEnabled).Any(doorLock => doorLock.locked); // doorLock.locked;

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            bool success = !IsLocked();
            door.ActivateDoorknob(data.burglar.transform.position, data.burglar.transform, doorLocks);
            if (success) {
                return BurglarAttackResult.None with {
                    finish = success,
                };
            } else {
                List<Vector3> lockPositions = new List<Vector3>();
                foreach (DoorLock doorLock in doorLocks) {
                    if (doorLock.isActiveAndEnabled && doorLock.locked) {
                        lockPositions.Add(doorLock.transform.position);
                    }
                }

                return BurglarAttackResult.None with {
                    finish = success,
                    success = false,
                    feedbackText = "locked",
                    lockPositions = lockPositions
                };
            }
        }

        return BurglarAttackResult.None;
    }

    public void ActivateDoorKnob() {

    }
}
