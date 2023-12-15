using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AlarmNode : Node<AlarmNode> {
    public enum AlarmNodeType { normal, terminal, radio }
    public enum AlarmOverrideState { none, disabled, enabled }
    public AlarmNodeType nodeType;
    public bool alarmTriggered;
    public float countdownTimer;
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
