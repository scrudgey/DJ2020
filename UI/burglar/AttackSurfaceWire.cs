using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackSurfaceWire : MonoBehaviour, INodeBinder<PowerNode>, INodeBinder<CyberNode>, INodeBinder<AlarmNode> {
    PowerNode INodeBinder<PowerNode>.node { get; set; }
    CyberNode INodeBinder<CyberNode>.node { get; set; }
    AlarmNode INodeBinder<AlarmNode>.node { get; set; }

    public bool disablePowerNode;
    public bool disableCyberNode;
    public bool disableAlarmNode;
    public bool activateAlarmNode;
    public SecurityCamera securityCamera;
    public string resultText;
    public bool electricalDamage;
    public BurglarAttackResult DoCut() {
        BurglarAttackResult result = BurglarAttackResult.None;
        if (electricalDamage) {
            PowerNode powerNode = ((INodeBinder<PowerNode>)this).node;
            bool doZap = powerNode == null || powerNode.getEnabled();
            if (doZap) {
                result = result with {
                    success = false,
                    electricDamage = new ElectricalDamage(10f, Vector3.up, transform.position, transform.position)
                };
            } else {
                result = result with {
                    success = true,
                    makeTamperEvidenceSuspicious = true,
                    tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
                };
            }
            return result;
        }

        if (disableAlarmNode) {
            AlarmNode alarmNode = ((INodeBinder<AlarmNode>)this).node;
            GameManager.I.SetNodeEnabled(alarmNode, false);
            result = result with {
                success = true,
                feedbackText = resultText,
                makeTamperEvidenceSuspicious = true,
                tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
            };
        }
        if (activateAlarmNode) {
            AlarmNode alarmNode = ((INodeBinder<AlarmNode>)this).node;
            GameManager.I.SetAlarmNodeTriggered(alarmNode, true);
            result = result with {
                success = true,
                feedbackText = resultText,
                makeTamperEvidenceSuspicious = true,
                tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
            };
        }
        if (disablePowerNode) {
            PowerNode powerNode = ((INodeBinder<PowerNode>)this).node;
            GameManager.I.SetNodeEnabled(powerNode, false);
            result = result with {
                success = true,
                feedbackText = resultText,
                makeTamperEvidenceSuspicious = true,
                tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
            };
        }
        if (securityCamera != null) {
            securityCamera.doRotate = false;
            result = result with {
                success = true,
                feedbackText = resultText,
                makeTamperEvidenceSuspicious = true,
                tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
            };
        }
        return result;
    }

    void INodeBinder<CyberNode>.HandleNodeChange() {

    }
    void INodeBinder<PowerNode>.HandleNodeChange() {

    }
    void INodeBinder<AlarmNode>.HandleNodeChange() {

    }
}
