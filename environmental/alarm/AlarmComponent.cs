using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmComponent : GraphNodeComponent<AlarmComponent, AlarmNode> {
    protected bool _alarmTriggered;
    public float countdownTimer;
    public CyberComponent cyberComponent;
    public PoweredComponent powerComponent;
    public PulseLight pulseLight;
    private TamperEvidence sensorTripIndicator;
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
    public virtual void Start() {
        if (pulseLight != null) {
            GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/tamperEvidence"), pulseLight.transform.position, Quaternion.identity) as GameObject;
            sensorTripIndicator = obj.GetComponent<TamperEvidence>();
            sensorTripIndicator.suspicious = false;
            sensorTripIndicator.reportText = "HQ, respond. An alarm sensor was tripped.";
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
            bool dirty = _alarmTriggered != value;
            _alarmTriggered = value;
            if (value) {
                countdownTimer = 30f;
            }
            if (pulseLight != null) {
                pulseLight.doPulse = value;
            }
            if (sensorTripIndicator != null) {
                sensorTripIndicator.suspicious = value;
            }
            OnStateChange?.Invoke(this);
        }
    }

    public override AlarmNode GetNode() => GameManager.I.GetAlarmNode(idn);

    public void OnPowerChange(PoweredComponent node) {
        // Debug.Log($"alarm component {this} on power change powered: {node.power}");
        powerPowered = node.power;
        ApplyCyberPowerState();
    }

    public void OnCyberChange(CyberComponent node) {
        // Debug.Log($"alarm component {this} on cyber change compromised: {node.compromised}");
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

#if UNITY_EDITOR
    protected override void OnDrawGizmos() {
        foreach (AlarmComponent other in edges) {
            if (other == null)
                continue;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(NodePosition(), other.NodePosition());
        }
    }
#endif
}
