using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AlarmNode : Node {
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
