using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmComponent : GraphNodeComponent<AlarmComponent, AlarmNode>, INodeBinder<PowerNode>, INodeBinder<CyberNode>, INodeBinder<AlarmNode> {
    PowerNode INodeBinder<PowerNode>.node { get; set; }
    CyberNode INodeBinder<CyberNode>.node { get; set; }
    AlarmNode INodeBinder<AlarmNode>.node { get; set; }
    public bool debug;
    public AlarmNode.AlarmNodeType nodeType;
    public PulseLight pulseLight;
    private TamperEvidence sensorTripIndicator;

    void INodeBinder<PowerNode>.HandleNodeChange() {
        ApplyCyberPowerState();
    }
    void INodeBinder<CyberNode>.HandleNodeChange() {
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
        CyberNode cyberNode = ((INodeBinder<CyberNode>)this).node;
        AlarmNode alarmNode = ((INodeBinder<AlarmNode>)this).node;
        PowerNode powerNode = ((INodeBinder<PowerNode>)this).node;

        bool cyberNodeEnabled = cyberNode == null ? true : !cyberNode.compromised && cyberNode.utilityActive;
        bool alarmNodeEnabled = alarmNode == null ? true : alarmNode.enabled;
        bool powerNodeEnabled = powerNode == null ? true : powerNode.getEnabled() && powerNode.powered;

        node.overallEnabled = cyberNodeEnabled && alarmNodeEnabled && powerNodeEnabled;

        if (debug) {
            Debug.Log($"[ALARM COMPONENT] cybernode:{cyberNode}\talarmnode:{alarmNode}\tpowernode:{powerNode}");
            Debug.Log($"[ALARM COMPONENT] cyber enabled:{cyberNodeEnabled}\talarm enabled:{alarmNodeEnabled}\tpower enabled:{powerNodeEnabled}");
            if (alarmNode != null)
                Debug.Log($"[ALARM COMPONENT] alarm node overall: {alarmNode.overallEnabled} alarm node enabled: {alarmNode.enabled}");
            Debug.Log($"[ALARM COMPONENT] overall enabled: {node.overallEnabled}");
        }

        if (node.overallEnabled) {
            EnableSource();
        } else {
            DisableSource();
            GameManager.I.SetAlarmNodeTriggered(node, false);
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
