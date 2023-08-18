using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRandomizer : MonoBehaviour {
    public Door door;
    public bool locked = true;
    public AttackSurfaceLatch[] latches;
    // public AttackSurfaceDoorknob[] knobs;
    public AttackSurfaceLock[] doorLocks;
    public AttackSurfaceLatchGuard[] latchGuards;
    public GameObject innerDeadbolt;
    public GameObject outerDeadbolt;
    public DoorLock deadboltLock;

    // void Start() {
    //     ApplyState();
    // }
    public void ApplyState(LevelTemplate levelTemplate) {
        bool latchesEnabled = Random.Range(0f, 1f) < 0.75f;
        bool latchesVulnerable = Random.Range(0f, 1f) < 0.8f;
        bool latchGuardEnabled = Random.Range(0f, 1f) < 0.5f;
        bool latchGuardScrews = Random.Range(0f, 1f) < 0.75f;
        bool deadboltEnabled = Random.Range(0f, 1f) < 0.5f;
        bool autoClose = Random.Range(0f, 1f) < 0.5f;

        if (latchesEnabled) {
            foreach (AttackSurfaceLatch latch in latches) {
                latch.gameObject.SetActive(true);
                latch.vulnerable = latchesVulnerable;
            }
            if (latchGuardEnabled) {
                foreach (AttackSurfaceLatchGuard guard in latchGuards) {
                    guard.gameObject.SetActive(true);
                    guard.Configure(latchGuardScrews);
                }
            }
        } else {
            foreach (AttackSurfaceLatch latch in latches) {
                if (latch == null) continue;
                latch.gameObject.SetActive(false);
            }
            foreach (AttackSurfaceLatchGuard guard in latchGuards) {
                guard.gameObject.SetActive(false);
            }
        }

        foreach (AttackSurfaceLock knob in doorLocks) {
            if (levelTemplate.securityLevel == LevelTemplate.SecurityLevel.lax) {
                knob.setbackProb = Random.Range(0f, 0.2f);
                knob.progressStages = Random.Range(1, 3);
            } else if (levelTemplate.securityLevel == LevelTemplate.SecurityLevel.commercial) {
                knob.setbackProb = Random.Range(0.05f, 0.45f);
                knob.progressStages = Random.Range(1, 4);
            } else if (levelTemplate.securityLevel == LevelTemplate.SecurityLevel.hardened) {
                knob.setbackProb = Random.Range(0.1f, 0.7f);
                knob.progressStages = Random.Range(3, 5);
            }
            knob.doorLock.locked = locked;
        }
        door.autoClose = autoClose;

        if (deadboltEnabled) {
            if (innerDeadbolt != null)
                innerDeadbolt.SetActive(true);
            if (outerDeadbolt != null)
                outerDeadbolt?.SetActive(true);
        } else {
            if (innerDeadbolt != null)
                innerDeadbolt?.SetActive(false);
            if (outerDeadbolt != null)
                outerDeadbolt?.SetActive(false);
        }
    }
}
