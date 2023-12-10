using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AlarmNode : Node<AlarmNode> {
    public enum AlarmNodeType { normal, terminal, radio }
    public enum AlarmOverrideState { none, disabled, enabled }
    public AlarmNodeType nodeType;
    public AlarmOverrideState overrideState;
    public bool alarmTriggered;
    public float countdownTimer;
    public override bool getEnabled() {
        if (overrideState == AlarmOverrideState.disabled) {
            return false;
        } else if (overrideState == AlarmOverrideState.enabled) {
            return true;
        } else {
            return base.getEnabled();
        }
    }
    public AlarmNode() { }

    public void Update() {
        if (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0) {
                alarmTriggered = false;
                GameManager.I.RefreshAlarmGraph();
            }
        }
    }
}
