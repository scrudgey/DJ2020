using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class DoorRandomizer : MonoBehaviour {
    public DoorRandomizerTemplate template;
    [Header("door objects")]
    public Door door;
    public bool locked = true;
    public AttackSurfaceLatch[] latches;
    public AttackSurfaceLock[] doorLocks;
    public AttackSurfaceLatchGuard[] latchGuards;
    public GameObject innerDeadbolt;
    public GameObject outerDeadbolt;
    public DoorLock deadboltLock;

    public void ApplyState(LevelTemplate levelTemplate) {
        bool latchesEnabled = template.getLatchesEnabled(levelTemplate.securityLevel);
        bool latchesVulnerable = template.getLatchesVulnerable(levelTemplate.securityLevel);
        bool latchGuardEnabled = template.getLatchGuardEnabled(levelTemplate.securityLevel);
        bool latchGuardScrews = template.getLatchGuardScrews(levelTemplate.securityLevel);
        bool deadboltEnabled = template.getDeadboltEnabled(levelTemplate.securityLevel);
        bool autoClose = template.getAutoClose(levelTemplate.securityLevel);


        foreach (AttackSurfaceLatch latch in latches) {
            if (latch == null) continue;
            latch.gameObject.SetActive(latchesEnabled);
            latch.vulnerable = latchesVulnerable;
        }
        foreach (AttackSurfaceLatchGuard guard in latchGuards) {
            guard.gameObject.SetActive(latchGuardEnabled && latchesEnabled);
            if (latchGuardEnabled)
                guard.Configure(latchGuardScrews);
        }

        foreach (AttackSurfaceLock knob in doorLocks) {
            knob.setbackProb = template.knobLock.getSetbackProb(levelTemplate.securityLevel);
            knob.progressStages = template.knobLock.getProgressStages(levelTemplate.securityLevel);
            knob.doorLock.locked = locked;
        }
        door.autoClose = autoClose;

        if (innerDeadbolt != null)
            innerDeadbolt.SetActive(deadboltEnabled);
        if (outerDeadbolt != null)
            outerDeadbolt.SetActive(deadboltEnabled);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos() {
        GUIStyle myStyle = new GUIStyle();
        myStyle.normal.textColor = template.color;
        Handles.Label(transform.position, $"{template.doorName}", myStyle);
    }
#endif

}
