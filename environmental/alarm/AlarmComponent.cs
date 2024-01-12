using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmComponent : GraphNodeComponent<AlarmComponent, AlarmNode>, INodeBinder<PowerNode>, INodeBinder<CyberNode>, INodeBinder<AlarmNode> {
    PowerNode INodeBinder<PowerNode>.node { get; set; }
    CyberNode INodeBinder<CyberNode>.node { get; set; }
    AlarmNode INodeBinder<AlarmNode>.node { get; set; }

    public AlarmNode.AlarmNodeType nodeType;
    public PulseLight pulseLight;
    private TamperEvidence sensorTripIndicator;
    bool cyberCompromised;
    bool powerPowered;
    public void Awake() {
        powerPowered = true;
        cyberCompromised = false;
    }
    void INodeBinder<PowerNode>.HandleNodeChange() {
        INodeBinder<PowerNode> x = (INodeBinder<PowerNode>)this;
        powerPowered = x.node.powered;
        ApplyCyberPowerState();
    }
    void INodeBinder<CyberNode>.HandleNodeChange() {
        INodeBinder<CyberNode> x = (INodeBinder<CyberNode>)this;
        cyberCompromised = !x.node.utilityActive;
        ApplyCyberPowerState();
    }
    void INodeBinder<AlarmNode>.HandleNodeChange() {
        AlarmNode alarmnode = ((INodeBinder<AlarmNode>)this).node;
        bool alarmTriggered = alarmnode.alarmTriggered;
        if (pulseLight != null) {
            pulseLight.doPulse = alarmTriggered;
        }
        if (sensorTripIndicator != null) {
            sensorTripIndicator.suspicious = alarmTriggered;
        }
    }
    public override AlarmNode NewNode() {
        AlarmNode alarmNode = base.NewNode();
        alarmNode.nodeType = nodeType;
        return alarmNode;
    }
    public virtual void Start() {
        if (pulseLight != null) {
            GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/tamperEvidence"), pulseLight.transform.position, Quaternion.identity) as GameObject;
            sensorTripIndicator = obj.GetComponent<TamperEvidence>();
            sensorTripIndicator.suspicious = false;
            sensorTripIndicator.reportText = "HQ, respond. An alarm sensor was tripped.";
        }
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
