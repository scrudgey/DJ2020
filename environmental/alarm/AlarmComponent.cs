using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmComponent : GraphNodeComponent<AlarmComponent, AlarmNode> {
    protected bool _alarmTriggered;
    public float countdownTimer;

    public virtual bool alarmTriggered {
        get { return _alarmTriggered; }
        set {
            _alarmTriggered = value;
            if (value) {
                countdownTimer = 30f;
            }
            OnStateChange?.Invoke(this);
        }
    }

    public override AlarmNode GetNode() => GameManager.I.GetAlarmNode(idn);
}
