using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmComponent : GraphNodeComponent<AlarmComponent, AlarmNode> {
    protected bool _alarmTriggered;
    public float countdownTimer;
    public CyberComponent cyberComponent;
    public PoweredComponent powerComponent;

    public override void Start() {
        base.Start();
        if (cyberComponent != null) {
            cyberComponent.OnStateChange += OnCyberChange;
        }
        if (powerComponent != null) {
            powerComponent.OnStateChange += OnPowerChange;
        }
    }
    public override void OnDestroy() {
        base.OnDestroy();
        if (cyberComponent != null) {
            cyberComponent.OnStateChange -= OnCyberChange;
        }
        if (powerComponent != null) {
            powerComponent.OnStateChange -= OnPowerChange;
        }
    }
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

    public void OnPowerChange(PoweredComponent node) {
        if (!node.power) {
            DisableSource();
        }
    }

    public void OnCyberChange(CyberComponent node) {
        if (node.compromised) {
            DisableSource();
        }
    }
}
