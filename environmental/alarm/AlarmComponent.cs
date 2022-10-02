using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmComponent : GraphNodeComponent<AlarmComponent, AlarmNode> {
    protected bool _alarmTriggered;
    public float countdownTimer;
    public CyberComponent cyberComponent;
    public PoweredComponent powerComponent;
    bool cyberCompromised;
    bool powerPowered;
    public void Awake() {
        powerPowered = true;
        cyberCompromised = false;
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
        Debug.Log($"alarm component {this} on power change powered: {node.power}");
        powerPowered = node.power;
        ApplyCyberPowerState();
    }

    public void OnCyberChange(CyberComponent node) {
        Debug.Log($"alarm component {this} on cyber change compromised: {node.compromised}");
        cyberCompromised = node.compromised;
        ApplyCyberPowerState();
    }

    void ApplyCyberPowerState() {
        if (cyberCompromised || !powerPowered) {
            DisableSource();
        } else {
            EnableSource();
        }
    }
}
