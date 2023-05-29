using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceWire : MonoBehaviour {
    public AlarmComponent[] alarmComponents;
    public AlarmComponent[] alarmComponentsToActivate;
    public SecurityCamera securityCamera;
    public string resultText;
    public BurglarAttackResult DoCut() {
        BurglarAttackResult result = BurglarAttackResult.None;
        foreach (AlarmComponent component in alarmComponents) {
            GameManager.I.SetNodeEnabled<AlarmComponent, AlarmNode>(component, false);
            result = BurglarAttackResult.None with {
                success = true,
                feedbackText = resultText
            };
        }
        foreach (AlarmComponent component in alarmComponentsToActivate) {
            GameManager.I.SetAlarmNodeState(component, true);
            result = BurglarAttackResult.None with {
                success = true,
                feedbackText = resultText
            };
        }
        if (securityCamera != null) {
            securityCamera.doRotate = false;
            result = BurglarAttackResult.None with {
                success = true,
                feedbackText = resultText
            };
        }

        return result;
    }
}
