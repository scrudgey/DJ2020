using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackSurfaceWire : MonoBehaviour {
    public AlarmComponent[] alarmComponentsToDisable;
    public AlarmComponent[] alarmComponentsToActivate;
    public PoweredComponent[] poweredComponentsToDisable;
    public SecurityCamera securityCamera;
    public string resultText;
    public bool electricalDamage;
    public PoweredComponent electricalDamageSourceComponent;
    public BurglarAttackResult DoCut() {
        BurglarAttackResult result = BurglarAttackResult.None;

        if (electricalDamage) {
            bool doZap = electricalDamageSourceComponent == null || electricalDamageSourceComponent.nodeEnabled;
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

        foreach (AlarmComponent component in alarmComponentsToDisable) {
            GameManager.I.SetAlarmOverride(component);
            GameManager.I.SetNodeEnabled<AlarmComponent, AlarmNode>(component, false);
            result = result with {
                success = true,
                feedbackText = resultText,
                makeTamperEvidenceSuspicious = true,
                tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
            };
        }
        foreach (AlarmComponent component in alarmComponentsToActivate) {
            GameManager.I.SetAlarmNodeTriggered(component, true);
            result = result with {
                success = true,
                feedbackText = resultText,
                makeTamperEvidenceSuspicious = true,
                tamperEvidenceReportString = "HQ respond. Someone has tampered with the electronics."
            };
        }
        foreach (PoweredComponent component in poweredComponentsToDisable) {
            GameManager.I.SetNodeEnabled<PoweredComponent, PowerNode>(component, false);
            GameManager.I.SetPowerNodeState(component, false);
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
}
