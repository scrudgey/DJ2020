using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRandomizer : MonoBehaviour {
    public Door door;
    public AttackSurfaceLatch[] latches;
    public AttackSurfaceDoorknob[] knobs;
    public AttackSurfaceLatchGuard[] latchGuards;
    public GameObject innerDeadbolt;
    public GameObject outerDeadbolt;
    public DoorLock deadboltLock;

    void Start() {
        ApplyState();
    }
    void ApplyState() {
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

        foreach (AttackSurfaceDoorknob knob in knobs) {
            knob.setbackProb = Random.Range(0f, 0.6f);
            knob.progressStages = Random.Range(1, 4);
        }
        door.autoClose = autoClose;

        if (deadboltEnabled) {
            innerDeadbolt?.SetActive(true);
            outerDeadbolt?.SetActive(true);
        } else {
            door.doorLocks?.Remove(deadboltLock);
            innerDeadbolt?.SetActive(false);
            outerDeadbolt?.SetActive(false);
        }
    }
}
